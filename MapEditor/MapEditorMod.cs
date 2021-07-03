using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Jotunn.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;
using UnboundLib;
using Photon.Pun;
using Sirenix.Serialization;

namespace MapEditor
{
    [BepInDependency("com.willis.rounds.unbound", "2.1.4")]
    [BepInPlugin(ModId, "MapEditor", Version)]
    public class MapEditorMod : BaseUnityPlugin
    {
        private const string ModId = "io.olavim.rounds.mapeditor";
        public const string Version = "1.0.0";

        public static MapEditorMod instance;

        public Dictionary<string, GameObject> mapObjects = new Dictionary<string, GameObject>();
        public bool editorActive = false;

        private int newMapID = -1;

        public void Awake()
        {
            MapEditorMod.instance = this;

            var harmony = new Harmony(MapEditorMod.ModId);
            harmony.PatchAll();

            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if (mode == LoadSceneMode.Single)
                {
                    this.newMapID = -1;
                    this.editorActive = false;
                }
            };

            AssetUtils.LoadAssetBundleFromResources("mapeditor", typeof(MapEditorMod).Assembly);
            var objectBundle = AssetUtils.LoadAssetBundleFromResources("mapobjects", typeof(MapEditorMod).Assembly);

            this.RegisterMapObject<Transformers.BoxTransformer>("Ground", objectBundle.LoadAsset<GameObject>("Ground"));
            this.RegisterMapObject<Transformers.BoxTransformer>("Box", Resources.Load<GameObject>("4 Map Objects/Box"));
            this.RegisterMapObject<Transformers.BoxTransformer>("Destructible Box", Resources.Load<GameObject>("4 Map Objects/Box_Destructible"));
            this.RegisterMapObject<Transformers.BoxTransformer>("Background Box", Resources.Load<GameObject>("4 Map Objects/Box_BG"));
            this.RegisterMapObject<Transformers.SawTransformer>("Saw", Resources.Load<GameObject>("4 Map Objects/MapObject_Saw_Stat"));
        }

        private void RegisterMapObject<TTrans>(string name, GameObject prefab) where TTrans : Component
        {
            this.mapObjects.Add(name, prefab);
            prefab.AddComponent<TTrans>();

            var obj = prefab.AddComponent<MapObject>();
            obj.mapObjectName = name;
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                if (this.newMapID == -1)
                {
                    this.InsertNewMap();
                }

                this.StartCoroutine(this.AddEditorOnLevelLoad());
                MapManager.instance.LoadLevelFromID(this.newMapID);
                this.editorActive = true;
            }
        }

        private void InsertNewMap()
        {
            this.newMapID = MapManager.instance.levels.Length;
            var list = MapManager.instance.levels.ToList();
            list.Add("NewMap");
            MapManager.instance.levels = list.ToArray();
        }

        private IEnumerator AddEditorOnLevelLoad()
        {
            while (MapManager.instance.currentLevelID != this.newMapID)
            {
                yield return null;
            }

            var go = MapManager.instance.currentMap.Map.gameObject;
            go.transform.position = Vector3.zero;

            if (!go.GetComponent<MapEditor>())
            {
                go.AddComponent<MapEditor>();
            }
        }

        public void AddSpawn(GameObject map, SpawnPointData data = null)
        {
            if (data == null)
            {
                int id = map.GetComponentsInChildren<SpawnPoint>().Length;
                int teamID = id % 2;
                data = new SpawnPointData(id, teamID, Vector3.zero);
            }

            var spawnGo = new GameObject($"SPAWN POINT {data.id}");
            spawnGo.transform.SetParent(map.transform);
            spawnGo.transform.position = data.position;

            var spawn = spawnGo.AddComponent<SpawnPoint>();
            spawn.ID = data.id;
            spawn.TEAMID = data.teamID;

            if (this.editorActive)
            {
                spawnGo.AddComponent<Visualizers.SpawnVisualizer>();
            }
        }

        public GameObject SpawnObject(GameObject prefab, GameObject map)
        {
            GameObject instance;

            if (this.editorActive && prefab.GetComponent<PhotonMapObject>())
            {
                instance = PhotonNetwork.Instantiate($"4 Map Objects/{prefab.name}", Vector3.zero, Quaternion.identity);
            }
            else
            {
                instance = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, map.transform);
            }

            instance.name = prefab.name;
            this.SetupMapObject(instance, map);

            if (this.editorActive)
            {
                this.ExecuteAfterFrames(1, () =>
                {
                    var rig = instance.GetComponent<Rigidbody2D>();
                    if (rig)
                    {
                        rig.simulated = true;
                        rig.isKinematic = true;
                    }

                    this.ResetAnimations(map.gameObject);
                });
            }

            return instance;
        }

        public void ResetAnimations(GameObject go)
        {
            var codeAnimation = go.GetComponent<CodeAnimation>();
            if (codeAnimation)
            {
                codeAnimation.PlayIn();
            }

            var curveAnimation = go.GetComponent<CurveAnimation>();
            if (curveAnimation)
            {
                curveAnimation.PlayIn();
            }

            foreach (Transform child in go.transform)
            {
                this.ResetAnimations(child.gameObject);
            }
        }

        private void SetupMapObject(GameObject go, GameObject map)
        {
            if (go.GetComponent<CodeAnimation>())
            {
                var originalPosition = go.transform.position;
                var originalScale = go.transform.localScale;

                var wrapper = new GameObject(go.name + "Wrapper");
                wrapper.transform.SetParent(map.transform);
                go.transform.SetParent(wrapper.transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;

                wrapper.transform.position = originalPosition;
                wrapper.transform.localScale = originalScale;

                // Offset object to snap top left corner instead of center
                var scale = wrapper.transform.localScale;
                wrapper.transform.position += new Vector3(scale.x / 2f, -scale.y / 2f, 0);
            }
            else
            {
                // Offset object to snap top left corner instead of center
                var scale = go.transform.localScale;
                go.transform.position += new Vector3(scale.x / 2f, -scale.y / 2f, 0);
            }

            // The Map component normally sets the renderers and masks, but only on load
            var renderer = go.GetComponent<SpriteRenderer>();
            if (renderer && renderer.color.a >= 0.5f)
            {
                renderer.transform.position = new Vector3(renderer.transform.position.x, renderer.transform.position.y, -3f);
                if (renderer.gameObject.tag != "NoMask")
                {
                    renderer.color = new Color(0.21568628f, 0.21568628f, 0.21568628f);
                    if (!renderer.GetComponent<SpriteMask>())
                    {
                        renderer.gameObject.AddComponent<SpriteMask>().sprite = renderer.sprite;
                    }
                }
            }

            var mask = go.GetComponent<SpriteMask>();
            if (mask && mask.gameObject.tag != "NoMask")
            {
                mask.isCustomRangeActive = true;
                mask.frontSortingLayerID = SortingLayer.NameToID("MapParticle");
                mask.frontSortingOrder = 1;
                mask.backSortingLayerID = SortingLayer.NameToID("MapParticle");
                mask.backSortingOrder = 0;
            }
        }

        public void SpawnMap(Map map)
        {
            var bytes = File.ReadAllBytes("map.json");
            var mapData = SerializationUtility.DeserializeValue<CustomMap>(bytes, DataFormat.JSON);

            foreach (var mapObject in mapData.mapObjects)
            {
                var prefab = MapEditorMod.instance.mapObjects[mapObject.mapObjectName];
                var instance = MapEditorMod.instance.SpawnObject(prefab, map.gameObject);
                instance.transform.position = mapObject.position;
                instance.transform.localScale = mapObject.scale;
                instance.transform.rotation = mapObject.rotation;
            }

            foreach (var spawn in mapData.spawns)
            {
                MapEditorMod.instance.AddSpawn(map.gameObject, spawn);
            }
        }
    }

    [HarmonyPatch(typeof(ArtHandler), "Update")]
    class ArtHandlerPatch
    {
        public static bool Prefix()
        {
            return !MapEditorMod.instance.editorActive;
        }
    }

    [HarmonyPatch(typeof(MapManager), "RPCA_LoadLevel")]
    class MapManagerPatch
    {
        private static void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= MapManagerPatch.OnLevelFinishedLoading;
            Map map = scene.GetRootGameObjects().Select(obj => obj.GetComponent<Map>()).Where(m => m != null).FirstOrDefault();
            MapEditorMod.instance.SpawnMap(map);
        }

        public static void Prefix(ref string sceneName)
        {
            if (sceneName != null)
            {
                sceneName = "NewMap";
                SceneManager.sceneLoaded += MapManagerPatch.OnLevelFinishedLoading;
            }
        }
    }
}

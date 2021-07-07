using System.IO;
using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;
using UnboundLib;

namespace MapsExtended.Editor
{
    [BepInDependency("com.willis.rounds.unbound", "2.2.0")]
    [BepInDependency("io.olavim.rounds.mapsextended", "1.0.0")]
    [BepInPlugin(ModId, "MapsExtended.Editor", Version)]
    public class EditorMod : BaseUnityPlugin
    {
        private const string ModId = "io.olavim.rounds.mapsextended.editor";
        public const string Version = "1.0.0";

        public static EditorMod instance;

        public bool editorActive = false;

        public void Awake()
        {
            EditorMod.instance = this;

            var harmony = new Harmony(EditorMod.ModId);
            harmony.PatchAll();

            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if (mode == LoadSceneMode.Single)
                {
                    this.editorActive = false;
                }
            };

            Directory.CreateDirectory(Path.Combine(BepInEx.Paths.GameRootPath, "maps"));
        }

        public void Start()
        {
            MapObjectManager.instance.RegisterMapObjectComponent<BoxActionHandler>("Ground", c => c.enabled = this.editorActive);
            MapObjectManager.instance.RegisterMapObjectComponent<BoxActionHandler>("Box", c => c.enabled = this.editorActive);
            MapObjectManager.instance.RegisterMapObjectComponent<BoxActionHandler>("Destructible Box", c => c.enabled = this.editorActive);
            MapObjectManager.instance.RegisterMapObjectComponent<BoxActionHandler>("Background Box", c => c.enabled = this.editorActive);
            MapObjectManager.instance.RegisterMapObjectComponent<BoxActionHandler>("Saw", c => c.enabled = this.editorActive);
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                this.editorActive = true;
                MapManager.instance.RPCA_LoadLevel("NewMap");
                SceneManager.sceneLoaded += this.AddEditorOnLevelLoad;
            }
        }

        private void AddEditorOnLevelLoad(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= this.AddEditorOnLevelLoad;

            var go = MapManager.instance.currentMap.Map.gameObject;
            go.transform.position = Vector3.zero;

            if (!go.GetComponent<MapEditor>())
            {
                go.AddComponent<MapEditor>();
            }
        }

        public void LoadMap(Map mapBase, string mapFilePath)
        {
            MapsExtended.LoadMap(mapBase, mapFilePath, true);

            this.ExecuteAfterFrames(1, () =>
            {
                var mapObjects = mapBase.GetComponentsInChildren<MapObject>();
                var spawns = mapBase.GetComponentsInChildren<SpawnPoint>();

                foreach (var mapObject in mapObjects)
                {
                    this.SetupMapObject(mapBase, mapObject.gameObject);
                }

                foreach (var spawn in spawns)
                {
                    this.SetupSpawn(spawn.gameObject);
                }
            });
        }

        public GameObject SpawnObject(Map map, string mapObjectName)
        {
            GameObject instance = MapsExtended.SpawnMapObject(map, mapObjectName, true);
            this.SetupMapObject(map, instance);
            return instance;
        }

        public GameObject AddSpawn(Map map)
        {
            var spawn = MapsExtended.AddSpawn(map);
            this.SetupSpawn(spawn);
            return spawn;
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

        private void SetupSpawn(GameObject spawn)
        {
            spawn.AddComponent<Visualizers.SpawnVisualizer>();
            spawn.AddComponent<SpawnActionHandler>();
        }

        private void SetupMapObject(Map map, GameObject go)
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

            this.ExecuteAfterFrames(1, () =>
            {
                var rig = go.GetComponent<Rigidbody2D>();
                if (rig)
                {
                    rig.simulated = true;
                    rig.isKinematic = true;
                }

                this.ResetAnimations(map.gameObject);
            });
        }
    }

    [HarmonyPatch(typeof(ArtHandler), "Update")]
    class ArtHandlerPatch
    {
        public static bool Prefix()
        {
            return !EditorMod.instance.editorActive;
        }
    }
}

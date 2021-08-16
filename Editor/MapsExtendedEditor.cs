using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;
using UnboundLib;
using MapsExt.MapObjects;

namespace MapsExt.Editor
{
    [BepInDependency("com.willis.rounds.unbound", "2.2.0")]
    [BepInDependency("io.olavim.rounds.mapsextended", "1.0.0")]
    [BepInPlugin(ModId, "MapsExtended.Editor", Version)]
    public class MapsExtendedEditor : BaseUnityPlugin
    {
        private const string ModId = "io.olavim.rounds.mapsextended.editor";
        public const string Version = "1.0.0";

        public static MapsExtendedEditor instance;

        public bool editorActive = false;

        internal MapObjectManager mapObjectManager;

        public void Awake()
        {
            MapsExtendedEditor.instance = this;

            var harmony = new Harmony(MapsExtendedEditor.ModId);
            harmony.PatchAll();

            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if (mode == LoadSceneMode.Single)
                {
                    this.editorActive = false;
                }
            };

            Directory.CreateDirectory(Path.Combine(BepInEx.Paths.GameRootPath, "maps"));

            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Assembly.LoadFrom($"{assemblyDir}{Path.DirectorySeparatorChar}MapsExtended.Editor.UI.dll");

            this.mapObjectManager = this.gameObject.AddComponent<MapObjectManager>();
            this.mapObjectManager.NetworkID = $"{ModId}/RootMapObjectManager";

            this.RegisterMapObjects(Assembly.GetExecutingAssembly());
        }

        public void RegisterMapObjects(Assembly assembly)
        {
            var types = assembly.GetTypes();
            var typesWithAttribute = types.Where(t => t.GetCustomAttribute<MapsExtendedEditorMapObject>() != null);

            foreach (var type in typesWithAttribute)
            {
                try
                {
                    var attr = type.GetCustomAttribute<MapsExtendedEditorMapObject>();
                    var instance = (MapObjectSpecification) AccessTools.CreateInstance(type);
                    this.mapObjectManager.RegisterSpecification(attr.dataType, instance);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError(ex);
                }
            }
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                // Prevents Landfall's own map testing stuff from triggering
                GameManager.instance.isPlaying = true;

                this.editorActive = true;
                MapManager.instance.RPCA_LoadLevel("NewMap");
                SceneManager.sceneLoaded += this.AddEditorOnLevelLoad;
            }
        }

        private void AddEditorOnLevelLoad(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= this.AddEditorOnLevelLoad;

            var map = MapManager.instance.currentMap.Map;
            var go = map.gameObject;
            go.transform.position = Vector3.zero;

            if (!go.GetComponent<MapEditor>())
            {
                go.AddComponent<MapEditor>();
            }

            MapManager.instance.isTestingMap = true;
            GameObject.Find("Game/UI/UI_MainMenu").gameObject.SetActive(false);
            GameObject.Find("Game").GetComponent<SetOfflineMode>().SetOffline();
            map.hasEntered = true;

            ArtHandler.instance.NextArt();
        }

        public void LoadMap(GameObject container, string mapFilePath)
        {
            MapsExtended.LoadMap(container, mapFilePath, this.mapObjectManager);

            this.ExecuteAfterFrames(1, () =>
            {
                var mapObjects = container.GetComponentsInChildren<MapObjectInstance>();

                foreach (var mapObject in mapObjects)
                {
                    this.SetupMapObject(container, mapObject.gameObject);
                }

                this.SetMapPhysicsActive(container, false);
            });
        }

        public void SpawnObject(GameObject container, MapObject data, Action<GameObject> cb)
        {
            this.mapObjectManager.Instantiate(data, container.transform, instance =>
            {
                this.SetupMapObject(container, instance);

                var rig = instance.GetComponent<Rigidbody2D>();
                if (rig)
                {
                    this.ExecuteAfterFrames(1, () => this.SetPhysicsActive(rig, false));
                }

                cb(instance);
            });
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

        private void SetupMapObject(GameObject container, GameObject go)
        {
            if (go.GetComponent<CodeAnimation>())
            {
                var originalPosition = go.transform.position;
                var originalScale = go.transform.localScale;

                var wrapper = new GameObject(go.name + "Wrapper");
                wrapper.transform.SetParent(container.transform);
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
                this.ResetAnimations(container);
            });
        }

        public void SetMapPhysicsActive(GameObject container, bool active)
        {
            var rigs = container.GetComponentsInChildren<Rigidbody2D>();
            foreach (var rig in rigs)
            {
                this.SetPhysicsActive(rig, active);
            }
        }

        private void SetPhysicsActive(Rigidbody2D rig, bool active)
        {
            rig.velocity = Vector2.zero;
            rig.angularVelocity = 0;
            rig.simulated = true;
            rig.isKinematic = !active;
        }
    }

    [HarmonyPatch(typeof(ArtHandler), "Update")]
    class ArtHandlerPatch
    {
        public static bool Prefix()
        {
            return !MapsExtendedEditor.instance.editorActive;
        }
    }
}

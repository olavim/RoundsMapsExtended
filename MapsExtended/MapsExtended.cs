using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using Jotunn.Utils;
using UnityEngine.SceneManagement;
using Sirenix.Serialization;
using UnityEngine;
using UnboundLib;
using Photon.Pun;
using System.Collections;
using MapsExtended.MapObjects;

namespace MapsExtended
{
    [BepInDependency("com.willis.rounds.unbound", "2.2.0")]
    [BepInPlugin(ModId, "MapsExtended", Version)]
    public class MapsExtended : BaseUnityPlugin
    {
        private const string ModId = "io.olavim.rounds.mapsextended";
        public const string Version = "1.0.0";

#if DEBUG
        public static readonly bool DEBUG = true;
#else
        public static readonly bool DEBUG = false;
#endif

        public static MapsExtended instance;

        public MapObjectManager mapObjectManager;
        public List<string> mapFiles;
        public bool forceCustomMaps = false;
        public string loadedMapName;
        public string loadedMapSceneName;

        internal Dictionary<string, string> mapFolderPrefixes = new Dictionary<string, string>();
        internal Dictionary<PhotonMapObject, Action<GameObject>> photonInstantiationListeners = new Dictionary<PhotonMapObject, Action<GameObject>>();

        public void Awake()
        {
            MapsExtended.instance = this;
            new Harmony(MapsExtended.ModId).PatchAll();

            AssetUtils.LoadAssetBundleFromResources("mapbase", typeof(MapsExtended).Assembly);

            this.mapObjectManager = this.gameObject.AddComponent<MapObjectManager>();
            this.mapObjectManager.NetworkID = $"{ModId}/RootMapObjectManager";

            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if (mode == LoadSceneMode.Single)
                {
                    this.UpdateMapFiles();
                }
            };

            this.mapFolderPrefixes.Add("0", BepInEx.Paths.PluginPath);
            this.mapFolderPrefixes.Add("1", BepInEx.Paths.GameRootPath);

            this.RegisterMapObjects(Assembly.GetExecutingAssembly());
        }

        public void Start()
        {
            this.UpdateMapFiles();

            if (MapsExtended.DEBUG)
            {
                Unbound.RegisterGUI("MapsExtended Debug", this.DrawDebugGUI);
            }
        }

        public void RegisterMapObjects(Assembly assembly)
        {
            var types = assembly.GetTypes();
            var typesWithAttribute = types.Where(t => t.GetCustomAttribute<MapsExtendedMapObject>() != null);

            foreach (var type in typesWithAttribute)
            {
                try
                {
                    var attr = type.GetCustomAttribute<MapsExtendedMapObject>();
                    var instance = (MapObjectSpecification) AccessTools.CreateInstance(type);
                    this.mapObjectManager.RegisterSpecification(attr.dataType, instance);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError(ex);
                }
            }
        }

        public void DrawDebugGUI()
        {
            this.forceCustomMaps = GUILayout.Toggle(this.forceCustomMaps, "Force Custom Maps");
        }

        public void UpdateMapFiles()
        {
            var pluginPaths = Directory.GetFiles(BepInEx.Paths.PluginPath, "*.map", SearchOption.AllDirectories);
            var rootPaths = Directory.GetFiles(Path.Combine(BepInEx.Paths.GameRootPath, "maps"), "*.map", SearchOption.AllDirectories);

            this.mapFiles = new List<string>();
            this.mapFiles.AddRange(pluginPaths.Select(p => "0:" + p.Replace(BepInEx.Paths.PluginPath, "")));
            this.mapFiles.AddRange(rootPaths.Select(p => "1:" + p.Replace(BepInEx.Paths.GameRootPath, "")));

            Logger.LogMessage($"Loaded {mapFiles.Count} custom maps");

            Unbound.RegisterMaps(this.mapFiles);
        }

        public void OnPhotonMapObjectInstantiate(PhotonMapObject mapObject, Action<GameObject> callback)
        {
            this.photonInstantiationListeners.Add(mapObject, callback);
        }

        public static void LoadMap(GameObject container, string mapFilePath, MapObjectManager mapObjectManager, Action onLoad = null)
        {
            var bytes = File.ReadAllBytes(mapFilePath);
            var mapData = SerializationUtility.DeserializeValue<CustomMap>(bytes, DataFormat.JSON);
            MapsExtended.LoadMap(container, mapData, mapObjectManager, onLoad);
        }

        public static void LoadMap(GameObject container, CustomMap mapData, MapObjectManager mapObjectManager, Action onLoad = null)
        {
            MapsExtended.instance.StartCoroutine(MapsExtended.LoadMapCoroutine(container, mapData, mapObjectManager, onLoad));
        }

        private static IEnumerator LoadMapCoroutine(GameObject container, CustomMap mapData, MapObjectManager mapObjectManager, Action onLoad = null)
        {
            foreach (Transform child in container.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            int toLoad = mapData.mapObjects.Count;

            foreach (var mapObject in mapData.mapObjects)
            {
                mapObjectManager.Instantiate(mapObject, container.transform, instance => toLoad--);
            }

            while (toLoad > 0)
            {
                yield return null;
            }

            onLoad?.Invoke();
        }
    }

    [HarmonyPatch(typeof(MapManager), "RPCA_LoadLevel")]
    class MapManagerPatch_LoadLevel
    {
        private static void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
        {
            if (MapManager.instance.currentMap != null)
            {
                MapManager.instance.currentMap.Map.wasSpawned = false;
            }

            SceneManager.sceneLoaded -= MapManagerPatch_LoadLevel.OnLevelFinishedLoading;
            Map map = scene.GetRootGameObjects().Select(obj => obj.GetComponent<Map>()).Where(m => m != null).FirstOrDefault();
            MapsExtended.LoadMap(map.gameObject, MapsExtended.instance.loadedMapName, MapsExtended.instance.mapObjectManager);
        }

        public static void Prefix(ref string sceneName)
        {
            if (sceneName != null && sceneName.EndsWith(".map"))
            {
                string prefix = sceneName.Split(':')[0];
                string filename = sceneName.Substring(prefix.Length + 1);
                string basePath = MapsExtended.instance.mapFolderPrefixes[prefix];

                MapsExtended.instance.loadedMapName = basePath + filename;
                MapsExtended.instance.loadedMapSceneName = sceneName;

                sceneName = "NewMap";
                SceneManager.sceneLoaded += MapManagerPatch_LoadLevel.OnLevelFinishedLoading;
            }
        }
    }

    [HarmonyPatch(typeof(MapManager), "GetIDFromScene")]
    class MapManagerPatch_GetIDFromScene
    {
        public static bool Prefix(Scene scene, MapManager __instance, ref int __result)
        {
            if (scene.name == "NewMap")
            {
                __result = __instance.levels.ToList().IndexOf(MapsExtended.instance.loadedMapSceneName);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(MapManager), "GetRandomMap")]
    class MapManagerDebugPatch
    {
        public static bool Prefix(ref string __result)
        {
            if (!MapsExtended.instance.forceCustomMaps)
            {
                return true;
            }

            var customMaps = MapsExtended.instance.mapFiles;

            int index = UnityEngine.Random.Range(0, customMaps.Count);
            __result = customMaps[index];
            return false;
        }
    }

    [HarmonyPatch(typeof(PhotonMapObject), "Update")]
    class PhotonMapObjectPatch
    {
        public static void OnPhotonInstantiate(GameObject instance, PhotonMapObject mapObject)
        {
            MapsExtended.instance.photonInstantiationListeners.TryGetValue(mapObject, out Action<GameObject> listener);
            if (listener != null)
            {
                listener(instance);
                MapsExtended.instance.photonInstantiationListeners.Remove(mapObject);
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            /* The PhotonMapObject instantiates a networked copy of itself in the Update method. Here we basically change
             * `PhotonNetwork.Instantiate(...)` to `OnPhotonInstantiate(PhotonNetwork.Instantiate(...), this)`.
             */
            var list = instructions.ToList();
            var newInstructions = new List<CodeInstruction>();

            var m_instantiate = ExtensionMethods.GetMethodInfo(typeof(PhotonNetwork), "Instantiate");

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Calls(m_instantiate))
                {
                    newInstructions.Add(list[i]);
                    newInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    newInstructions.Add(CodeInstruction.Call(typeof(PhotonMapObjectPatch), "OnPhotonInstantiate"));
                    i++;
                }
                else
                {
                    newInstructions.Add(list[i]);
                }
            }

            return newInstructions;
        }
    }
}

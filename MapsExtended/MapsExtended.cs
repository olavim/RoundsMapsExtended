using System.IO;
using System.Linq;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using Jotunn.Utils;
using UnityEngine.SceneManagement;
using Sirenix.Serialization;
using UnityEngine;
using UnboundLib;

namespace MapsExtended
{
    [BepInDependency("com.willis.rounds.unbound", "2.1.4")]
    [BepInPlugin(ModId, "MapsExtended", Version)]
    public class MapsExtended : BaseUnityPlugin
    {
        private const string ModId = "io.olavim.rounds.mapsextended";
        public const string Version = "1.0.0";

        public static MapsExtended instance;

        public List<string> mapFiles;

        public void Awake()
        {
            MapsExtended.instance = this;
            new Harmony(MapsExtended.ModId).PatchAll();

            AssetUtils.LoadAssetBundleFromResources("mapbase", typeof(MapsExtended).Assembly);

            this.gameObject.AddComponent<MapObjectManager>();

            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if (mode == LoadSceneMode.Single)
                {
                    this.UpdateMapFiles();
                }
            };
        }

        public void Start()
        {
            this.UpdateMapFiles();
        }

        public void UpdateMapFiles()
        {
            this.mapFiles = new List<string>();
            var paths = Directory.GetFiles(BepInEx.Paths.PluginPath, "*.map", SearchOption.AllDirectories);
            this.mapFiles.AddRange(paths.Select(p => p.Replace(BepInEx.Paths.PluginPath, "")));

            Logger.LogMessage($"Loaded {mapFiles.Count} custom maps");

            Unbound.RegisterMaps(this.mapFiles);
        }

        public void SpawnMap(Map mapBase, string mapFilePath)
        {
            var bytes = File.ReadAllBytes(BepInEx.Paths.PluginPath + mapFilePath);
            var mapData = SerializationUtility.DeserializeValue<CustomMap>(bytes, DataFormat.JSON);

            foreach (var mapObject in mapData.mapObjects)
            {
                var prefab = MapObjectManager.instance.GetMapObject(mapObject.mapObjectName);
                var instance = GameObject.Instantiate(prefab, mapBase.transform);
                instance.name = prefab.name;
                instance.transform.position = mapObject.position;
                instance.transform.localScale = mapObject.scale;
                instance.transform.rotation = mapObject.rotation;
            }

            foreach (var spawn in mapData.spawns)
            {
                this.AddSpawn(mapBase.gameObject, spawn);
            }
        }

        public GameObject AddSpawn(GameObject map, SpawnPointData data = null)
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

            return spawnGo;
        }
    }

    [HarmonyPatch(typeof(MapManager), "RPCA_LoadLevel")]
    class MapManagerPatch
    {
        private static string mapToLoad;

        private static void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= MapManagerPatch.OnLevelFinishedLoading;
            Map map = scene.GetRootGameObjects().Select(obj => obj.GetComponent<Map>()).Where(m => m != null).FirstOrDefault();
            MapsExtended.instance.SpawnMap(map, MapManagerPatch.mapToLoad);
        }

        public static void Prefix(ref string sceneName)
        {
            if (sceneName != null && sceneName.EndsWith(".map"))
            {
                MapManagerPatch.mapToLoad = sceneName;
                sceneName = "NewMap";
                SceneManager.sceneLoaded += MapManagerPatch.OnLevelFinishedLoading;
            }
        }
    }
}

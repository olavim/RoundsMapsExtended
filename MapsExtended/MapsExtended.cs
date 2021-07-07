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
using Photon.Pun;

namespace MapsExtended
{
    [BepInDependency("com.willis.rounds.unbound", "2.2.0")]
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
            var pluginPaths = Directory.GetFiles(BepInEx.Paths.PluginPath, "*.map", SearchOption.AllDirectories);
            var rootPaths = Directory.GetFiles(Path.Combine(BepInEx.Paths.GameRootPath, "maps"), "*.map", SearchOption.AllDirectories);

            this.mapFiles = new List<string>();
            this.mapFiles.AddRange(pluginPaths.Select(p => p.Replace(BepInEx.Paths.GameRootPath, "")));
            this.mapFiles.AddRange(rootPaths.Select(p => p.Replace(BepInEx.Paths.GameRootPath, "")));

            Logger.LogMessage($"Loaded {mapFiles.Count} custom maps");

            Unbound.RegisterMaps(this.mapFiles);
        }

        public static void LoadMap(Map mapBase, string mapFilePath, bool isDelayedLoad = false)
        {
            var bytes = File.ReadAllBytes(BepInEx.Paths.GameRootPath + mapFilePath);
            var mapData = SerializationUtility.DeserializeValue<CustomMap>(bytes, DataFormat.JSON);

            foreach (Transform child in mapBase.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            foreach (var mapObject in mapData.mapObjects)
            {
                var instance = MapsExtended.SpawnMapObject(mapBase, mapObject.mapObjectName, isDelayedLoad);
                instance.transform.position = mapObject.position;
                instance.transform.localScale = mapObject.scale;
                instance.transform.rotation = mapObject.rotation;
            }

            foreach (var spawn in mapData.spawns)
            {
                MapsExtended.AddSpawn(mapBase, spawn);
            }
        }

        public static GameObject SpawnMapObject(Map map, string mapObjectName, bool isDelayedSpawn = false)
        {
            var prefab = MapObjectManager.instance.GetMapObject(mapObjectName);
            GameObject instance;

            if (isDelayedSpawn && prefab.GetComponent<PhotonMapObject>())
            {
                instance = PhotonNetwork.Instantiate($"4 Map Objects/{prefab.name}", Vector3.zero, Quaternion.identity);
            }
            else
            {
                instance = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, map.transform);
            }

            instance.SetActive(false);
            MapObjectManager.instance.AddMapObjectComponents(mapObjectName, instance);
            instance.SetActive(true);

            instance.name = prefab.name;
            return instance;
        }

        public static GameObject AddSpawn(Map map, SpawnPointData data = null)
        {
            if (data == null)
            {
                int id = map.gameObject.GetComponentsInChildren<SpawnPoint>().Length;
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
            MapsExtended.LoadMap(map, MapManagerPatch.mapToLoad);
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

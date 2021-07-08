﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.Emit;
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

        internal Dictionary<string, string> mapFolderPrefixes = new Dictionary<string, string>();
        internal Dictionary<PhotonMapObject, Action<GameObject>> photonInstantiationListeners = new Dictionary<PhotonMapObject, Action<GameObject>>();

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

            this.mapFolderPrefixes.Add("0", BepInEx.Paths.PluginPath);
            this.mapFolderPrefixes.Add("1", BepInEx.Paths.GameRootPath);
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
            this.mapFiles.AddRange(pluginPaths.Select(p => "0:" + p.Replace(BepInEx.Paths.PluginPath, "")));
            this.mapFiles.AddRange(rootPaths.Select(p => "1:" + p.Replace(BepInEx.Paths.GameRootPath, "")));

            Logger.LogMessage($"Loaded {mapFiles.Count} custom maps");

            foreach (string f in this.mapFiles)
            {
                UnityEngine.Debug.Log("Loaded: " + f);
            }

            Unbound.RegisterMaps(this.mapFiles);
        }

        public void OnPhotonMapObjectInstantiate(PhotonMapObject mapObject, Action<GameObject> callback)
        {
            this.photonInstantiationListeners.Add(mapObject, callback);
        }

        public static void LoadMap(Map mapBase, string mapFilePath, bool isDelayedLoad = false)
        {
            var bytes = File.ReadAllBytes(mapFilePath);
            var mapData = SerializationUtility.DeserializeValue<CustomMap>(bytes, DataFormat.JSON);

            mapBase.SetFieldValue("spawnPoints", null);

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
                /* We don't need to care about the photon instantiation dance (see below comment) when instantiating PhotonMapObjects
                 * after the map transition has already been done.
                 */
                instance = PhotonNetwork.Instantiate($"4 Map Objects/{prefab.name}", Vector3.zero, Quaternion.identity);
            }
            else
            {
                instance = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, map.transform);

                var photonMapObject = instance.GetComponent<PhotonMapObject>();
                if (photonMapObject)
                {
                    /* PhotonMapObjects (networked map objects like movable boxes) are first instantiated client-side, which is what we
                     * see during the map transition animation. After the transition is done, the client-side instance is removed and a
                     * networked (photon instantiated) version is spawned in its place. This means we need to do weird shit to get the
                     * "actual" map object instance, since the instance we get from `GameObject.Instantiate` above is in this case not
                     * the instance we care about.
                     */
                    MapsExtended.instance.OnPhotonMapObjectInstantiate(photonMapObject, networkInstance =>
                    {
                        MapObjectManager.instance.AddMapObjectComponents(mapObjectName, networkInstance);
                    });
                }
            }

            MapObjectManager.instance.AddMapObjectComponents(mapObjectName, instance);

            instance.name = prefab.name;
            return instance;
        }

        public static GameObject AddSpawn(Map map, SpawnPointData data = null)
        {
            if (data == null)
            {
                int id = map.gameObject.GetComponentsInChildren<SpawnPoint>().Length;
                int teamID = id;
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

            map.mapIsReadyAction += () =>
            {

            };
        }

        public static void Prefix(ref string sceneName)
        {
            if (sceneName != null && sceneName.EndsWith(".map"))
            {
                string prefix = sceneName.Split(':')[0];
                string filename = sceneName.Substring(prefix.Length + 1);
                string basePath = MapsExtended.instance.mapFolderPrefixes[prefix];

                MapManagerPatch.mapToLoad = basePath + filename;
                sceneName = "NewMap";
                SceneManager.sceneLoaded += MapManagerPatch.OnLevelFinishedLoading;
            }
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

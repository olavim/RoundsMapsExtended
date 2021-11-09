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
using UnboundLib.Utils.UI;
using Photon.Pun;
using System.Collections;
using MapsExt.MapObjects;
using UnboundLib.Utils;

namespace MapsExt
{
	[BepInDependency("com.willis.rounds.unbound", "2.7.3")]
	[BepInPlugin(ModId, "MapsExtended", Version)]
	public class MapsExtended : BaseUnityPlugin
	{
		private const string ModId = "io.olavim.rounds.mapsextended";
		public const string Version = "0.9.2";

#if DEBUG
		public static readonly bool DEBUG = true;
#else
		public static readonly bool DEBUG = false;
#endif

		public static MapsExtended instance;

		public MapObjectManager mapObjectManager;
		public List<CustomMap> maps;
		public bool forceCustomMaps = false;
		public CustomMap loadedMap;
		public string loadedMapSceneName;
		public Action<Assembly> RegisterMapObjectsAction;

		internal Dictionary<PhotonMapObject, Action<GameObject>> photonInstantiationListeners = new Dictionary<PhotonMapObject, Action<GameObject>>();

		public void Awake()
		{
			MapsExtended.instance = this;
			new Harmony(MapsExtended.ModId).PatchAll();

			AssetUtils.LoadAssetBundleFromResources("mapbase", typeof(MapsExtended).Assembly);

			var mapObjectManagerGo = new GameObject("Root Map Object Manager");
			DontDestroyOnLoad(mapObjectManagerGo);
			this.mapObjectManager = mapObjectManagerGo.AddComponent<MapObjectManager>();
			this.mapObjectManager.SetNetworkID($"{ModId}/RootMapObjectManager");

			SceneManager.sceneLoaded += (scene, mode) =>
			{
				if (mode == LoadSceneMode.Single)
				{
					this.UpdateMapFiles();
				}
			};

			this.RegisterMapObjectsAction += this.OnRegisterMapObjects;
		}

		public void Start()
		{
			this.RegisterMapObjects();
			this.UpdateMapFiles();

			if (MapsExtended.DEBUG)
			{
				Unbound.RegisterMenu("Maps Extended DEBUG", () => { }, this.DrawDebugGUI, null, true);
			}
		}

		public void OnDisable()
		{
			UnityEngine.Debug.Log(UnityEngine.StackTraceUtility.ExtractStackTrace());
		}

		public void RegisterMapObjects()
		{
			this.RegisterMapObjectsAction?.Invoke(Assembly.GetCallingAssembly());
		}

		private void OnRegisterMapObjects(Assembly assembly)
		{
			var types = assembly.GetTypes();
			var typesWithAttribute = types.Where(t => t.GetCustomAttribute<MapObjectSpec>() != null);

			foreach (var type in typesWithAttribute)
			{
				try
				{
					var attr = type.GetCustomAttribute<MapObjectSpec>();
					var prefab = ReflectionUtils.GetAttributedProperty<GameObject>(type, typeof(MapObjectPrefab));
					var serializer = ReflectionUtils.GetAttributedMethod<SerializerAction<MapObject>>(type, typeof(MapObjectSerializer));
					var deserializer = ReflectionUtils.GetAttributedMethod<DeserializerAction<MapObject>>(type, typeof(MapObjectDeserializer));

					if (prefab == null)
					{
						throw new Exception($"{type.Name} is not a valid map object spec: Missing prefab property");
					}

					if (serializer == null)
					{
						throw new Exception($"{type.Name} is not a valid map object spec: Missing serializer method or property");
					}

					if (deserializer == null)
					{
						throw new Exception($"{type.Name} is not a valid map object spec: Missing deserializer method or property");
					}

					// Getting methods with reflection makes it possible to call explicit interface implementations later when exact types are not known
					this.mapObjectManager.RegisterType(attr.dataType, prefab);
					this.mapObjectManager.RegisterSerializer(attr.dataType, serializer);
					this.mapObjectManager.RegisterDeserializer(attr.dataType, deserializer);
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError($"Could not register map object {type.Name}: {ex.Message}");

#if DEBUG
					UnityEngine.Debug.LogError(ex.StackTrace);
#endif
				}
			}
		}

		public void DrawDebugGUI(GameObject menu)
		{
			MenuHandler.CreateToggle(this.forceCustomMaps, "Force Custom Maps", menu, null, 30, false, Color.red);
		}

		public void UpdateMapFiles()
		{
			var pluginPaths = Directory.GetFiles(BepInEx.Paths.PluginPath, "*.map", SearchOption.AllDirectories);
			var rootPaths = Directory.GetFiles(Path.Combine(BepInEx.Paths.GameRootPath, "maps"), "*.map", SearchOption.AllDirectories);

			this.maps = new List<CustomMap>();
			this.maps.AddRange(pluginPaths.Select(MapsExtended.LoadMapData));
			this.maps.AddRange(rootPaths.Select(MapsExtended.LoadMapData));

			Logger.LogMessage($"Loaded {maps.Count} custom maps");

			IList<string> activeLevels = (IList<string>) AccessTools.Field(typeof(LevelManager), "activeLevels").GetValue(null);
			IList<string> inactiveLevels = (IList<string>) AccessTools.Field(typeof(LevelManager), "inactiveLevels").GetValue(null);
			IList<string> levelsToRedraw = (IList<string>) AccessTools.Field(typeof(ToggleLevelMenuHandler), "levelsThatNeedToRedrawn").GetValue(ToggleLevelMenuHandler.instance);
			IDictionary<string, Level> allLevels = LevelManager.levels;

			var invalidatedLevels = allLevels.Keys.Where(m => m.StartsWith("MapsExtended:")).ToArray();

			foreach (var level in invalidatedLevels)
			{
				activeLevels?.Remove(level);
				inactiveLevels?.Remove(level);
				levelsToRedraw?.Remove(level);
				allLevels.Remove(level);
			}

			LevelManager.RegisterMaps(this.maps.Select(m => "MapsExtended:" + m.id));
		}

		private static CustomMap LoadMapData(string path)
		{
			var bytes = File.ReadAllBytes(path);
			return SerializationUtility.DeserializeValue<CustomMap>(bytes, DataFormat.JSON);
		}

		public void OnPhotonMapObjectInstantiate(PhotonMapObject mapObject, Action<GameObject> callback)
		{
			this.photonInstantiationListeners.Add(mapObject, callback);
		}

		public static void LoadMap(GameObject container, string mapFilePath, MapObjectManager mapObjectManager, Action onLoad = null)
		{
			var mapData = MapsExtended.LoadMapData(mapFilePath);
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

			yield return null;
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
			MapsExtended.LoadMap(map.gameObject, MapsExtended.instance.loadedMap, MapsExtended.instance.mapObjectManager);
		}

		public static void Prefix(ref string sceneName)
		{
			if (sceneName != null && sceneName.StartsWith("MapsExtended:"))
			{
				string id = sceneName.Split(':')[1];

				MapsExtended.instance.loadedMap = MapsExtended.instance.maps.First(m => m.id == id);
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

			var customMaps = MapsExtended.instance.maps;

			int index = UnityEngine.Random.Range(0, customMaps.Count);
			__result = customMaps[index].id;
			return false;
		}
	}

	[HarmonyPatch(typeof(MapManager), "GetSpawnPoints")]
	class MapManagerPatch_GetSpawnPoints
	{
		public static void Postfix(ref SpawnPoint[] __result)
		{
			var spawns = __result.ToList();

			int playerCount = PlayerManager.instance.players.Count;
			int teamCount = PlayerManager.instance.players.Select(p => p.teamID).Distinct().Count();

			// Ensure at least one spawn exists
			if (spawns.Count == 0)
			{
				spawns.Add(new SpawnPoint()
				{
					ID = 0,
					TEAMID = 0,
					// Choose center of map as default spawn location
					localStartPos = Vector3.zero
				});
			}

			// Ensure there are at least as many spawns as there are players
			while (spawns.Count < playerCount)
			{
				var prevSpawn = spawns[spawns.Count - 1];
				int nextID = spawns.Count;
				int nextTeamID = spawns.Count % teamCount;

				/* If map has two spawns, but there are four players, then players 1 and 3 should have the same spawn location,
				 * and players 2 and 4 should have the same spawn location.
				 */
				var prevTeamSpawn = spawns.Count > teamCount ? spawns[spawns.Count - 1 - teamCount] : null;
				var nextPosition = prevTeamSpawn ? prevTeamSpawn.localStartPos : prevSpawn.localStartPos;

				spawns.Add(new SpawnPoint()
				{
					ID = nextID,
					TEAMID = nextTeamID,
					localStartPos = nextPosition
				});
			}

			__result = spawns.ToArray();
		}
	}

	[HarmonyPatch(typeof(MapTransition), "Toggle")]
	class MapTransitionPatch_Toggle
	{
		public static void Postfix(GameObject obj, bool enabled)
		{
			var anim = obj.GetComponent<MapObjectAnimation>();
			if (anim)
			{
				anim.enabled = enabled;
			}
		}
	}

	[HarmonyPatch(typeof(MapObjet_Rope), "AddJoint")]
	class RopePatch_AddJoint
	{
		public static void Postfix(MapObjet_Rope __instance, AnchoredJoint2D ___joint)
		{
			UnityEngine.Debug.Log("AddJoint");
			__instance.JointAdded(___joint);
		}
	}

	// Needed to fix collision with animated saws
	[HarmonyPatch(typeof(NetworkPhysicsObject), "BulletPush")]
	class NetworkPhysicsObject_BulletPush
	{
		public static bool Prefix(NetworkPhysicsObject __instance)
		{
			return __instance.gameObject.GetComponent<MapObjectAnimation>() == null;
		}
	}

	// Needed to fix collision with animated saws
	[HarmonyPatch(typeof(NetworkPhysicsObject), "Push")]
	class NetworkPhysicsObject_Push
	{
		public static bool Prefix(NetworkPhysicsObject __instance)
		{
			return __instance.gameObject.GetComponent<MapObjectAnimation>() == null;
		}
	}

	// Fixes saw collision with destructible boxes
	[HarmonyPatch(typeof(DamageBox), "Collide")]
	class DamageBox_Collide
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var list = instructions.ToList();
			var newInstructions = new List<CodeInstruction>();

			var f_shake = AccessTools.Field(typeof(DamageBox), "shake");
			var m_getCharacterData = AccessTools.Method(typeof(Component), "GetComponent", null, new Type[] { typeof(CharacterData) });
			var m_opImplicit = typeof(UnityEngine.Object)
				.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.First(mi => mi.Name == "op_Implicit" && mi.ReturnType == typeof(bool));

			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsLdloc() && list[i + 1].Calls(m_getCharacterData))
				{
					// Call GetComponent<CharacterData>() on a non-null component (base game bug)
					newInstructions.Add(new CodeInstruction(OpCodes.Ldloc_1));
					newInstructions.Add(list[i + 1]);
					i++;
				}
				else if (
					list[i].IsLdarg(0) &&
					list[i + 1].LoadsField(f_shake) &&
					list[i + 2].LoadsConstant(0f)
				)
				{
					// Make sure local variable `component2` is not null before using it
					newInstructions.Add(new CodeInstruction(OpCodes.Ldloc_S, 4));
					newInstructions.Add(new CodeInstruction(OpCodes.Call, m_opImplicit));
					newInstructions.Add(new CodeInstruction(OpCodes.Brfalse, list[i + 3].operand));
					newInstructions.Add(list[i]);
					newInstructions.Add(list[i + 1]);
					newInstructions.Add(list[i + 2]);
					newInstructions.Add(list[i + 3]);
					i += 3;
				}
				else
				{
					newInstructions.Add(list[i]);
				}
			}

			return newInstructions;
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

			var m_instantiate = UnboundLib.ExtensionMethods.GetMethodInfo(typeof(PhotonNetwork), "Instantiate");

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
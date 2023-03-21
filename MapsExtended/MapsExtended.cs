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
using MapsExt.MapObjects.Properties;
using UnboundLib.Utils;

namespace MapsExt
{
	[BepInDependency("com.willis.rounds.unbound", "3.2.8")]
	[BepInPlugin(ModId, ModName, ModVersion)]
	public sealed class MapsExtended : BaseUnityPlugin
	{
		public const string ModId = "io.olavim.rounds.mapsextended";
		public const string ModName = "MapsExtended";
		public const string ModVersion = ThisAssembly.Project.Version;

#if DEBUG
		public static readonly bool DEBUG = true;
#else
		public static readonly bool DEBUG = false;
#endif

		public static MapsExtended instance;

		public MapObjectManager mapObjectManager;
		public List<CustomMap> maps;
		public bool forceCustomMaps;
		public CustomMap loadedMap;
		public string loadedMapSceneName;
		public Action<Assembly> RegisterMapObjectPropertiesAction;
		public Action<Assembly> RegisterMapObjectsAction;

		internal Dictionary<PhotonMapObject, Action<GameObject>> photonInstantiationListeners = new Dictionary<PhotonMapObject, Action<GameObject>>();

		private void Awake()
		{
			MapsExtended.instance = this;
			new Harmony(MapsExtended.ModId).PatchAll();

			AssetUtils.LoadAssetBundleFromResources("mapbase", typeof(MapsExtended).Assembly);

			var mapObjectManagerGo = new GameObject("Root Map Object Manager");
			DontDestroyOnLoad(mapObjectManagerGo);
			this.mapObjectManager = mapObjectManagerGo.AddComponent<MapObjectManager>();
			this.mapObjectManager.SetNetworkID($"{ModId}/RootMapObjectManager");

			SceneManager.sceneLoaded += (_, mode) =>
			{
				if (mode == LoadSceneMode.Single)
				{
					this.UpdateMapFiles();
				}
			};

			this.RegisterMapObjectPropertiesAction += this.OnRegisterMapObjectProperties;
			this.RegisterMapObjectsAction += this.OnRegisterMapObjects;
		}

		private void Start()
		{
			this.RegisterMapObjectProperties();
			this.RegisterMapObjects();
			this.UpdateMapFiles();

			if (MapsExtended.DEBUG)
			{
				Unbound.RegisterMenu("Maps Extended DEBUG", () => { }, this.DrawDebugGUI, null, true);
			}
		}

#if DEBUG
		private void Update()
		{
			UnityEngine.Debug.developerConsoleVisible = false;
		}
#endif

		public void RegisterMapObjectProperties()
		{
			this.RegisterMapObjectPropertiesAction?.Invoke(Assembly.GetCallingAssembly());
		}

		public void RegisterMapObjects()
		{
			this.RegisterMapObjectsAction?.Invoke(Assembly.GetCallingAssembly());
		}

		private void OnRegisterMapObjectProperties(Assembly assembly)
		{
			var types = assembly.GetTypes();
			foreach (var propertyType in types.Where(t => t.GetCustomAttribute<MapObjectProperty>() != null))
			{
				try
				{
					var propertyTargetType = MapObjectUtils.GetMapObjectPropertyTargetType(propertyType);

					if (propertyTargetType == null)
					{
						throw new Exception($"Invalid serializer: {propertyType.Name} does not inherit from {typeof(IMapObjectProperty<>)}");
					}

					this.mapObjectManager.RegisterProperty(propertyTargetType, propertyType);
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError($"Could not register map object serializer {propertyType.Name}: {ex.Message}");

#if DEBUG
					UnityEngine.Debug.LogError(ex.StackTrace);
#endif
				}
			}
		}

		private void OnRegisterMapObjects(Assembly assembly)
		{
			var types = assembly.GetTypes();
			foreach (var type in types.Where(t => t.GetCustomAttribute<MapObject>() != null))
			{
				try
				{
					var dataType = MapObjectUtils.GetMapObjectDataType(type);

					if (dataType == null)
					{
						throw new Exception($"Invalid map object: {type.Name} does not inherit from {typeof(IMapObject<>)}");
					}

					var mapObject = (IMapObject) AccessTools.CreateInstance(type);
					this.mapObjectManager.RegisterMapObject(dataType, mapObject);
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

			var invalidatedLevels = LevelManager.levels.Keys.Where(m => m.StartsWith("MapsExtended:")).ToArray();
			LevelManager.RemoveLevels(invalidatedLevels);
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
			GameObjectUtils.DestroyChildrenImmediateSafe(container);

			int toLoad = mapData.mapObjects.Count;

			foreach (var mapObject in mapData.mapObjects)
			{
				mapObjectManager.Instantiate(mapObject, container.transform, _ => toLoad--);
			}

			while (toLoad > 0)
			{
				yield return null;
			}

			onLoad?.Invoke();
		}
	}

	[HarmonyPatch(typeof(MapManager), "RPCA_LoadLevel")]
	static class MapManagerPatch_LoadLevel
	{
		private static void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
		{
			if (MapManager.instance.currentMap != null)
			{
				MapManager.instance.currentMap.Map.wasSpawned = false;
			}

			SceneManager.sceneLoaded -= MapManagerPatch_LoadLevel.OnLevelFinishedLoading;
			Map map = scene.GetRootGameObjects().Select(obj => obj.GetComponent<Map>()).FirstOrDefault(m => m != null);
			MapsExtended.LoadMap(map.gameObject, MapsExtended.instance.loadedMap, MapsExtended.instance.mapObjectManager);
		}

		public static void Prefix(ref string sceneName)
		{
			if (sceneName?.StartsWith("MapsExtended:") == true)
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
	static class MapManagerPatch_GetIDFromScene
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
	static class MapManagerDebugPatch
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
	static class MapManagerPatch_GetSpawnPoints
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
	static class MapTransitionPatch_Toggle
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
	static class RopePatch_AddJoint
	{
		public static void Postfix(MapObjet_Rope __instance, AnchoredJoint2D ___joint)
		{
			__instance.JointAdded(___joint);
		}
	}

	// Needed to fix collision with animated saws
	[HarmonyPatch(typeof(NetworkPhysicsObject), "BulletPush")]
	static class NetworkPhysicsObject_BulletPush
	{
		public static bool Prefix(NetworkPhysicsObject __instance)
		{
			return __instance.gameObject.GetComponent<MapObjectAnimation>() == null;
		}
	}

	// Needed to fix collision with animated saws
	[HarmonyPatch(typeof(NetworkPhysicsObject), "Push")]
	static class NetworkPhysicsObject_Push
	{
		public static bool Prefix(NetworkPhysicsObject __instance)
		{
			return __instance.gameObject.GetComponent<MapObjectAnimation>() == null;
		}
	}

	// Fixes saw collision with destructible boxes
	[HarmonyPatch(typeof(DamageBox), "Collide")]
	static class DamageBox_Collide
	{
		public static bool Prefix(Collision2D collision)
		{
			var dmgInstance = collision.transform.GetComponent<DamageableMapObjectInstance>();
			return dmgInstance?.damageableByEnvironment != false;
		}

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

	[HarmonyPatch(typeof(PhotonMapObject), "Start")]
	static class PhotonMapObjectPatch_Start
	{
		public static bool Prefix(PhotonMapObject __instance)
		{
			var view = __instance.GetComponent<PhotonView>();
			if (view?.InstantiationData?.Length >= 1 && (view.InstantiationData[0] as string) == "lateInstantiated")
			{
				__instance.SetFieldValue("photonSpawned", true);
				__instance.SetFieldValue("map", __instance.GetComponentInParent<Map>());
				return false;
			}

			return true;
		}
	}

	[HarmonyPatch(typeof(PhotonMapObject), "Update")]
	class PhotonMapObjectPatch_Update
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
					newInstructions.Add(CodeInstruction.Call(typeof(PhotonMapObjectPatch_Update), "OnPhotonInstantiate"));
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

	[HarmonyPatch(typeof(Sonigon.Internal.Voice), "SetVolumeRatioUpdate")]
	static class SonigonDebugPatch
	{
		public static bool Prefix(Sonigon.SoundContainer ___soundContainer)
		{
			return ___soundContainer != null;
		}
	}

	[HarmonyPatch(typeof(MapTransition), "Toggle")]
	static class MapTransitionTogglePatch
	{
		public static void Prefix(GameObject obj, bool enabled)
		{
			var anim = obj.GetComponent<MapObjectAnimation>();
			if (anim)
			{
				anim.enabled = enabled;
			}
		}
	}

	[HarmonyPatch(typeof(PlayerManager), "MovePlayers")]
	static class PlayerManagerPatch_MovePlayers
	{
		public static void Prefix(PlayerManager __instance)
		{
			__instance.GetExtraData().movingPlayer = new bool[__instance.players.Count];

			for (int i = 0; i < __instance.players.Count; i++)
			{
				__instance.GetExtraData().movingPlayer[i] = true;
			}
		}
	}

	[HarmonyPatch(typeof(PlayerManager), "Move")]
	static class GM_ArmsRace_Patch_PointTransition
	{
		public static IEnumerator Postfix(IEnumerator e, PlayerManager __instance, PlayerVelocity player)
		{
			while (e.MoveNext())
			{
				yield return e.Current;
			}

			int index = __instance.players.FindIndex(p => p.data.playerVel == player);
			__instance.GetExtraData().movingPlayer[index] = false;
		}
	}
}
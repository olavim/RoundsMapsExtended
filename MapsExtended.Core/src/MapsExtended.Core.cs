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
using UnityEngine;
using UnboundLib;
using Photon.Pun;
using System.Collections;
using MapsExt.MapObjects;
using MapsExt.Properties;
using UnboundLib.Utils;
using MapsExt.Compatibility;
using UnboundLib.Utils.UI;

namespace MapsExt
{
	[BepInDependency("com.willis.rounds.unbound", "3.2.8")]
	[BepInPlugin(ModId, ModName, ModVersion)]
	public sealed partial class MapsExtended : BaseUnityPlugin
	{
		public const string ModId = "io.olavim.rounds.mapsextended";
		public const string ModName = "MapsExtended";
		public const string ModVersion = ThisAssembly.Project.Version;

		[Obsolete("Deprecated")]
		public static MapsExtended instance;

		[Obsolete("Map objects are registered automatically")]
		public Action<Assembly> RegisterMapObjectsAction;

		private static MapsExtended s_instance;

		public static NetworkedMapObjectManager MapObjectManager => s_instance._mapObjectManager;
		public static PropertyManager PropertyManager => s_instance._propertyManager;
		public static IEnumerable<CustomMap> LoadedMaps => s_instance._maps;

		private readonly Dictionary<PhotonMapObject, Action<GameObject>> _photonInstantiationListeners = new();
		private readonly PropertyManager _propertyManager = new();
		private NetworkedMapObjectManager _mapObjectManager;
		private List<CustomMap> _maps;

		private void Awake()
		{
#pragma warning disable CS0618
			MapsExtended.instance = this;
#pragma warning restore CS0618

			s_instance = this;

			new Harmony(MapsExtended.ModId).PatchAll();

			AssetUtils.LoadAssetBundleFromResources("mapbase", typeof(MapsExtended).Assembly);

			var mapObjectManagerGo = new GameObject("Root Map Object Manager");
			DontDestroyOnLoad(mapObjectManagerGo);
			this._mapObjectManager = mapObjectManagerGo.AddComponent<NetworkedMapObjectManager>();
			this._mapObjectManager.SetNetworkID($"{ModId}/RootMapObjectManager");

			SceneManager.sceneLoaded += (_, mode) =>
			{
				if (mode == LoadSceneMode.Single)
				{
					this.OnInit();
				}
			};
		}

		private void Start()
		{
			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				this.RegisterMapObjectProperties(asm);
				this.RegisterMapObjects(asm);
			}

			this.OnInit();
		}

		private void OnInit()
		{
			MapsExt.PropertyManager.Current = s_instance._propertyManager;
			MapsExt.MapObjectManager.Current = s_instance._mapObjectManager;
			this.UpdateMapFiles();
		}

#if DEBUG
		private void Update()
		{
			UnityEngine.Debug.developerConsoleVisible = false;
		}
#endif

		private void RegisterMapObjectProperties(Assembly assembly)
		{
			var types = assembly.GetTypes();
			foreach (var propertySerializerType in types.Where(t => t.GetCustomAttribute<PropertySerializerAttribute>() != null))
			{
				try
				{
					var attr = propertySerializerType.GetCustomAttribute<PropertySerializerAttribute>();
					var propertyType = attr.PropertyType;
					var instance = Activator.CreateInstance(propertySerializerType);
					var serializer = new LazyPropertySerializer(instance, propertyType);
					this._propertyManager.RegisterProperty(propertyType, serializer);
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError($"Could not register map object serializer {propertySerializerType.Name}: {ex.Message}");

#if DEBUG
					UnityEngine.Debug.LogError(ex.StackTrace);
#endif
				}
			}
		}

		private void RegisterMapObjects(Assembly assembly)
		{
			var serializer = new PropertyCompositeSerializer(this._propertyManager);
			var types = assembly.GetTypes();

			foreach (var mapObjectType in types.Where(t => t.GetCustomAttribute<MapObjectAttribute>() != null))
			{
				try
				{
					var attr = mapObjectType.GetCustomAttribute<MapObjectAttribute>();
					var dataType = attr.DataType;

					if (!typeof(MapObjectData).IsAssignableFrom(dataType))
					{
						throw new Exception($"Data type {mapObjectType.Name} is not assignable to {typeof(MapObjectData)}");
					}

					if (!typeof(IMapObject).IsAssignableFrom(mapObjectType))
					{
						throw new Exception($"{mapObjectType.Name} is not assignable to {typeof(IMapObject)}");
					}

					if (mapObjectType.GetConstructor(Type.EmptyTypes) == null)
					{
						throw new Exception($"{mapObjectType.Name} does not have a default constructor");
					}

					var mapObject = (IMapObject) AccessTools.CreateInstance(mapObjectType);
					this._mapObjectManager.RegisterMapObject(dataType, mapObject, serializer);
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError($"Could not register map object {mapObjectType.Name}: {ex.Message}");

#if DEBUG
					UnityEngine.Debug.LogException(ex);
#endif
				}
			}

			this.RegisterV0MapObjects(assembly);
		}

		private void UpdateMapFiles()
		{
			var activeLevels = (IList<string>) AccessTools.Field(typeof(LevelManager), "activeLevels").GetValue(null);
			var inactiveLevels = (IList<string>) AccessTools.Field(typeof(LevelManager), "inactiveLevels").GetValue(null);
			var levelsToRedraw = (IList<string>) AccessTools.Field(typeof(ToggleLevelMenuHandler), "levelsThatNeedToRedrawn").GetValue(ToggleLevelMenuHandler.instance);
			var allLevels = LevelManager.levels;

			var invalidatedLevels = allLevels.Keys.Where(m => m.StartsWith("MapsExtended:")).ToArray();

			foreach (var level in invalidatedLevels)
			{
				activeLevels?.Remove(level);
				inactiveLevels?.Remove(level);
				levelsToRedraw?.Remove(level);
				allLevels.Remove(level);
			}

			var pluginMapPaths = Directory.GetFiles(BepInEx.Paths.PluginPath, "*.map", SearchOption.AllDirectories);

			var personalMapsFolder = Path.Combine(BepInEx.Paths.GameRootPath, "maps");
			Directory.CreateDirectory(personalMapsFolder);

			var personalMapPaths = Directory.GetFiles(personalMapsFolder, "*.map", SearchOption.AllDirectories);

			var personalMaps = personalMapPaths.Select(path => MapLoader.LoadPath(path));
			var pluginMaps = new Dictionary<string, List<CustomMap>>();

			foreach (var path in pluginMapPaths)
			{
				string packName = Path.GetDirectoryName(path).Replace("_", " ");

				if (packName.Contains("-"))
				{
					packName = packName.Substring(packName.IndexOf("-") + 1);
				}

				packName = packName.Trim();

				if (!pluginMaps.ContainsKey(packName))
				{
					pluginMaps[packName] = new List<CustomMap>();
				}

				pluginMaps[packName].Add(MapLoader.LoadPath(path));
			}

			this._maps = new List<CustomMap>();
			this._maps.AddRange(personalMaps);

			foreach (var m in pluginMaps.Values)
			{
				this._maps.AddRange(m);
			}

			Logger.LogMessage($"Loaded {this._maps.Count} custom maps");

			this.RegisterNamedMaps(personalMaps, "Personal");

			foreach (var mod in pluginMaps.Keys)
			{
				this.RegisterNamedMaps(pluginMaps[mod], mod);
			}
		}

		private void RegisterNamedMaps(IEnumerable<CustomMap> maps, string category)
		{
			var mapNames = new Dictionary<string, string>();
			foreach (var map in maps)
			{
				mapNames["MapsExtended:" + map.Id] = map.Name;
			}

			LevelManager.RegisterNamedMaps(maps.Select(m => "MapsExtended:" + m.Id), mapNames, category);
		}

		internal static void AddPhotonInstantiateListener(PhotonMapObject mapObject, Action<GameObject> callback)
		{
			s_instance._photonInstantiationListeners.Add(mapObject, callback);
		}

		internal static void OnPhotonInstantiate(GameObject instance, PhotonMapObject mapObject)
		{
			s_instance._photonInstantiationListeners.TryGetValue(mapObject, out Action<GameObject> listener);
			if (listener != null)
			{
				listener(instance);
				s_instance._photonInstantiationListeners.Remove(mapObject);
			}
		}

		public static void LoadMap(GameObject container, string mapFilePath, MapObjectManager mapObjectManager, Action onLoad = null)
		{
			var mapData = MapLoader.LoadPath(mapFilePath);
			MapsExtended.LoadMap(container, mapData, mapObjectManager, onLoad);
		}

		public static void LoadMap(GameObject container, CustomMap mapData, MapObjectManager mapObjectManager, Action onLoad = null)
		{
			s_instance.StartCoroutine(MapsExtended.LoadMapCoroutine(container, mapData, mapObjectManager, onLoad));
		}

		private static IEnumerator LoadMapCoroutine(GameObject container, CustomMap mapData, MapObjectManager mapObjectManager, Action onLoad = null)
		{
			GameObjectUtils.DestroyChildrenImmediateSafe(container);

			int toLoad = mapData.MapObjects.Length;

			foreach (var mapObject in mapData.MapObjects)
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

	[HarmonyPatch(typeof(MapManager))]
	static class MapManagerPatch
	{
		private static CustomMap s_loadedMap;
		private static string s_loadedMapSceneName;

		private static void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
		{
			if (MapManager.instance.currentMap != null)
			{
				MapManager.instance.currentMap.Map.wasSpawned = false;
			}

			SceneManager.sceneLoaded -= MapManagerPatch.OnLevelFinishedLoading;
			Map map = scene.GetRootGameObjects().Select(obj => obj.GetComponent<Map>()).FirstOrDefault(m => m != null);
			MapsExtended.LoadMap(map.gameObject, s_loadedMap, MapsExtended.MapObjectManager);
		}

		[HarmonyPrefix]
		[HarmonyPatch("RPCA_LoadLevel")]
		public static void Prefix_LoadLevel(ref string sceneName)
		{
			if (sceneName?.StartsWith("MapsExtended:") == true)
			{
				string id = sceneName.Split(':')[1];

				s_loadedMap = MapsExtended.LoadedMaps.First(m => m.Id == id);
				s_loadedMapSceneName = sceneName;

				sceneName = "NewMap";
				SceneManager.sceneLoaded += MapManagerPatch.OnLevelFinishedLoading;
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch("GetIDFromScene")]
		public static bool Prefix_GetIDFromScene(Scene scene, MapManager __instance, ref int __result)
		{
			if (scene.name == "NewMap")
			{
				__result = __instance.levels.ToList().IndexOf(s_loadedMapSceneName);
				return false;
			}

			return true;
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
				spawns.Add(new()
				{
					ID = 0,
					TEAMID = 0,
					// Choose center of map as default spawn location
					localStartPos = Vector2.zero
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

				spawns.Add(new()
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
			var m_getCharacterData = AccessTools.Method(typeof(Component), "GetComponent", null, new[] { typeof(CharacterData) });
			var m_opImplicit = typeof(UnityEngine.Object)
				.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.First(mi => mi.Name == "op_Implicit" && mi.ReturnType == typeof(bool));

			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsLdloc() && list[i + 1].Calls(m_getCharacterData))
				{
					// Call GetComponent<CharacterData>() on a non-null component (base game bug)
					newInstructions.Add(new(OpCodes.Ldloc_1));
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
					newInstructions.Add(new(OpCodes.Ldloc_S, 4));
					newInstructions.Add(new(OpCodes.Call, m_opImplicit));
					newInstructions.Add(new(OpCodes.Brfalse, list[i + 3].operand));
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
	static class PhotonMapObjectPatch_Update
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			/* The PhotonMapObject instantiates a networked copy of itself in the Update method. Here we basically change
			 * `PhotonNetwork.Instantiate(...)` to `OnPhotonInstantiate(PhotonNetwork.Instantiate(...), this)`.
			 */
			var list = instructions.ToList();
			var newInstructions = new List<CodeInstruction>();

			var m_instantiate = UnboundLib.ExtensionMethods.GetMethodInfo(typeof(PhotonNetwork), nameof(PhotonNetwork.Instantiate));

			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Calls(m_instantiate))
				{
					newInstructions.Add(list[i]);
					newInstructions.Add(new(OpCodes.Ldarg_0));
					newInstructions.Add(CodeInstruction.Call(typeof(MapsExtended), nameof(MapsExtended.OnPhotonInstantiate)));
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
			__instance.GetExtraData().PlayersBeingMoved = new bool[__instance.players.Count];

			for (int i = 0; i < __instance.players.Count; i++)
			{
				__instance.GetExtraData().PlayersBeingMoved[i] = true;
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
			__instance.GetExtraData().PlayersBeingMoved[index] = false;
		}
	}
}
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
using Sirenix.Utilities;
using MapsExt.Utils;
using System.Runtime.CompilerServices;

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

		internal static MapsExtended Instance { get; private set; }

		public static NetworkedMapObjectManager MapObjectManager => Instance._mapObjectManager;
		public static PropertyManager PropertyManager => Instance._propertyManager;

#pragma warning disable CS0618
		public static IEnumerable<CustomMap> LoadedMaps => Instance._maps.Concat(Instance.maps);
#pragma warning restore CS0618

#pragma warning disable IDE1006
		[Obsolete("Use LoadedMaps instead")]
		public List<CustomMap> maps = new();
#pragma warning restore IDE1006

		private readonly Dictionary<PhotonMapObject, Action<GameObject>> _photonInstantiationListeners = new();
		private readonly PropertyManager _propertyManager = new();
		private NetworkedMapObjectManager _mapObjectManager;
		private List<CustomMap> _maps;
		private Dictionary<Type, ICompatibilityPatch> _compatibilityPatches = new();

		private void Awake()
		{
#pragma warning disable CS0618
			instance = this;
#pragma warning restore CS0618

			Instance = this;

			new Harmony(ModId).PatchAll();

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

			On.MainMenuHandler.Awake += (orig, self) =>
			{
				orig(self);
				MainCam.instance.cam.GetComponentInParent<CameraZoomHandler>().gameObject.AddComponent<CameraHandler>();
			};
		}

		private void Start()
		{
			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				this.RegisterMapObjectProperties(asm);
				this.RegisterMapObjects(asm);
			}

			this.ApplyCompatibilityPatches();
			this.OnInit();
		}

		private void OnInit()
		{
			PropertyManager.Current = Instance._propertyManager;
			MapsExt.MapObjectManager.Current = Instance._mapObjectManager;
			this.UpdateMapFiles();
		}

#if DEBUG
		private void Update()
		{
			UnityEngine.Debug.developerConsoleVisible = false;
		}
#endif

		private void ApplyCompatibilityPatches()
		{
			var types = ReflectionUtils.GetAssemblyTypes(Assembly.GetExecutingAssembly());

			foreach (var patchType in types.Where(t => Attribute.IsDefined(t, typeof(CompatibilityPatchAttribute))))
			{
				if (!patchType.ImplementsOrInherits(typeof(ICompatibilityPatch)))
				{
					throw new Exception($"Compatibility patch {patchType} does not implement {typeof(ICompatibilityPatch).Name}");
				}

				var patch = (ICompatibilityPatch) Activator.CreateInstance(patchType);
				this._compatibilityPatches.Add(patchType, patch);
				this.ExecuteAfterFrames(1, () => patch.Apply());
			}
		}

		public static T GetCompatibilityPatch<T>() where T : ICompatibilityPatch
		{
			return (T) (Instance._compatibilityPatches as IReadOnlyDictionary<Type, ICompatibilityPatch>).GetValueOrDefault(typeof(T), null)
				?? throw new ArgumentException($"No compatibility patch of type {typeof(T)} loaded");
		}

		[Obsolete("Map objects are registered automatically")]
		public void RegisterMapObjects() { }

		private void RegisterMapObjectProperties(Assembly assembly)
		{
			var types = ReflectionUtils.GetAssemblyTypes(assembly);

			foreach (var propertySerializerType in types.Where(t => Attribute.IsDefined(t, typeof(PropertySerializerAttribute))))
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
			var types = ReflectionUtils.GetAssemblyTypes(assembly);

			foreach (var mapObjectType in types.Where(t => Attribute.IsDefined(t, typeof(MapObjectAttribute))))
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

			var pluginMapPaths = Directory.GetFiles(Paths.PluginPath, "*.map", SearchOption.AllDirectories);

			var personalMapsFolder = Path.Combine(Paths.GameRootPath, "maps");
			Directory.CreateDirectory(personalMapsFolder);

			var personalMapPaths = Directory.GetFiles(personalMapsFolder, "*.map", SearchOption.AllDirectories);

			var personalMaps = new List<CustomMap>();

			foreach (var path in personalMapPaths)
			{
				try
				{
					personalMaps.Add(MapLoader.LoadPath(path));
				}
				catch (Exception)
				{
					this.Logger.LogError($"Could not load personal map {path}");
				}
			}

			var pluginMaps = new Dictionary<string, List<CustomMap>>();

			foreach (var path in pluginMapPaths)
			{
				string packName = Path.GetDirectoryName(path).Replace("_", " ");

				if (packName.Contains("-"))
				{
					packName = packName.Substring(packName.LastIndexOf("-") + 1);
				}

				packName = packName.Trim();

				if (!pluginMaps.ContainsKey(packName))
				{
					pluginMaps[packName] = new List<CustomMap>();
				}

				try
				{
					pluginMaps[packName].Add(MapLoader.LoadPath(path));
				}
				catch (Exception)
				{
					this.Logger.LogError($"Could not load plugin map {path}");
				}
			}

			this._maps = new List<CustomMap>();
			this._maps.AddRange(personalMaps);

			foreach (var m in pluginMaps.Values)
			{
				this._maps.AddRange(m);
			}

			this.Logger.LogMessage($"Loaded {this._maps.Count} custom maps");

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
			Instance._photonInstantiationListeners.Add(mapObject, callback);
		}

		internal static void OnPhotonInstantiate(GameObject instance, PhotonMapObject mapObject)
		{
			Instance._photonInstantiationListeners.TryGetValue(mapObject, out Action<GameObject> listener);
			if (listener != null)
			{
				listener(instance);
				Instance._photonInstantiationListeners.Remove(mapObject);
			}
		}

		[Obsolete("Use MapLoader.LoadPath instead")]
		private static CustomMap LoadMapData(string path)
		{
			return MapLoader.LoadPath(path);
		}

		public static void LoadMap(GameObject container, string mapFilePath, MapObjectManager mapObjectManager, Action onLoad = null)
		{
			var mapData = MapLoader.LoadPath(mapFilePath);
			LoadMap(container, mapData, mapObjectManager, onLoad);
		}

		public static void LoadMap(GameObject container, CustomMap mapData, MapObjectManager mapObjectManager, Action onLoad = null)
		{
			Instance.StartCoroutine(LoadMapCoroutine(container, mapData, mapObjectManager, onLoad));
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

			SceneManager.sceneLoaded -= OnLevelFinishedLoading;
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

				MapManager.instance.SetCurrentCustomMap(s_loadedMap);

				sceneName = "NewMap";
				SceneManager.sceneLoaded += OnLevelFinishedLoading;
			}
			else
			{
				MapManager.instance.SetCurrentCustomMap(null);
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

	[HarmonyPatch(typeof(MapTransition))]
	static class MapTransitionPatch_EnterExit
	{
		private class MapData
		{
			public bool Entered { get; set; }
		}

		private static readonly ConditionalWeakTable<Map, MapData> s_mapData = new();

		[HarmonyPatch("Enter")]
		[HarmonyPrefix]
		public static void EnterPrefix(Map map)
		{
			s_mapData.GetOrCreateValue(map).Entered = true;
		}

		[HarmonyPatch("Exit")]
		[HarmonyPrefix]
		public static bool ExitPrefix(Map map)
		{
			return s_mapData.GetOrCreateValue(map).Entered;
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

			var m_instantiate = ExtensionMethods.GetMethodInfo(typeof(PhotonNetwork), nameof(PhotonNetwork.Instantiate));

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

	[HarmonyPatch(typeof(OutOfBoundsHandler))]
	static class OutOfBoundsHandler_Patch
	{
		private static Vector2 GetMapSize()
		{
			var customMap = MapManager.instance.GetCurrentCustomMap();
			if (customMap != null)
			{
				var cam = MainCam.instance.cam;
				return (cam.ScreenToWorldPoint(customMap.Settings.MapSize) - cam.ScreenToWorldPoint(Vector2.zero)) * (20f / cam.orthographicSize);
			}

			return new Vector2(71.12f, 40f);
		}

		private static float GetMinX()
		{
			return -GetMapSize().x * 0.5f;
		}

		private static float GetMaxX()
		{
			return GetMapSize().x * 0.5f;
		}

		private static float GetMinY()
		{
			return -GetMapSize().y * 0.5f;
		}

		private static float GetMaxY()
		{
			return GetMapSize().y * 0.5f;
		}

		private static readonly MethodInfo m_minX = AccessTools.Method(typeof(OutOfBoundsHandler_Patch), nameof(OutOfBoundsHandler_Patch.GetMinX));
		private static readonly MethodInfo m_maxX = AccessTools.Method(typeof(OutOfBoundsHandler_Patch), nameof(OutOfBoundsHandler_Patch.GetMaxX));
		private static readonly MethodInfo m_minY = AccessTools.Method(typeof(OutOfBoundsHandler_Patch), nameof(OutOfBoundsHandler_Patch.GetMinY));
		private static readonly MethodInfo m_maxY = AccessTools.Method(typeof(OutOfBoundsHandler_Patch), nameof(OutOfBoundsHandler_Patch.GetMaxY));

		private static IEnumerable<CodeInstruction> SwitchOutDefaultMapSizes(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var ins in instructions)
			{
				if (ins.LoadsConstant(-35.56f))
				{
					yield return new CodeInstruction(OpCodes.Call, m_minX).WithLabels(ins.labels);
				}
				else if (ins.LoadsConstant(35.56f))
				{
					yield return new CodeInstruction(OpCodes.Call, m_maxX).WithLabels(ins.labels);
				}
				else if (ins.LoadsConstant(-20f))
				{
					yield return new CodeInstruction(OpCodes.Call, m_minY).WithLabels(ins.labels);
				}
				else if (ins.LoadsConstant(20f))
				{
					yield return new CodeInstruction(OpCodes.Call, m_maxY).WithLabels(ins.labels);
				}
				else
				{
					yield return ins;
				}
			}
		}

		[HarmonyPatch("LateUpdate")]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> OutOfBoundsHandler_Transpiler1(IEnumerable<CodeInstruction> instructions)
		{
			return SwitchOutDefaultMapSizes(instructions);
		}

		[HarmonyPatch("GetPoint")]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> OutOfBoundsHandler_Transpiler2(IEnumerable<CodeInstruction> instructions)
		{
			return SwitchOutDefaultMapSizes(instructions);
		}
	}

	[HarmonyPatch(typeof(ScreenEdgeBounce), "Update")]
	static class ScreenEdgeBounce_Patch
	{
		private static Vector2 GetMapSize()
		{
			var customMap = MapManager.instance.GetCurrentCustomMap();
			return customMap == null ? new Vector2(71.12f, 40f) : ConversionUtils.ScreenToWorldUnits(customMap.Settings.MapSize);
		}

		private static Vector3 GetNormalizedMapPosition(Vector3 worldPosition)
		{
			var mapSize = GetMapSize();
			var clampedX = Mathf.Clamp(worldPosition.x + mapSize.x * 0.5f, 0f, mapSize.x);
			var clampedY = Mathf.Clamp(worldPosition.y + mapSize.y * 0.5f, 0f, mapSize.y);
			return new Vector3(clampedX / mapSize.x, clampedY / mapSize.y, 0f);
		}

		private static Vector3 NormalizedMapPositionToWorldPosition(Vector3 normalizedPosition)
		{
			var mapSize = GetMapSize();
			return new Vector3(mapSize.x * (normalizedPosition.x - 0.5f), mapSize.y * (normalizedPosition.y - 0.5f), normalizedPosition.z);
		}

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var m_normalizedMapPos = AccessTools.Method(typeof(ScreenEdgeBounce_Patch), nameof(ScreenEdgeBounce_Patch.GetNormalizedMapPosition));
			var m_normalizedMapPosToWorldPos = AccessTools.Method(typeof(ScreenEdgeBounce_Patch), nameof(ScreenEdgeBounce_Patch.NormalizedMapPositionToWorldPosition));
			var m_worldToScreenPoint = typeof(Camera).GetMethod("WorldToScreenPoint", new[] { typeof(Vector3) });
			var m_screenToWorldPoint = typeof(Camera).GetMethod("ScreenToWorldPoint", new[] { typeof(Vector3) });
			var m_screenWidth = typeof(Screen).GetProperty("width").GetGetMethod();
			var m_screenHeight = typeof(Screen).GetProperty("height").GetGetMethod();
			var list = instructions.ToList();

			for (int i = 0; i < list.Count; i++)
			{
				if (i < list.Count - 22 && list[i].IsLdarg(0) && list[i + 5].Calls(m_worldToScreenPoint))
				{
					yield return new CodeInstruction(OpCodes.Nop).WithLabels(list[i].labels);
					yield return new CodeInstruction(OpCodes.Nop).WithLabels(list[i + 1].labels);
					yield return list[i + 2];
					yield return list[i + 3];
					yield return list[i + 4];
					yield return new CodeInstruction(OpCodes.Call, m_normalizedMapPos).WithLabels(list[i + 5].labels);
					yield return list[i + 6];
					for (int k = i + 7; k <= i + 22; k++)
					{
						yield return new CodeInstruction(OpCodes.Nop).WithLabels(list[k].labels);
					}
					i += 22;
				}
				else if (list[i].Calls(m_screenWidth))
				{
					yield return new CodeInstruction(OpCodes.Ldc_I4_1);
				}
				else if (list[i].Calls(m_screenHeight))
				{
					yield return new CodeInstruction(OpCodes.Ldc_I4_1);
				}
				else if (i < list.Count - 4 && list[i].IsLdloc() && list[i + 4].Calls(m_screenToWorldPoint))
				{
					yield return list[i];
					yield return new CodeInstruction(OpCodes.Ldloc_0).WithLabels(list[i + 1].labels);
					yield return new CodeInstruction(OpCodes.Call, m_normalizedMapPosToWorldPos).WithLabels(list[i + 2].labels);
					yield return new CodeInstruction(OpCodes.Nop).WithLabels(list[i + 3].labels);
					yield return new CodeInstruction(OpCodes.Nop).WithLabels(list[i + 4].labels);
					yield return new CodeInstruction(OpCodes.Nop).WithLabels(list[i + 5].labels);
					yield return list[i + 6];
					i += 6;
				}
				else
				{
					yield return list[i];
				}
			}
		}
	}
}
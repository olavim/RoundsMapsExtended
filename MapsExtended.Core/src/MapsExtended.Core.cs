﻿using System;
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
using BepInEx.Logging;
using Sirenix.Serialization;
using System.Threading.Tasks;
using System.Collections.Concurrent;

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

		internal static ManualLogSource Log;

		[Obsolete("Map objects are registered automatically")]
		public Action<Assembly> RegisterMapObjectsAction;

		internal static MapsExtended Instance { get; private set; }

		public static NetworkedMapObjectManager MapObjectManager => Instance._mapObjectManager;
		public static PropertyManager PropertyManager => Instance._propertyManager;

		[Obsolete("Deprecated")]
		public static IEnumerable<CustomMap> LoadedMaps => Instance._loadedMaps.Values.Concat(Instance.maps);

		[Obsolete("Deprecated")]
		public List<CustomMap> maps = new();

		private readonly Dictionary<PhotonMapObject, Action<GameObject>> _photonInstantiationListeners = new();
		private readonly PropertyManager _propertyManager = new();
		private NetworkedMapObjectManager _mapObjectManager;
		private List<(CustomMapInfo mapInfo, string path)> _mapInfos;
		private Dictionary<string, CustomMap> _loadedMaps;
		private Dictionary<Type, ICompatibilityPatch> _compatibilityPatches = new();

		private void Awake()
		{
#pragma warning disable CS0618
			instance = this;
#pragma warning restore CS0618

			Instance = this;

			Log = this.Logger;

			AssetUtils.LoadAssetBundleFromResources("mapbase", typeof(MapsExtended).Assembly);

			var mapObjectManagerGo = new GameObject("Root Map Object Manager");
			DontDestroyOnLoad(mapObjectManagerGo);
			this._mapObjectManager = mapObjectManagerGo.AddComponent<NetworkedMapObjectManager>();
			this._mapObjectManager.SetNetworkID($"{ModId}/RootMapObjectManager");

			On.MainMenuHandler.Awake += (orig, self) =>
			{
				orig(self);

				MainCam.instance.cam.GetComponentInParent<CameraZoomHandler>().gameObject.AddComponent<CameraHandler>();
				GameObject.Find("/Game/Visual/Rendering /").AddComponent<LightHandler>();

				this.OnInit();
			};
		}

		private void Start()
		{
			new Harmony(ModId).PatchAll();

			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				this.RegisterMapObjectProperties(asm);
				this.RegisterMapObjects(asm);
			}

			this.ApplyCompatibilityPatches();
		}

		private static void RefreshLevelMenu()
		{
			if (ToggleLevelMenuHandler.instance != null)
			{
				AccessTools.Field(typeof(ToggleLevelMenuHandler), "ScrollViews").SetValue(null, new Dictionary<string, Transform>());
				GameObject.Destroy(ToggleLevelMenuHandler.instance.mapMenuCanvas);
				GameObject.Destroy(Unbound.Instance.gameObject.GetComponent<ToggleLevelMenuHandler>());
				Unbound.Instance.gameObject.AddComponent<ToggleLevelMenuHandler>();
			}
		}

		private void OnInit()
		{
			PropertyManager.Current = Instance._propertyManager;
			MapsExt.MapObjectManager.Current = Instance._mapObjectManager;
			RefreshLevelMenu();

			// Wait for LevelManager to initialize before updating maps
			this.ExecuteAfterFrames(1, () =>
			{
				this.UpdateMapFiles();
			});
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
					MapsExtended.Log.LogError($"Could not register map object serializer {propertySerializerType.Name}: {ex.Message}");

#if DEBUG
					MapsExtended.Log.LogError(ex.StackTrace);
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
					MapsExtended.Log.LogError($"Could not register map object {mapObjectType.Name}: {ex.Message}");

#if DEBUG
					MapsExtended.Log.LogError(ex.StackTrace);
#endif
				}
			}

			this.RegisterV0MapObjects(assembly);
		}

		private void UpdateMapFiles()
		{
			LevelManager.RemoveLevels(LevelManager.levels.Keys.Where(m => m.StartsWith("MapsExtended:")).ToArray());

			var pluginMapPaths = Directory.GetFiles(Paths.PluginPath, "*.map", SearchOption.AllDirectories);

			var personalMapsFolder = Path.Combine(Paths.GameRootPath, "maps");
			Directory.CreateDirectory(personalMapsFolder);

			var personalMapPaths = Directory.GetFiles(personalMapsFolder, "*.map", SearchOption.AllDirectories);
			var personalMaps = new List<(CustomMapInfo, string)>();

			var deserializationContext = new DeserializationContext()
			{
				Config = new SerializationConfig()
				{
					DebugContext = new DebugContext()
					{
						ErrorHandlingPolicy = ErrorHandlingPolicy.ThrowOnErrors
					}
				}
			};

			foreach (var path in personalMapPaths)
			{
				try
				{
					personalMaps.Add((MapLoader.LoadInfoFromPath(path, deserializationContext), path));
				}
				catch (Exception ex)
				{
					this.Logger.LogException(ex);
				}
			}

			var pluginMaps = new Dictionary<string, List<(CustomMapInfo, string)>>();

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
					pluginMaps[packName] = new List<(CustomMapInfo, string)>();
				}

				try
				{
					pluginMaps[packName].Add((MapLoader.LoadInfoFromPath(path, deserializationContext), path));
				}
				catch (Exception ex)
				{
					this.Logger.LogException(ex);
				}
			}

			this._loadedMaps = new Dictionary<string, CustomMap>();
			this._mapInfos = new();
			this._mapInfos.AddRange(personalMaps);

			foreach (var mapInfo in pluginMaps.Values)
			{
				this._mapInfos.AddRange(mapInfo);
			}

			this.Logger.LogMessage($"Loaded {this._mapInfos.Count} custom maps");

			this.RegisterNamedMaps(personalMaps, "Personal");

			foreach (var mod in pluginMaps.Keys)
			{
				this.RegisterNamedMaps(pluginMaps[mod], mod);
			}

			this.StartCoroutine(this.ValidateMaps());
		}

		private IEnumerator ValidateMaps()
		{
			// Parsing JSON is slow, so we split the workload among multiple threads
			const int mapsPerTask = 5;
			int taskCount = (int) Mathf.Ceil(this._mapInfos.Count / (float) mapsPerTask);
			var tasks = new List<Task>();

			var invalidMaps = new ConcurrentBag<string>();

			// Validate map dependencies in the background
			for (int i = 0; i < taskCount; i++)
			{
				// Split workload among threads
				var mapInfos = this._mapInfos.Skip(i * mapsPerTask).Take(mapsPerTask).ToList();

				tasks.Add(Task.Run(() =>
				{
					foreach (var (mapInfo, _) in mapInfos)
					{
						try
						{
							/* Ideally we would just use Sirenix's SerializationUtility to check if the map can be loaded, but
							 * we need access to Unity's APIs to load referenced types, which is not allowed in worker threads.
							 * Instead we load the map data into a bit more manual JsonTextReader and check if all referenced
							 * types can be loaded.
							 */
							using var stream = new MemoryStream(mapInfo.Data);
							var types = new List<string>();

							using var reader = new JsonTextReader(stream, new());
							EntryType entry = 0;

							while (entry != EntryType.EndOfStream)
							{
								reader.ReadToNextEntry(out string currentKey, out string value, out entry);

								if (currentKey == "$type" && value.StartsWith("\""))
								{
									types.Add(value.Trim('"').Split('|')[1]);
								}
							}

							// Try to load all referenced types in the map
							var loader = MapLoader.ForVersion(mapInfo.Version);
							foreach (var typeName in types)
							{
								var type = loader.Context.Binder.BindToType(typeName);
								if (type == null)
								{
									invalidMaps.Add(mapInfo.Id);
									throw new Exception($"Could not load map {mapInfo.Name}: Missing type \"{typeName}\"");
								}
							}
						}
						catch (Exception ex)
						{
							this.Logger.LogError(ex.Message);
						}
					}
				}));
			}

			while (tasks.Any(t => !t.IsCompleted))
			{
				yield return null;
			}

			if (invalidMaps.Count > 0)
			{
				this._mapInfos.RemoveAll(m => invalidMaps.Contains(m.mapInfo.Id));

				foreach (string id in invalidMaps)
				{
					this._loadedMaps.Remove(id);
				}

				LevelManager.RemoveLevels(invalidMaps.Select(id => $"MapsExtended:{id}").ToArray());
				RefreshLevelMenu();
			}
		}

		private void RegisterNamedMaps(IEnumerable<(CustomMapInfo, string)> maps, string category)
		{
			var mapNames = new Dictionary<string, string>();
			foreach (var (mapInfo, _) in maps)
			{
				mapNames["MapsExtended:" + mapInfo.Id] = mapInfo.Name;
			}

			LevelManager.RegisterNamedMaps(mapNames.Keys, mapNames, category);
		}

		internal CustomMap GetMapById(string id)
		{
			if (!this._loadedMaps.ContainsKey(id))
			{
				var result = this._mapInfos.FirstOrDefault(m => m.mapInfo.Id == id);
				if (result == default)
				{
					throw new ArgumentException($"No map with id {id} found");
				}

				this._loadedMaps[id] = result == default ? null : MapLoader.LoadPath(result.path);
			}

			return this._loadedMaps[id];
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
			Utils.GameObjectUtils.DestroyChildrenImmediateSafe(container);

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

				s_loadedMap = MapsExtended.Instance.GetMapById(id);
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
			return customMap == null ? new Vector2(71.12f, 40f) : ConversionUtils.ScreenToWorldUnits(customMap.Settings.MapSize);
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
				if (i < list.Count - 5 && list[i].IsLdarg(0) && list[i + 5].Calls(m_worldToScreenPoint))
				{
					yield return new CodeInstruction(OpCodes.Nop).WithLabels(list[i].labels);
					yield return new CodeInstruction(OpCodes.Nop).WithLabels(list[i + 1].labels);
					yield return list[i + 2];
					yield return list[i + 3];
					yield return list[i + 4];
					yield return new CodeInstruction(OpCodes.Call, m_normalizedMapPos).WithLabels(list[i + 5].labels);
					i += 5;
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
					yield return new CodeInstruction(OpCodes.Ldloc_0);
					yield return new CodeInstruction(OpCodes.Call, m_normalizedMapPosToWorldPos);
					yield return new CodeInstruction(OpCodes.Nop);
					yield return new CodeInstruction(OpCodes.Nop);
					yield return list[i + 5];
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

	[HarmonyPatch(typeof(CardBar), "OnHover")]
	static class CardBarPatch_OnHover_Patch
	{
		public static void Postfix(GameObject ___currentCard)
		{
			___currentCard.GetComponentInChildren<SetScaleToZero>().transform.localScale *= MainCam.instance.cam.orthographicSize / 20f;
		}
	}

	[HarmonyPatch(typeof(CardVisuals), "ChangeSelected")]
	static class CardVisuals_ChangeSelected_Patch
	{
		public static void Postfix(ScaleShake ___shake)
		{
			___shake.targetScale *= MainCam.instance.cam.orthographicSize / 20f;
		}
	}

	[HarmonyPatch(typeof(MapTransition), "MoveObject")]
	static class MapTransition_MoveObject_Patch
	{
		public static void Prefix(ref Vector3 targetPos)
		{
			if (MapManager.instance.GetCurrentCustomMap() != null && targetPos.x < 0)
			{
				var mapSizeWorld = ConversionUtils.ScreenToWorldUnits(MapManager.instance.GetCurrentCustomMap().Settings.MapSize);
				targetPos = new Vector3(-(mapSizeWorld.x + 20f), targetPos.y, targetPos.z);
			}

			if (targetPos.x < 0 && targetPos.x > -90f)
			{
				targetPos = new Vector3(-90f, targetPos.y, targetPos.z);
			}
		}
	}

	[HarmonyPatch(typeof(MapTransition), "SetStartPos")]
	static class MapTransition_SetStartPos_Patch
	{
		public static void Postfix(Map map)
		{
			var targetPos = map.transform.position;

			if (MapManager.instance.GetCurrentCustomMap() != null && map.transform.position.x > 0)
			{
				var mapSizeWorld = ConversionUtils.ScreenToWorldUnits(MapManager.instance.GetCurrentCustomMap().Settings.MapSize);
				targetPos = new Vector3(mapSizeWorld.x + 20f, map.transform.position.y, map.transform.position.z);
			}

			if (targetPos.x < 90f)
			{
				targetPos = new Vector3(90f, targetPos.y, targetPos.z);
			}

			map.transform.position = targetPos;
		}
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;
using UnboundLib;
using MapsExt.MapObjects;
using MapsExt.Properties;
using Jotunn.Utils;
using MapsExt.Editor.MapObjects;
using MapsExt.Editor.Properties;
using System.Collections;
using UnityEngine.EventSystems;
using MapsExt.Editor.UI;
using MapsExt.Editor.Events;
using MapsExt.Compatibility;
using Sirenix.Utilities;

namespace MapsExt.Editor
{
	[BepInDependency("com.willis.rounds.unbound", "3.2.8")]
	[BepInDependency(MapsExtended.ModId, MapsExtended.ModVersion)]
	[BepInPlugin(ModId, ModName, ModVersion)]
	public sealed partial class MapsExtendedEditor : BaseUnityPlugin
	{
		public const string ModId = "io.olavim.rounds.mapsextended.editor";
		public const string ModName = "MapsExtended.Editor";
		public const string ModVersion = MapsExtended.ModVersion;

		public const int MapObjectAnimationLayer = 30;
		public const int MapObjectUILayer = 31;

		public static PropertyManager PropertyManager => s_instance._propertyManager;
		public static EditorMapObjectManager MapObjectManager => s_instance._mapObjectManager;


		internal static Dictionary<Type, Type> PropertyInspectorElements => s_instance._propertyInspectorElements;
		internal static List<(Type, string, string)> MapObjectAttributes => s_instance._mapObjectAttributes;
		internal static Dictionary<Type, Type[]> GroupEditorEventHandlers => s_instance._groupEventHandlers;
		internal static bool IsEditorActive => s_instance._editorActive;

		private static MapsExtendedEditor s_instance;

		private readonly List<(Type, string, string)> _mapObjectAttributes = new();
		private readonly Dictionary<Type, Type> _propertyInspectorElements = new();
		private readonly Dictionary<Type, Type[]> _groupEventHandlers = new();
		private Dictionary<Type, ICompatibilityPatch> _compatibilityPatches = new();
		private readonly PropertyManager _propertyManager = new();
		private EditorMapObjectManager _mapObjectManager;
		private bool _editorActive;
		private bool _editorClosing;

		private void Awake()
		{
			s_instance = this;

			var harmony = new Harmony(ModId);
			harmony.PatchAll();

			AssetUtils.LoadAssetBundleFromResources("mapeditor", typeof(MapsExtendedEditor).Assembly);

			var mapObjectManagerGo = new GameObject("Editor Map Object Manager");
			DontDestroyOnLoad(mapObjectManagerGo);
			this._mapObjectManager = mapObjectManagerGo.AddComponent<EditorMapObjectManager>();

			SceneManager.sceneLoaded += (_, mode) =>
			{
				if (mode == LoadSceneMode.Single)
				{
					this._editorActive = false;
					this._editorClosing = false;
				}
			};

			Directory.CreateDirectory(Path.Combine(Paths.GameRootPath, "maps"));
		}

		private void Start()
		{
			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				this.RegisterMapObjectProperties(asm);
				this.RegisterMapObjects(asm);
				this.RegisterPropertyInspectors(asm);
				this.RegisterGroupEditorEventHandlers(asm);
			}

			this.ApplyCompatibilityPatches();
			Unbound.RegisterMenu("Map Editor", OpenEditor, (_) => { }, null, false);
		}

		public static void OpenEditor()
		{
			s_instance.StartCoroutine(OpenEditorCoroutine());
		}

		public static void CloseEditor()
		{
			s_instance.StartCoroutine(CloseEditorCoroutine());
		}

		public static IEnumerator OpenEditorCoroutine()
		{
			yield return CloseEditorCoroutine();

			AccessTools.Field(typeof(UnboundLib.Utils.UI.ModOptions), "showingModOptions").SetValue(null, false);
			GameManager.instance.isPlaying = true;

			MapManager.instance.RPCA_LoadLevel("MapEditor");
			SceneManager.sceneLoaded += OnEditorLevelLoad;

			while (!s_instance._editorActive)
			{
				yield return null;
			}

			PropertyManager.Current = s_instance._propertyManager;
			MapsExt.MapObjectManager.Current = s_instance._mapObjectManager;
		}

		public static IEnumerator CloseEditorCoroutine()
		{
			while (s_instance._editorClosing)
			{
				yield return null;
			}

			if (!s_instance._editorActive)
			{
				yield break;
			}

			s_instance._editorClosing = true;
			var op = SceneManager.UnloadSceneAsync("MapEditor");
			MapManager.instance.currentMap = null;

			while (!op.isDone)
			{
				yield return null;
			}

			s_instance._editorActive = false;
			s_instance._editorClosing = false;

			MapManager.instance.isTestingMap = false;
			GameObject.Find("Game/UI/UI_MainMenu").gameObject.SetActive(true);
			GameObject.Find("Game").GetComponent<SetOfflineMode>().SetOnline();
		}

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
			return (T) (s_instance._compatibilityPatches as IReadOnlyDictionary<Type, ICompatibilityPatch>).GetValueOrDefault(typeof(T), null)
				?? throw new ArgumentException($"No compatibility patch of type {typeof(T)} loaded");
		}

		private void RegisterMapObjectProperties(Assembly assembly)
		{
			var types = ReflectionUtils.GetAssemblyTypes(assembly);

			foreach (var propertySerializerType in types.Where(t => Attribute.IsDefined(t, typeof(EditorPropertySerializerAttribute))))
			{
				try
				{
					var attr = propertySerializerType.GetCustomAttribute<EditorPropertySerializerAttribute>();
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

			foreach (var mapObjectType in types.Where(t => Attribute.IsDefined(t, typeof(EditorMapObjectAttribute))))
			{
				try
				{
					var attr = mapObjectType.GetCustomAttribute<EditorMapObjectAttribute>();
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
					this._mapObjectAttributes.Add((dataType, attr.Label, attr.Category ?? ""));
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError($"Could not register editor map object {mapObjectType.Name}: {ex.Message}");

#if DEBUG
					UnityEngine.Debug.LogException(ex);
#endif
				}
			}

			this.RegisterV0MapObjects(assembly);
		}

		private void RegisterPropertyInspectors(Assembly assembly)
		{
			var types = ReflectionUtils.GetAssemblyTypes(assembly);

			foreach (var elementType in types.Where(t => Attribute.IsDefined(t, typeof(InspectorElementAttribute))))
			{
				try
				{
					var attr = elementType.GetCustomAttribute<InspectorElementAttribute>();
					var propertyType = attr.PropertyType;

					if (!typeof(IProperty).IsAssignableFrom(propertyType))
					{
						throw new Exception($"Property type {propertyType.Name} is not assignable to {typeof(IProperty)}");
					}

					if (!typeof(IInspectorElement).IsAssignableFrom(elementType))
					{
						throw new Exception($"{elementType.Name} is not assignable to {typeof(IInspectorElement)}");
					}

					if (this._propertyInspectorElements.ContainsKey(propertyType))
					{
						throw new Exception($"Inspector for {propertyType.Name} is already registered");
					}

					this._propertyInspectorElements[propertyType] = elementType;
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError($"Could not register PropertyInspector {elementType.Name}: {ex.Message}");

#if DEBUG
					UnityEngine.Debug.LogError(ex.StackTrace);
#endif
				}
			}
		}

		private void RegisterGroupEditorEventHandlers(Assembly assembly)
		{
			var types = ReflectionUtils.GetAssemblyTypes(assembly);

			foreach (var type in types.Where(t => Attribute.IsDefined(t, typeof(GroupEventHandlerAttribute))))
			{
				this._groupEventHandlers[type] = type.GetCustomAttribute<GroupEventHandlerAttribute>().RequiredHandlerTypes;
			}
		}

		private static void OnEditorLevelLoad(Scene scene, LoadSceneMode mode)
		{
			SceneManager.sceneLoaded -= OnEditorLevelLoad;

			s_instance._editorActive = true;
			var map = MapManager.instance.currentMap.Map;
			map.SetFieldValue("hasCalledReady", true);

			var go = map.gameObject;
			go.transform.position = Vector3.zero;
			EventSystem.current.currentInputModule.inputOverride = go.AddComponent<EditorBaseInput>();

			MapManager.instance.isTestingMap = true;
			GameObject.Find("Game/UI/UI_MainMenu").gameObject.SetActive(false);
			GameObject.Find("Game").GetComponent<SetOfflineMode>().SetOffline();
			map.hasEntered = true;

			ArtHandler.instance.NextArt();
		}
	}

	[HarmonyPatch(typeof(ArtHandler), "Update")]
	static class ArtHandlerPatch
	{
		public static bool Prefix()
		{
			return !MapsExtendedEditor.IsEditorActive;
		}
	}
}

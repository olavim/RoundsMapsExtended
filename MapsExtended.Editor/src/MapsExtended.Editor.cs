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

		internal const int LAYER_ANIMATION_MAPOBJECT = 30;
		internal const int LAYER_MAPOBJECT_UI = 31;

		public static MapsExtendedEditor instance;

		internal bool _editorActive;
		internal bool _editorClosing;
		internal List<(Type, string, string)> _mapObjectAttributes = new();
		internal MapObjectManager _mapObjectManager;
		internal PropertyManager _propertyManager = new();
		internal Dictionary<Type, Type> _propertyInspectorElements = new();

		private void Awake()
		{
			MapsExtendedEditor.instance = this;

			var harmony = new Harmony(MapsExtendedEditor.ModId);
			harmony.PatchAll();

			AssetUtils.LoadAssetBundleFromResources("mapeditor", typeof(MapsExtendedEditor).Assembly);

			var mapObjectManagerGo = new GameObject("Editor Map Object Manager");
			DontDestroyOnLoad(mapObjectManagerGo);
			this._mapObjectManager = mapObjectManagerGo.AddComponent<MapObjectManager>();
			this._mapObjectManager.SetNetworkID($"{ModId}/RootMapObjectManager");

			SceneManager.sceneLoaded += (_, mode) =>
			{
				if (mode == LoadSceneMode.Single)
				{
					this._editorActive = false;
					this._editorClosing = false;
				}
			};

			Directory.CreateDirectory(Path.Combine(BepInEx.Paths.GameRootPath, "maps"));

			MapsExtended.instance.RegisterMapObjectPropertiesAction += this.RegisterPropertySerializers;
			MapsExtended.instance.RegisterMapObjectsAction += this.RegisterMapObjects;
		}

		private void Start()
		{
			MapsExtended.instance.RegisterMapObjectProperties();
			MapsExtended.instance.RegisterMapObjects();

			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				this.RegisterPropertyInspectors(asm);
			}

			Unbound.RegisterMenu("Map Editor", this.OpenEditor, (_) => { }, null, false);
		}

		public void OpenEditor()
		{
			this.StartCoroutine(this.OpenEditorCoroutine());
		}

		public void CloseEditor()
		{
			this.StartCoroutine(this.CloseEditorCoroutine());
		}

		public IEnumerator OpenEditorCoroutine()
		{
			yield return this.CloseEditorCoroutine();

			AccessTools.Field(typeof(UnboundLib.Utils.UI.ModOptions), "showingModOptions").SetValue(null, false);
			GameManager.instance.isPlaying = true;

			MapManager.instance.RPCA_LoadLevel("MapEditor");
			SceneManager.sceneLoaded += this.OnEditorLevelLoad;

			while (!this._editorActive)
			{
				yield return null;
			}
		}

		public IEnumerator CloseEditorCoroutine()
		{
			while (this._editorClosing)
			{
				yield return null;
			}

			if (!this._editorActive)
			{
				yield break;
			}

			this._editorClosing = true;
			var op = SceneManager.UnloadSceneAsync("MapEditor");
			MapManager.instance.currentMap = null;

			while (!op.isDone)
			{
				yield return null;
			}

			this._editorActive = false;
			this._editorClosing = false;

			MapManager.instance.isTestingMap = false;
			GameObject.Find("Game/UI/UI_MainMenu").gameObject.SetActive(true);
			GameObject.Find("Game").GetComponent<SetOfflineMode>().SetOnline();
		}

		private void RegisterPropertySerializers(Assembly assembly)
		{
			var types = assembly.GetTypes();
			foreach (var propertySerializerType in types.Where(t => t.GetCustomAttribute<EditorPropertySerializerAttribute>() != null))
			{
				try
				{
					var attr = propertySerializerType.GetCustomAttribute<EditorPropertySerializerAttribute>();
					var propertyType = attr.PropertyType;

					if (!typeof(IPropertySerializer).IsAssignableFrom(propertySerializerType))
					{
						throw new Exception($"{propertyType.Name} is not assignable to {typeof(PropertySerializer<>)}");
					}

					this._propertyManager.RegisterProperty(propertyType, propertySerializerType);
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

			foreach (var mapObjectType in types.Where(t => t.GetCustomAttribute<EditorMapObjectAttribute>() != null))
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
					UnityEngine.Debug.LogError($"Could not register EditorMapObject {mapObjectType.Name}: {ex.Message}");

#if DEBUG
					UnityEngine.Debug.LogError(ex.StackTrace);
#endif
				}
			}

			this.RegisterV0MapObjects(assembly);
		}

		private void RegisterPropertyInspectors(Assembly assembly)
		{
			var types = assembly.GetTypes();

			foreach (var elementType in types.Where(t => t.GetCustomAttribute<PropertyInspectorAttribute>() != null))
			{
				try
				{
					var attr = elementType.GetCustomAttribute<PropertyInspectorAttribute>();
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

			this.RegisterV0MapObjects(assembly);
		}

		private void OnEditorLevelLoad(Scene scene, LoadSceneMode mode)
		{
			SceneManager.sceneLoaded -= this.OnEditorLevelLoad;

			this._editorActive = true;
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
			return !MapsExtendedEditor.instance._editorActive;
		}
	}
}

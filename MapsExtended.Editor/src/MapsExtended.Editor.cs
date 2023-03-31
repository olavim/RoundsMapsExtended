using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using HarmonyLib;
using UnboundLib;
using MapsExt.MapObjects;
using MapsExt.MapObjects.Properties;
using Jotunn.Utils;
using MapsExt.Editor.MapObjects;
using MapsExt.Editor.MapObjects.Properties;
using System.Collections;
using UnityEngine.EventSystems;
using MapsExt.Editor.UI;
using UnityEngine.UI;

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
		internal GameObject _frontParticles;
		internal GameObject _mainPostProcessing;

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
					this._frontParticles = GameObject.Find("/Game/Visual/Rendering /FrontParticles");
					this._mainPostProcessing = GameObject.Find("/Game/Visual/Post/Post_Main");

					MainCam.instance.gameObject.GetComponent<PostProcessLayer>().enabled = false;
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

			this._frontParticles = GameObject.Find("/Game/Visual/Rendering /FrontParticles");
			this._mainPostProcessing = GameObject.Find("/Game/Visual/Post/Post_Main");

			var cameraGo = new GameObject("PostProcessCamera");
			cameraGo.transform.SetParent(this.transform);

			var camera = cameraGo.AddComponent<Camera>();
			camera.CopyFrom(MainCam.instance.cam);
			camera.depth = 2;
			camera.cullingMask = 0; // Render nothing, only apply post-processing fx

			var layer = cameraGo.AddComponent<PostProcessLayer>();
			layer.Init((PostProcessResources) MainCam.instance.gameObject.GetComponent<PostProcessLayer>().GetFieldValue("m_Resources"));
			layer.volumeTrigger = cameraGo.transform;
			layer.volumeLayer = 1 << LayerMask.NameToLayer("Default Post");
			layer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;

			MainCam.instance.gameObject.GetComponent<PostProcessLayer>().enabled = false;
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

		public void LoadMap(GameObject container, string mapFilePath)
		{
			MapsExtended.LoadMap(container, mapFilePath, this._mapObjectManager, () =>
			{
				foreach (var mapObject in container.GetComponentsInChildren<MapObjectInstance>())
				{
					this.SetupMapObject(container, mapObject.gameObject);
				}

				this.SetMapPhysicsActive(container, false);
			});
		}

		public void SpawnObject(GameObject container, Type dataType, Action<GameObject> cb)
		{
			try
			{
				var mapObject = (MapObjectData) AccessTools.CreateInstance(dataType);
				this.SpawnObject(container, mapObject, cb);
			}
			catch (Exception ex)
			{
				throw new Exception($"Could not spawn map object {dataType.Name}", ex);
			}
		}

		public void SpawnObject(GameObject container, MapObjectData data, Action<GameObject> cb = null)
		{
			this._mapObjectManager.Instantiate(data, container.transform, instance =>
			{
				this.SetupMapObject(container, instance);

				var rig = instance.GetComponent<Rigidbody2D>();
				if (rig)
				{
					this.SetPhysicsActive(rig, false);
				}

				cb?.Invoke(instance);
			});
		}

		private void SetupMapObject(GameObject container, GameObject go)
		{
			if (go.GetComponent<CodeAnimation>())
			{
				var originalPosition = go.transform.position;
				var originalScale = go.transform.localScale;

				var wrapper = new GameObject(go.name + "Wrapper");
				wrapper.transform.SetParent(container.transform);
				go.transform.SetParent(wrapper.transform);
				go.transform.localPosition = Vector3.zero;
				go.transform.localScale = Vector3.one;

				wrapper.transform.position = originalPosition;
				wrapper.transform.localScale = originalScale;
			}

			// The Map component normally sets the renderers and masks, but only on load
			var renderer = go.GetComponent<SpriteRenderer>();
			if (renderer && renderer.color.a >= 0.5f)
			{
				renderer.transform.position = new Vector3(renderer.transform.position.x, renderer.transform.position.y, -3f);
				if (renderer.gameObject.tag != "NoMask")
				{
					renderer.color = new Color(0.21568628f, 0.21568628f, 0.21568628f);
					if (!renderer.GetComponent<SpriteMask>())
					{
						renderer.gameObject.AddComponent<SpriteMask>().sprite = renderer.sprite;
					}
				}
			}

			var damageable = instance.GetComponent<DamagableEvent>();
			if (damageable)
			{
				damageable.disabled = true;
			}

			this.ResetAnimations(container);
		}

		public void ResetAnimations(GameObject go)
		{
			foreach (var anim in go.GetComponentsInChildren<MapObjectAnimation>())
			{
				anim.PlayOnAwake = false;
				anim.Stop();
			}
		}

		public void SetMapPhysicsActive(GameObject container, bool active)
		{
			foreach (var rig in container.GetComponentsInChildren<Rigidbody2D>())
			{
				this.SetPhysicsActive(rig, active);
			}
		}

		private void SetPhysicsActive(Rigidbody2D rig, bool active)
		{
			rig.simulated = true;
			rig.velocity = Vector2.zero;
			rig.angularVelocity = 0;

			if (!rig.gameObject.GetComponent<MapObjectAnimation>())
			{
				rig.isKinematic = !active;
			}
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

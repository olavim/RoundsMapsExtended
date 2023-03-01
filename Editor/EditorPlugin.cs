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

namespace MapsExt.Editor
{
	[BepInDependency("com.willis.rounds.unbound", "2.7.3")]
	[BepInDependency(MapsExtended.ModId, MapsExtended.ModVersion)]
	[BepInPlugin(ModId, ModName, ModVersion)]
	public class MapsExtendedEditor : BaseUnityPlugin
	{
		public const string ModId = "io.olavim.rounds.mapsextended.editor";
		public const string ModName = "MapsExtended.Editor";
		public const string ModVersion = MapsExtended.ModVersion;

		public const int LAYER_ANIMATION_MAPOBJECT = 30;
		public const int LAYER_MAPOBJECT_UI = 31;

		public static MapsExtendedEditor instance;

		public bool editorActive = false;
		public bool editorClosing = false;
		public List<Tuple<Type, EditorMapObject>> mapObjectAttributes = new List<Tuple<Type, EditorMapObject>>();

		internal MapObjectManager mapObjectManager;
		internal GameObject frontParticles;
		internal GameObject mainPostProcessing;

		private void Awake()
		{
			MapsExtendedEditor.instance = this;

			var harmony = new Harmony(MapsExtendedEditor.ModId);
			harmony.PatchAll();

			AssetUtils.LoadAssetBundleFromResources("mapeditor", typeof(MapsExtendedEditor).Assembly);

			var mapObjectManagerGo = new GameObject("Editor Map Object Manager");
			DontDestroyOnLoad(mapObjectManagerGo);
			this.mapObjectManager = mapObjectManagerGo.AddComponent<MapObjectManager>();
			this.mapObjectManager.SetNetworkID($"{ModId}/RootMapObjectManager");

			SceneManager.sceneLoaded += (scene, mode) =>
			{
				if (mode == LoadSceneMode.Single)
				{
					this.editorActive = false;
					this.editorClosing = false;
					this.frontParticles = GameObject.Find("/Game/Visual/Rendering /FrontParticles");
					this.mainPostProcessing = GameObject.Find("/Game/Visual/Post/Post_Main");

					MainCam.instance.gameObject.GetComponent<PostProcessLayer>().enabled = false;
				}
			};

			Directory.CreateDirectory(Path.Combine(BepInEx.Paths.GameRootPath, "maps"));

			string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			Assembly.LoadFrom($"{assemblyDir}{Path.DirectorySeparatorChar}NetTopologySuite.dll");
			Assembly.LoadFrom($"{assemblyDir}{Path.DirectorySeparatorChar}System.Buffers.dll");

			MapsExtended.instance.RegisterMapObjectPropertiesAction += this.RegisterMapObjectSerializers;
			MapsExtended.instance.RegisterMapObjectsAction += this.RegisterMapObjects;
		}

		private void Start()
		{
			MapsExtended.instance.RegisterMapObjectProperties();
			MapsExtended.instance.RegisterMapObjects();

			Unbound.RegisterMenu("Map Editor", this.OpenEditor, (obj) => { }, null, false);

			this.frontParticles = GameObject.Find("/Game/Visual/Rendering /FrontParticles");
			this.mainPostProcessing = GameObject.Find("/Game/Visual/Post/Post_Main");

			var cameraGo = new GameObject("PostProcessCamera");
			cameraGo.transform.SetParent(this.transform);

			var camera = cameraGo.AddComponent<Camera>();
			camera.CopyFrom(MainCam.instance.cam);
			camera.depth = 2;
			camera.cullingMask = 0; // Render nothing, only apply post-processing fx

			var layer = cameraGo.AddComponent<PostProcessLayer>();
			layer.Init((PostProcessResources) MainCam.instance.gameObject.GetComponent<PostProcessLayer>().GetFieldValue("m_Resources"));
			layer.volumeTrigger = cameraGo.transform;
			layer.volumeLayer = (1 << LayerMask.NameToLayer("Default Post"));
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

			while (!this.editorActive)
			{
				yield return null;
			}
		}

		public IEnumerator CloseEditorCoroutine()
		{
			while (this.editorClosing)
			{
				yield return null;
			}

			if (!this.editorActive)
			{
				yield break;
			}

			this.editorClosing = true;
			var op = SceneManager.UnloadSceneAsync("MapEditor");
			MapManager.instance.currentMap = null;

			while (!op.isDone)
			{
				yield return null;
			}

			this.editorActive = false;
			this.editorClosing = false;

			MapManager.instance.isTestingMap = false;
			GameObject.Find("Game/UI/UI_MainMenu").gameObject.SetActive(true);
			GameObject.Find("Game").GetComponent<SetOfflineMode>().SetOnline();
		}

		private void RegisterMapObjectSerializers(Assembly assembly)
		{
			var types = assembly.GetTypes();
			var typesWithAttribute = types.Where(t => t.GetCustomAttribute<EditorMapObjectProperty>() != null);

			foreach (var propertyType in typesWithAttribute)
			{
				try
				{
					var propertyTargetType = MapObjectUtils.GetMapObjectPropertyTargetType(propertyType);

					if (propertyTargetType == null)
					{
						throw new Exception($"Invalid editor serializer: {propertyType.Name} does not inherit from {typeof(IMapObjectProperty<>)}");
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

		private void RegisterMapObjects(Assembly assembly)
		{
			var types = assembly.GetTypes();
			var typesWithAttribute = types.Where(t => t.GetCustomAttribute<EditorMapObject>() != null);

			foreach (var type in typesWithAttribute)
			{
				try
				{
					var attr = type.GetCustomAttribute<EditorMapObject>();
					var dataType = MapObjectUtils.GetMapObjectDataType(type);

					if (dataType == null)
					{
						throw new Exception($"Invalid editor blueprint: {type.Name} does not inherit from {typeof(IMapObject<>)}");
					}

					var mapObject = (IMapObject) AccessTools.CreateInstance(type);
					this.mapObjectManager.RegisterMapObject(dataType, mapObject);
					this.mapObjectAttributes.Add(new Tuple<Type, EditorMapObject>(dataType, attr));
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError($"Could not register editor map object {type.Name}: {ex.Message}");

#if DEBUG
					UnityEngine.Debug.LogError(ex.StackTrace);
#endif
				}
			}
		}

		private void OnEditorLevelLoad(Scene scene, LoadSceneMode mode)
		{
			SceneManager.sceneLoaded -= this.OnEditorLevelLoad;

			this.editorActive = true;
			var map = MapManager.instance.currentMap.Map;
			map.SetFieldValue("hasCalledReady", true);

			var go = map.gameObject;
			go.transform.position = Vector3.zero;
			var baseInput = go.AddComponent<EditorBaseInput>();
			EventSystem.current.currentInputModule.inputOverride = baseInput;

			MapManager.instance.isTestingMap = true;
			GameObject.Find("Game/UI/UI_MainMenu").gameObject.SetActive(false);
			GameObject.Find("Game").GetComponent<SetOfflineMode>().SetOffline();
			map.hasEntered = true;

			ArtHandler.instance.NextArt();
		}

		public void LoadMap(GameObject container, string mapFilePath)
		{
			MapsExtended.LoadMap(container, mapFilePath, this.mapObjectManager, () =>
			{
				var mapObjects = container.GetComponentsInChildren<MapObjectInstance>();

				foreach (var mapObject in mapObjects)
				{
					this.SetupMapObject(container, mapObject.gameObject);
				}

				this.SetMapPhysicsActive(container, false);
			});
		}

		public void SpawnObject(GameObject container, Type dataType, Action<GameObject> cb)
		{
			var mapObject = (MapObjectData) AccessTools.CreateInstance(dataType);
			this.SpawnObject(container, mapObject, cb);
		}

		public void SpawnObject(GameObject container, MapObjectData data, Action<GameObject> cb = null)
		{
			this.mapObjectManager.Instantiate(data, container.transform, instance =>
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
				anim.playOnAwake = false;
				anim.Stop();
			}
		}

		public void SetMapPhysicsActive(GameObject container, bool active)
		{
			var rigs = container.GetComponentsInChildren<Rigidbody2D>();
			foreach (var rig in rigs)
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
	class ArtHandlerPatch
	{
		public static bool Prefix()
		{
			return !MapsExtendedEditor.instance.editorActive;
		}
	}
}

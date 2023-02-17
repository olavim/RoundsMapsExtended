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

namespace MapsExt.Editor
{
	[BepInDependency("com.willis.rounds.unbound", "2.7.3")]
	[BepInDependency("io.olavim.rounds.mapsextended", "0.9.0")]
	[BepInPlugin(ModId, "MapsExtended.Editor", Version)]
	public class MapsExtendedEditor : BaseUnityPlugin
	{
		private const string ModId = "io.olavim.rounds.mapsextended.editor";
		public const string Version = MapsExtended.Version;

		public static MapsExtendedEditor instance;

		public bool editorActive = false;
		public List<Tuple<Type, EditorMapObject>> mapObjectAttributes = new List<Tuple<Type, EditorMapObject>>();

		internal MapObjectManager mapObjectManager;
		internal GameObject frontParticles;
		internal GameObject mainPostProcessing;

		public void Awake()
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

		public void Start()
		{
			MapsExtended.instance.RegisterMapObjectProperties();
			MapsExtended.instance.RegisterMapObjects();

			Unbound.RegisterMenu("Map Editor", () =>
			{
				AccessTools.Field(typeof(UnboundLib.Utils.UI.ModOptions), "showingModOptions").SetValue(null, false);
				GameManager.instance.isPlaying = true;

				this.editorActive = true;
				MapManager.instance.RPCA_LoadLevel("MapEditor");
				SceneManager.sceneLoaded += this.OnEditorLevelLoad;
			}, (obj) => { }, null, false);

			this.frontParticles = GameObject.Find("/Game/Visual/Rendering /FrontParticles");
			this.mainPostProcessing = GameObject.Find("/Game/Visual/Post/Post_Main");

			var cameraGo = new GameObject("PostProcessCamera");
			cameraGo.transform.SetParent(this.transform);

			var camera = cameraGo.AddComponent<Camera>();
			camera.CopyFrom(MainCam.instance.cam);
			camera.depth = 10;
			camera.cullingMask = 0; // Render nothing, only apply post-processing fx

			var layer = cameraGo.AddComponent<PostProcessLayer>();
			layer.Init((PostProcessResources) MainCam.instance.gameObject.GetComponent<PostProcessLayer>().GetFieldValue("m_Resources"));
			layer.volumeTrigger = cameraGo.transform;
			layer.volumeLayer = (1 << LayerMask.NameToLayer("Default Post"));
			layer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;

			MainCam.instance.gameObject.GetComponent<PostProcessLayer>().enabled = false;
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

			var map = MapManager.instance.currentMap.Map;
			map.SetFieldValue("hasCalledReady", true);

			var go = map.gameObject;
			go.transform.position = Vector3.zero;

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

		public void SpawnObject(GameObject container, Type type, Action<GameObject> cb)
		{
			var mapObject = (MapObjectData) AccessTools.CreateInstance(type);
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
					this.ExecuteAfterFrames(1, () => this.SetPhysicsActive(rig, false));
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

		private void FixRenderLayers(GameObject go)
		{
			foreach (var renderer in go.GetComponentsInChildren<Renderer>())
			{
				if (renderer is SpriteMask)
				{
					continue;
				}

				var spriteRenderer = renderer as SpriteRenderer;
				var particleRenderer = renderer as ParticleSystemRenderer;

				if (spriteRenderer)
				{
					spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
				}

				if (particleRenderer)
				{
					particleRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
				}

				if (renderer.sortingLayerName == "MapParticle")
				{
					renderer.sortingOrder = 0;
				}

				if (renderer.sortingLayerName == "MostFront")
				{
					renderer.sortingOrder = 1;
				}

				if (renderer.sortingLayerName == "Background")
				{
					renderer.sortingOrder = 2;
				}

				renderer.sortingLayerID = SortingLayer.NameToID("MapParticle");
			}

			foreach (var mask in go.GetComponentsInChildren<SpriteMask>())
			{
				int layerID = mask.frontSortingLayerID;
				mask.frontSortingLayerID = SortingLayer.NameToID("MapParticle");
				mask.backSortingLayerID = SortingLayer.NameToID("MapParticle");

				if (layerID == SortingLayer.NameToID("MapParticle") || layerID == SortingLayer.NameToID("Default"))
				{
					mask.frontSortingOrder = 1;
					mask.backSortingOrder = 0;
				}

				if (layerID == SortingLayer.NameToID("MostFront"))
				{
					mask.frontSortingOrder = 2;
					mask.backSortingOrder = 1;
				}

				if (layerID == SortingLayer.NameToID("Background"))
				{
					mask.frontSortingOrder = 3;
					mask.backSortingOrder = 2;
				}

				mask.isCustomRangeActive = true;
			}

			foreach (var parentMask in go.GetComponentsInChildren<SpriteMask>())
			{
				if (!parentMask.enabled)
				{
					continue;
				}

				int minOrder = 999;
				int maxOrder = 0;

				foreach (var childRenderer in parentMask.gameObject.GetComponentsInChildren<Renderer>())
				{
					if (!(childRenderer is SpriteMask) && childRenderer.enabled)
					{
						minOrder = Mathf.Min(minOrder, childRenderer.sortingOrder);
						maxOrder = Mathf.Max(maxOrder, childRenderer.sortingOrder);
					}
				}

				if (minOrder != 999)
				{
					parentMask.frontSortingOrder = maxOrder + 1;
					parentMask.backSortingOrder = minOrder;
				}
			}
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

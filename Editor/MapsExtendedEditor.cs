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
		public List<EditorMapObjectSpec> mapObjectAttributes = new List<EditorMapObjectSpec>();

		internal MapObjectManager mapObjectManager;

		public void Awake()
		{
			MapsExtendedEditor.instance = this;

			var harmony = new Harmony(MapsExtendedEditor.ModId);
			harmony.PatchAll();

			var mapObjectManagerGo = new GameObject("Editor Map Object Manager");
			DontDestroyOnLoad(mapObjectManagerGo);
			this.mapObjectManager = mapObjectManagerGo.AddComponent<MapObjectManager>();
			this.mapObjectManager.SetNetworkID($"{ModId}/RootMapObjectManager");

			SceneManager.sceneLoaded += (scene, mode) =>
			{
				if (mode == LoadSceneMode.Single)
				{
					this.editorActive = false;
				}
			};

			Directory.CreateDirectory(Path.Combine(BepInEx.Paths.GameRootPath, "maps"));

			string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			Assembly.LoadFrom($"{assemblyDir}{Path.DirectorySeparatorChar}MapsExtended.Editor.UI.dll");

			MapsExtended.instance.RegisterMapObjectsAction += this.RegisterMapObjects;
		}

		public void Start()
		{
			MapsExtended.instance.RegisterMapObjects();

			Unbound.RegisterMenu("Map Editor", () =>
			{
				AccessTools.Field(typeof(UnboundLib.Utils.UI.ModOptions), "showingModOptions").SetValue(null, false);
				GameManager.instance.isPlaying = true;

				this.editorActive = true;
				MapManager.instance.RPCA_LoadLevel("NewMap");
				SceneManager.sceneLoaded += this.AddEditorOnLevelLoad;
			}, (obj) => { }, null, false);
		}

		private void RegisterMapObjects(Assembly assembly)
		{
			var types = assembly.GetTypes();
			var typesWithAttribute = types.Where(t => t.GetCustomAttribute<EditorMapObjectSpec>() != null);

			foreach (var type in typesWithAttribute)
			{
				try
				{
					var attr = type.GetCustomAttribute<EditorMapObjectSpec>();
					var prefab =
						ReflectionUtils.GetAttributedProperty<GameObject>(type, typeof(EditorMapObjectPrefab)) ??
						ReflectionUtils.GetAttributedProperty<GameObject>(type, typeof(MapObjectPrefab));
					var serializer =
						ReflectionUtils.GetAttributedMethod<SerializerAction<MapObject>>(type, typeof(EditorMapObjectSerializer)) ??
						ReflectionUtils.GetAttributedMethod<SerializerAction<MapObject>>(type, typeof(MapObjectSerializer));
					var deserializer =
						ReflectionUtils.GetAttributedMethod<DeserializerAction<MapObject>>(type, typeof(EditorMapObjectDeserializer)) ??
						ReflectionUtils.GetAttributedMethod<DeserializerAction<MapObject>>(type, typeof(MapObjectDeserializer));

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
					this.mapObjectAttributes.Add(attr);
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

		private void AddEditorOnLevelLoad(Scene scene, LoadSceneMode mode)
		{
			SceneManager.sceneLoaded -= this.AddEditorOnLevelLoad;

			var map = MapManager.instance.currentMap.Map;
			map.SetFieldValue("hasCalledReady", true);

			var go = map.gameObject;
			go.transform.position = Vector3.zero;

			if (!go.GetComponent<MapEditor>())
			{
				go.AddComponent<MapEditor>();
			}

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
			var mapObject = (MapObject) AccessTools.CreateInstance(type);
			this.SpawnObject(container, mapObject, cb);
		}

		public void SpawnObject(GameObject container, MapObject data, Action<GameObject> cb = null)
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

			var mask = go.GetComponent<SpriteMask>();
			if (mask && mask.gameObject.tag != "NoMask")
			{
				mask.isCustomRangeActive = true;
				mask.frontSortingLayerID = SortingLayer.NameToID("MapParticle");
				mask.frontSortingOrder = 1;
				mask.backSortingLayerID = SortingLayer.NameToID("MapParticle");
				mask.backSortingOrder = 0;
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
			rig.isKinematic = rig.gameObject.GetComponent<MapObjectAnimation>() ? true : !active;
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

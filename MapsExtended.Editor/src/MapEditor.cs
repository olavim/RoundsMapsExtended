using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.Serialization;
using UnboundLib;
using UnboundLib.GameModes;
using MapsExt.MapObjects;
using MapsExt.Editor.Events;
using System;
using System.Collections;
using MapsExt.Editor.MapObjects;
using MapsExt.Utils;
using MapsExt.Editor.Utils;
using MapsExt.Compatibility;

namespace MapsExt.Editor
{
	public class MapEditor : MonoBehaviour
	{
		[SerializeField] private GameObject _content;
		[SerializeField] private GameObject _simulatedContent;
		[SerializeField] private MapEditorAnimationHandler _animationHandler;
		[SerializeField] private Grid _grid;
		private StateHistory<CustomMap> _stateHistory;
		private bool _isCreatingSelection;
		private Vector3 _selectionStartPosition;
		private Rect _selectionRect;
		private List<MapObjectData> _clipboardMapObjects;
		private GameObject _tempSpawn;
		private GameObject _dummyGroup;
		private GameObject _activeMapObjectPart;
		private GameObject _activeMapObjectOverride;
		private MapWrapper _mapWrapper;
		private CustomMapSettings _mapSettings = new();

		/// <summary>
		/// The currently active map object part. Can be the selected map object part or the container of multiple selected map object parts.
		/// </summary>
		public GameObject ActiveMapObjectPart => this._activeMapObjectPart;

		/// <summary>
		/// Returns <see cref="ActiveMapObjectPart"/> or the first of its parent game objects that has a <see cref="MapObjectInstance"/> component,
		/// unless overridden with <see cref="OverrideActiveMapObject"/>.
		/// </summary>
		public GameObject ActiveMapObject =>
			this._activeMapObjectOverride ??
			(this.SelectedMapObjects.Count == 1 ? this.SelectedMapObjects.First() : null);

		public GameObject Content { get => this._content; set => this._content = value; }
		public GameObject SimulatedContent { get => this._simulatedContent; set => this._simulatedContent = value; }
		public MapEditorAnimationHandler AnimationHandler { get => this._animationHandler; set => this._animationHandler = value; }
		public Grid Grid { get => this._grid; set => this._grid = value; }

		public HashSet<GameObject> SelectedMapObjectParts { get; } = new();
		public HashSet<GameObject> SelectedMapObjects { get; } = new();

		public event EventHandler<IEditorEvent> EditorEvent;

		/// <summary>
		/// Returns a copy of the current map settings.
		/// </summary>
		public CustomMapSettings MapSettings => new(this._mapSettings);

		public IEnumerable<GameObject> MapObjects => this.Content.GetComponentsInChildren<MapObjectInstance>(true).Select(x => x.gameObject);

		public bool SnapToGrid { get; set; } = true;
		public string CurrentMapName { get; private set; }
		public bool IsSimulating { get; private set; }

		public float GridSize
		{
			get => this.Grid.cellSize.x;
			set => this.Grid.cellSize = Vector2.one * value;
		}

		protected virtual void Awake()
		{
			this._stateHistory = new StateHistory<CustomMap>(this.GetMapData());
			this.gameObject.AddComponent<MapEditorInputHandler>();
		}

		protected virtual void Start()
		{
			MainCam.instance.cam.cullingMask &= ~(1 << MapsExtendedEditor.MapObjectAnimationLayer);
			MainCam.instance.cam.cullingMask &= ~(1 << MapsExtendedEditor.MapObjectUILayer);
			this._mapWrapper = MapManager.instance.currentMap;
			MapManager.instance.SetCurrentCustomMap(this.GetMapData());
		}

		protected virtual void Update()
		{
			if (this._isCreatingSelection)
			{
				this.UpdateSelection();
			}
		}

		public void ClearHistory()
		{
			this._stateHistory = new StateHistory<CustomMap>(this.GetMapData());
		}

		public void LoadMap(string mapFilePath)
		{
			var map = MapLoader.LoadPath(mapFilePath);
			this.LoadMap(map);

			string personalFolder = Path.Combine(BepInEx.Paths.GameRootPath, "maps" + Path.DirectorySeparatorChar);
			this.CurrentMapName = mapFilePath.StartsWith(personalFolder) ? map.Name : null;
		}

		public void LoadMap(CustomMap map)
		{
			MapsExtended.LoadMap(this.Content, map, MapsExtendedEditor.MapObjectManager);
			this.CurrentMapName = map.Name;
			this.SetMapSettings(new(map.Settings));

			this.ExecuteAfterFrames(1, () =>
			{
				this._stateHistory = new StateHistory<CustomMap>(this.GetMapData());
				this.ResetSpawnLabels();
				this.ClearSelected();
			});
		}

		public void SaveMap(string filename)
		{
			var bytes = SerializationUtility.SerializeValue(this.GetMapData(filename), DataFormat.JSON);

			string path = filename == null
				? Path.GetTempFileName()
				: Path.Combine(Path.Combine(BepInEx.Paths.GameRootPath, "maps"), filename + ".map");

			File.WriteAllBytes(path, bytes);

			this.CurrentMapName = filename;
		}

		private CustomMap GetMapData(string name = null)
		{
			var mapObjects = new List<MapObjectData>();

			foreach (var mapObject in this.Content.GetComponentsInChildren<MapObjectInstance>(true))
			{
				var data = mapObject.ReadMapObject();

				if (!data.Active && mapObject.gameObject == this.AnimationHandler.Animation?.gameObject)
				{
					data.Active = true;
				}

				mapObjects.Add(data);
			}

			return new CustomMap(Guid.NewGuid().ToString(), name, MapsExtended.ModVersion, this.MapSettings, mapObjects.ToArray());
		}

		public void CopySelected()
		{
			this._clipboardMapObjects = new List<MapObjectData>();
			var mapObjectInstances = this.SelectedMapObjectParts
				.Select(obj => obj.GetComponent<MapObjectInstance>() ?? obj.GetComponentInParent<MapObjectInstance>())
				.Distinct();

			foreach (var instance in mapObjectInstances)
			{
				var data = instance.ReadMapObject();
				data.MapObjectId = Guid.NewGuid().ToString();
				this._clipboardMapObjects.Add(data);
			}

			this.EditorEvent?.Invoke(this, new CopyEvent());
		}

		public IEnumerator Paste()
		{
			if (this._clipboardMapObjects == null || this._clipboardMapObjects.Count == 0)
			{
				yield break;
			}

			this.ClearSelected();

			var pastedMapObjects = new List<GameObject>();

			foreach (var mapObject in this._clipboardMapObjects)
			{
				MapsExtendedEditor.MapObjectManager.Instantiate(mapObject, this.Content.transform, obj => pastedMapObjects.Add(obj));
			}

			while (pastedMapObjects.Count < this._clipboardMapObjects.Count)
			{
				yield return null;
			}

			var pastedObjects = pastedMapObjects.SelectMany(obj => obj.GetComponentsInChildren<MapObjectPart>()).Select(obj => obj.gameObject);

			this.AddSelected(pastedObjects);
			this.ResetSpawnLabels();

			this.EditorEvent?.Invoke(this, new PasteEvent());
			yield return null;

			this.TakeSnaphot();
		}

		public void Undo()
		{
			this._stateHistory.Undo();
			this.LoadState(this._stateHistory.CurrentState);
		}

		public void Redo()
		{
			this._stateHistory.Redo();
			this.LoadState(this._stateHistory.CurrentState);
		}

		// A more graceful version of LoadMap which makes an effort to maintain selections and such
		private void LoadState(CustomMap state)
		{
			var dict = this.Content.GetComponentsInChildren<MapObjectInstance>(true).ToDictionary(item => item.MapObjectId, item => item.gameObject);

			foreach (var data in state.MapObjects)
			{
				if (dict.ContainsKey(data.MapObjectId))
				{
					// This map object already exists in the scene, so we just recover its state
					data.WriteMapObject(dict[data.MapObjectId]);

					// Mark a map object as "handled" by removing it from the dictionary
					dict.Remove(data.MapObjectId);
				}
				else
				{
					MapsExtendedEditor.MapObjectManager.Instantiate(data, this.Content.transform);
				}
			}

			var remainingSelected = this.SelectedMapObjectParts.Where(obj => !dict.ContainsKey(obj.GetComponentInParent<MapObjectInstance>().MapObjectId)).ToList();

			// Destroy map objects remaining in the dictionary since they don't exist in the new state
			foreach (var id in dict.Keys)
			{
				if (dict[id] == this.AnimationHandler.Animation?.gameObject)
				{
					this.AnimationHandler.SetAnimation(null);
				}

				MapsExt.Utils.GameObjectUtils.DestroyImmediateSafe(dict[id]);
			}

			this.ClearSelected();
			this.AddSelected(remainingSelected);
			this.AnimationHandler.Refresh();
			this.SetMapSettings(new(state.Settings));
		}

		public bool CanUndo()
		{
			return this._stateHistory.CanUndo();
		}

		public bool CanRedo()
		{
			return this._stateHistory.CanRedo();
		}

		public void CreateMapObject(Type mapObjectDataType)
		{
			this.ClearSelected();

			MapsExtendedEditor.MapObjectManager.Instantiate(mapObjectDataType, this.Content.transform, obj =>
			{
				var objectsWithHandlers = obj.GetComponentsInChildren<MapObjectPart>().Select(h => h.gameObject);

				this.AddSelected(objectsWithHandlers);
				this.ResetSpawnLabels();
				this.TakeSnaphot();
			});
		}

		public void ZoomIn()
		{
			CameraHandler.StaticZoom = Mathf.Max(2f, CameraHandler.StaticZoom - 2f);
		}

		public void ZoomOut()
		{
			CameraHandler.StaticZoom = Mathf.Min(500f, CameraHandler.StaticZoom + 2f);
		}

		public void ToggleSnapToGrid(bool enabled)
		{
			this.SnapToGrid = enabled;
		}

		public void DeleteSelectedMapObjects()
		{
			if (this.AnimationHandler.Animation != null)
			{
				throw new Exception("Cannot delete map objects while animating a map object.");
			}

			foreach (var instance in this.SelectedMapObjectParts.Select(obj => obj.GetComponentInParent<MapObjectInstance>().gameObject).Distinct().ToArray())
			{
				if (instance == this.AnimationHandler.Animation?.gameObject)
				{
					this.AnimationHandler.SetAnimation(null);
				}

				MapsExt.Utils.GameObjectUtils.DestroyImmediateSafe(instance);
			}

			this.ResetSpawnLabels();
			this.ClearSelected();
			this.TakeSnaphot();
		}

		public void StartSimulation()
		{
			this.ClearSelected();
			this.AnimationHandler.enabled = false;
			this.IsSimulating = true;
			this._isCreatingSelection = false;

			if (this.Content.GetComponentsInChildren<SpawnPoint>().Length == 0)
			{
				MapsExtendedEditor.MapObjectManager.Instantiate<SpawnData>(this.Content.transform, instance =>
				{
					Destroy(instance.GetComponent<Visualizers.SpawnVisualizer>());
					this._tempSpawn = instance;
					this.ExecuteAfterFrames(1, this.DoStartSimulation);
				});
			}
			else
			{
				this.ExecuteAfterFrames(1, this.DoStartSimulation);
			}
		}

		private void DoStartSimulation()
		{
			var simulatedMap = this.SimulatedContent.GetOrAddComponent<Map>();
			simulatedMap.SetFieldValue("spawnPoints", null);

			// var cam = MainCam.instance.cam;
			// cam.transform.position = new Vector3(0, 0, cam.transform.position.z);
			// var viewportDiff = this.MapSettings.ViewportSize - CustomMapSettings.DefaultViewportSize;
			// var wp = cam.ScreenToWorldPoint(this.MapSettings.ViewportSize - viewportDiff * 0.5f) * (20f / cam.orthographicSize);
			// simulatedMap.size = Mathf.Max(wp.y, wp.x / cam.aspect);
			simulatedMap.wasSpawned = true;
			simulatedMap.hasEntered = true;
			simulatedMap.SetFieldValue("missingObjects", 0);
			simulatedMap.SetFieldValue("hasCalledReady", true);
			simulatedMap.SetFieldValue("levelID", MapManager.instance.currentLevelID);
			simulatedMap.mapIsReadyEarlyAction = null;
			simulatedMap.mapIsReadyAction = null;

			var mapData = this.GetMapData();

			MapManager.instance.currentMap = new MapWrapper(simulatedMap, this._mapWrapper.Scene);
			MapManager.instance.SetCurrentCustomMap(mapData);
			CameraHandler.Mode = CameraHandler.CameraMode.FollowPlayer;

			MapsExtended.LoadMap(this.SimulatedContent, mapData, MapsExtended.MapObjectManager, () =>
			{
				this.Content.SetActive(false);
				this.SimulatedContent.SetActive(true);

				GameModeManager.SetGameMode("Sandbox");
				GameModeManager.CurrentHandler.StartGame();

				var gm = (GM_Test) GameModeManager.CurrentHandler.GameMode;
				gm.testMap = true;
				gm.gameObject.GetComponentInChildren<CurveAnimation>(true).enabled = false;

				this.ExecuteAfterFrames(1, () =>
				{
					simulatedMap.mapIsReadyEarlyAction?.Invoke();
					simulatedMap.allRigs = this.SimulatedContent.GetComponentsInChildren<Rigidbody2D>();

					this.ExecuteAfterFrames(1, () => simulatedMap.mapIsReadyAction?.Invoke());
				});
			});
		}

		public void StopSimulation()
		{
			var gm = (GM_Test) GameModeManager.CurrentHandler.GameMode;
			gm.gameObject.GetComponentInChildren<CurveAnimation>(true).enabled = true;

			this.IsSimulating = false;
			GameModeManager.SetGameMode(null);
			PlayerManager.instance.RemovePlayers();
			CardBarHandler.instance.ResetCardBards();
			CameraHandler.Mode = CameraHandler.CameraMode.Static;

			if (this._tempSpawn != null)
			{
				MapsExt.Utils.GameObjectUtils.DestroyImmediateSafe(this._tempSpawn);
				this._tempSpawn = null;
			}

			MapsExt.Utils.GameObjectUtils.DestroyChildrenImmediateSafe(this.SimulatedContent);

			this.Content.SetActive(true);
			this.SimulatedContent.SetActive(false);
			MapManager.instance.currentMap = this._mapWrapper;
			this.AnimationHandler.enabled = true;
		}

		public void StartSelection()
		{
			this._selectionStartPosition = EditorInput.MousePosition;
			this._isCreatingSelection = true;
		}

		public void EndSelection()
		{
			if (this._selectionRect.width > 2 && this._selectionRect.height > 2)
			{
				this.ClearSelected();
				var parts = EditorUtils.GetContainedMapObjectParts(UIUtils.GUIToWorldRect(this._selectionRect));

				// When editing animation, don't allow selecting other map objects
				if (this.AnimationHandler.Animation != null && parts.Any(p => p.gameObject == this.AnimationHandler.KeyframeMapObject))
				{
					this.AddSelected(this.AnimationHandler.KeyframeMapObject);
				}
				else if (this.AnimationHandler.Animation == null)
				{
					this.AddSelected(parts.Select(p => p.gameObject));
				}
			}

			this._isCreatingSelection = false;
			this._selectionRect = Rect.zero;
		}

		public Rect GetSelection()
		{
			return this._selectionRect;
		}

		internal void OnClickMapObjectParts(List<MapObjectPart> parts)
		{
			var gameObjects = parts.Where(p => p.GetComponentInParent<MapObjectInstance>() != null).Select(p => p.gameObject).ToList();
			gameObjects.Sort((a, b) => a.GetInstanceID() - b.GetInstanceID());
			GameObject selectedObject = null;

			// When editing animation, don't allow selecting other map objects
			if (this.AnimationHandler.Animation != null && gameObjects.Any(obj => obj == this.AnimationHandler.KeyframeMapObject))
			{
				selectedObject = this.AnimationHandler.KeyframeMapObject;
			}
			else if (this.AnimationHandler.Animation == null && gameObjects.Count > 0)
			{
				selectedObject = gameObjects[0];

				if (this.SelectedMapObjectParts.Count == 1)
				{
					int currentIndex = gameObjects.FindIndex(this.SelectedMapObjectParts.Contains);
					if (currentIndex != -1)
					{
						selectedObject = gameObjects[(currentIndex + 1) % gameObjects.Count];
					}
				}
			}

			int previouslySelectedCount = this.SelectedMapObjectParts.Count;
			bool clickedObjectIsSelected = this.SelectedMapObjectParts.Contains(selectedObject);
			this.ClearSelected();

			if (selectedObject == null)
			{
				return;
			}

			bool changeMultiSelectionToSingle = clickedObjectIsSelected && previouslySelectedCount > 1;
			bool selectUnselected = !clickedObjectIsSelected;

			if (changeMultiSelectionToSingle || selectUnselected)
			{
				this.AddSelected(selectedObject);
			}
		}

		internal void OnPointerDown()
		{
			this.EditorEvent?.Invoke(this, new PointerDownEvent());
		}

		internal void OnPointerUp()
		{
			this.EditorEvent?.Invoke(this, new PointerUpEvent());
		}

		internal void OnKeyDown(KeyCode key)
		{
			this.EditorEvent?.Invoke(this, new KeyDownEvent(key));
		}

		internal void OnKeyUp(KeyCode key)
		{
			this.EditorEvent?.Invoke(this, new KeyUpEvent(key));
		}

		private void UpdateSelection()
		{
			var mousePos = EditorInput.MousePosition;

			float width = Mathf.Abs(this._selectionStartPosition.x - mousePos.x);
			float height = Mathf.Abs(this._selectionStartPosition.y - mousePos.y);
			float x = Mathf.Min(this._selectionStartPosition.x, mousePos.x);
			float y = Screen.height - Mathf.Min(this._selectionStartPosition.y, mousePos.y) - height;

			this._selectionRect = new Rect(x, y, width, height);
		}

		public void ClearSelected()
		{
			this.EditorEvent?.Invoke(this, new DeselectEvent());

			if (this._dummyGroup != null)
			{
				MapsExt.Utils.GameObjectUtils.DestroyImmediateSafe(this._dummyGroup);
			}

			this.SelectedMapObjectParts.Clear();
			this.SelectedMapObjects.Clear();
			this._activeMapObjectPart = null;
		}

		public void AddSelected(GameObject obj)
		{
			this.AddSelected(new[] { obj });
		}

		public void AddSelected(IEnumerable<GameObject> list)
		{
			if (list.Any(obj => obj.GetComponentInParent<MapObjectInstance>() == null))
			{
				var offendingGameObject = list.First(obj => obj.GetComponentInParent<MapObjectInstance>() == null);
				throw new ArgumentException($"Cannot select {offendingGameObject.name}: One of the object's parents must contain a MapObjectInstance.");
			}

			if (list.Count() >= 2)
			{
				this._dummyGroup = new GameObject("Group");
				this._dummyGroup.transform.SetParent(this.Content.transform);
				this._dummyGroup.SetActive(false);
				this._dummyGroup.AddComponent<MapObjectPart>();

				var validGroupHandlerTypes = new List<Tuple<Type, Type>>();

				/* Find valid group event handlers.
				 * A group event handler is valid if all selected map objects have
				 * all event handlers that are required by the group event handler.
				 */
				foreach (var type in MapsExtendedEditor.GroupEditorEventHandlers.Keys)
				{
					var requiredTypes = MapsExtendedEditor.GroupEditorEventHandlers[type];
					if (list.All(obj => requiredTypes.All(t => obj.GetComponent(t) != null)))
					{
						this._dummyGroup.AddComponent(type);
					}
				}

				this._dummyGroup.SetActive(true);
				this._activeMapObjectPart = this._dummyGroup;
			}
			else
			{
				this._activeMapObjectPart = list.FirstOrDefault();
			}

			this.SelectedMapObjectParts.UnionWith(list);
			this.SelectedMapObjects.UnionWith(list.Select(x => x.GetComponentInParent<MapObjectInstance>()?.gameObject).Where(x => x != null));

			this.EditorEvent?.Invoke(this, new SelectEvent());
		}

		public void SelectAll()
		{
			this.ClearSelected();

			if (this.AnimationHandler.Animation != null)
			{
				this.AddSelected(this.AnimationHandler.KeyframeMapObject);
				return;
			}

			var list = new List<GameObject>();
			foreach (Transform child in this.Content.transform)
			{
				list.Add(child.gameObject);
			}

			this.AddSelected(list);
		}

		private void ResetSpawnLabels()
		{
			var spawns = this.Content.GetComponentsInChildren<SpawnPoint>().ToList();
			for (int i = 0; i < spawns.Count; i++)
			{
				spawns[i].ID = i;
				spawns[i].TEAMID = i;
				spawns[i].gameObject.name = $"SPAWN POINT {i}";
			}
		}

		public void TakeSnaphot()
		{
			this._stateHistory.AddState(this.GetMapData());
		}

		public void SetMapSize(Vector2 size)
		{
			this.SetMapSettings(new(this._mapSettings) { MapSize = size });
		}

		public void SetViewportHeight(int height)
		{
			this.SetMapSettings(new(this._mapSettings) { ViewportHeight = height });
		}

		private void SetMapSettings(CustomMapSettings settings)
		{
			this._mapSettings = settings;
			MapManager.instance.SetCurrentCustomMap(this.GetMapData());
		}

		/// <summary>
		/// Overrides <see cref="ActiveMapObject"/>. When overriden, a map object doesn't have to be selected to be active.
		/// Set to null to remove the override.
		/// </summary>
		/// <param name="obj">Object to override <see cref="ActiveMapObject"/> or null to remove the override</param>
		public void OverrideActiveMapObject(GameObject obj)
		{
			if (obj != null && obj.GetComponentInParent<MapObjectInstance>() == null)
			{
				throw new ArgumentException("Object must be null or have a MapObjectInstance component.");
			}

			this._activeMapObjectOverride = obj;
		}
	}
}

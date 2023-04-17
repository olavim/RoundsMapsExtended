using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.Serialization;
using UnboundLib;
using UnboundLib.GameModes;
using MapsExt.MapObjects;
using MapsExt.Editor.ActionHandlers;
using System;
using System.Collections;
using System.Reflection;

namespace MapsExt.Editor
{
	public class MapEditor : MonoBehaviour
	{
		[SerializeField] private GameObject _content;
		[SerializeField] private GameObject _simulatedContent;
		[SerializeField] private MapEditorAnimationHandler _animationHandler;
		[SerializeField] private Grid _grid;
		private readonly Dictionary<Type, Type[]> _groupActionHandlers = new();
		private StateHistory<CustomMap> _stateHistory;
		private bool _isCreatingSelection;
		private Vector3 _selectionStartPosition;
		private Rect _selectionRect;
		private List<MapObjectData> _clipboardMapObjects;
		private GameObject _tempSpawn;
		private GameObject _dummyGroup;

		public GameObject Content { get => this._content; set => this._content = value; }
		public GameObject SimulatedContent { get => this._simulatedContent; set => this._simulatedContent = value; }
		public MapEditorAnimationHandler AnimationHandler { get => this._animationHandler; set => this._animationHandler = value; }
		public Grid Grid { get => this._grid; set => this._grid = value; }

		public GameObject ActiveObject { get; set; }
		public RangeObservableCollection<GameObject> SelectedObjects { get; } = new RangeObservableCollection<GameObject>();

		public IEnumerable<GameObject> SelectedMapObjects => this.SelectedObjects
			.Where(x => x != null)
			.Select(x => x.GetComponentInParent<MapObjectInstance>()?.gameObject)
			.Where(x => x != null)
			.Distinct();

		public GameObject ActiveMapObject
		{
			get
			{
				if (this.ActiveObject?.GetComponentInParent<MapObjectInstance>() is MapObjectInstance instance)
				{
					return instance.gameObject;
				}

				var selected = this.SelectedMapObjects.ToList();
				return selected.Count == 1 ? selected[0] : null;
			}
		}

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

			foreach (var type in typeof(MapsExtendedEditor).Assembly.GetTypes().Where(t => t.GetCustomAttribute<GroupActionHandlerAttribute>() != null))
			{
				this._groupActionHandlers[type] = type.GetCustomAttribute<GroupActionHandlerAttribute>().requiredHandlerTypes;
			}
		}

		protected virtual void Start()
		{
			MainCam.instance.cam.cullingMask &= ~(1 << MapsExtendedEditor.LAYER_ANIMATION_MAPOBJECT);
			MainCam.instance.cam.cullingMask &= ~(1 << MapsExtendedEditor.LAYER_MAPOBJECT_UI);
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
				var data = MapsExtendedEditor.instance.MapObjectManager.Serialize(mapObject);

				if (!data.active && mapObject.gameObject == this.AnimationHandler.Animation?.gameObject)
				{
					data.active = true;
				}

				mapObjects.Add(data);
			}

			return new CustomMap(Guid.NewGuid().ToString(), name, MapsExtended.ModVersion, mapObjects.ToArray());
		}

		public void OnCopy()
		{
			this._clipboardMapObjects = new List<MapObjectData>();
			var mapObjectInstances = this.SelectedObjects
				.Select(obj => obj.GetComponent<MapObjectInstance>() ?? obj.GetComponentInParent<MapObjectInstance>())
				.Distinct();

			foreach (var instance in mapObjectInstances)
			{
				this._clipboardMapObjects.Add(MapsExtendedEditor.instance.MapObjectManager.Serialize(instance));
			}
		}

		public IEnumerator OnPaste()
		{
			if (this._clipboardMapObjects == null || this._clipboardMapObjects.Count == 0)
			{
				yield break;
			}

			int waiting = this._clipboardMapObjects.Count;
			this.ClearSelected();

			foreach (var mapObject in this._clipboardMapObjects)
			{
				MapObjectSpawner.SpawnObject(this.Content, mapObject, obj =>
				{
					foreach (var handler in obj.GetComponentsInChildren<PositionHandler>())
					{
						handler.Move(new Vector2(1, -1));
					}

					this.AddSelected(obj);
					waiting--;
				});
			}

			while (waiting > 0)
			{
				yield return null;
			}

			this.ResetSpawnLabels();
			this.RefreshHandlers();
			this.TakeSnaphot();
		}

		public void OnUndo()
		{
			this._stateHistory.Undo();
			this.LoadState(this._stateHistory.CurrentState);
		}

		public void OnRedo()
		{
			this._stateHistory.Redo();
			this.LoadState(this._stateHistory.CurrentState);
		}

		// A more graceful version of LoadMap which makes an effort to maintain selections and such
		private void LoadState(CustomMap state)
		{
			var dict = this.Content.GetComponentsInChildren<MapObjectInstance>(true).ToDictionary(item => item.MapObjectId, item => item.gameObject);

			foreach (var mapObject in state.MapObjects)
			{
				if (dict.ContainsKey(mapObject.mapObjectId))
				{
					// This map object already exists in the scene, so we just recover its state
					MapsExtendedEditor.instance.MapObjectManager.Deserialize(mapObject, dict[mapObject.mapObjectId]);

					// Mark a map object as "handled" by removing it from the dictionary
					dict.Remove(mapObject.mapObjectId);
				}
				else
				{
					MapObjectSpawner.SpawnObject(this.Content, mapObject);
				}
			}

			var remainingSelected = this.SelectedObjects.Where(obj => !dict.ContainsKey(obj.GetComponentInParent<MapObjectInstance>().MapObjectId)).ToList();

			// Destroy map objects remaining in the dictionary since they don't exist in the new state
			foreach (var id in dict.Keys)
			{
				if (dict[id] == this.AnimationHandler.Animation?.gameObject)
				{
					this.AnimationHandler.SetAnimation(null);
				}

				GameObjectUtils.DestroyImmediateSafe(dict[id]);
			}

			this.ClearSelected();
			this.AddSelected(remainingSelected);
			this.AnimationHandler.Refresh();
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

			MapObjectSpawner.SpawnObject(this.Content, mapObjectDataType, obj =>
			{
				var objectsWithHandlers = obj
					.GetComponentsInChildren<ActionHandler>()
					.Select(h => h.gameObject)
					.Distinct();

				this.AddSelected(objectsWithHandlers);
				this.ResetSpawnLabels();
				this.RefreshHandlers();
				this.TakeSnaphot();
			});
		}

		public void OnZoomIn()
		{
			var map = this.gameObject.GetComponent<Map>();
			float newSize = map.size - 2f;
			map.size = Mathf.Max(2f, newSize);
		}

		public void OnZoomOut()
		{
			var map = this.gameObject.GetComponent<Map>();
			float newSize = map.size + 2f;
			map.size = Mathf.Min(50f, newSize);
		}

		public void OnToggleSnapToGrid(bool enabled)
		{
			this.SnapToGrid = enabled;
		}

		public void OnDeleteSelectedMapObjects()
		{
			if (this.AnimationHandler.Animation != null)
			{
				throw new Exception("Cannot delete map objects while animating a map object.");
			}

			foreach (var instance in this.SelectedObjects.Select(obj => obj.GetComponentInParent<MapObjectInstance>().gameObject).Distinct().ToArray())
			{
				if (instance == this.AnimationHandler.Animation?.gameObject)
				{
					this.AnimationHandler.SetAnimation(null);
				}

				GameObjectUtils.DestroyImmediateSafe(instance);
			}

			this.ResetSpawnLabels();
			this.ClearSelected();
			this.RefreshHandlers();

			this.TakeSnaphot();
		}

		public void OnStartSimulation()
		{
			this.ClearSelected();
			this.gameObject.GetComponent<Map>().SetFieldValue("spawnPoints", null);
			this.AnimationHandler.enabled = false;
			this.IsSimulating = true;
			this._isCreatingSelection = false;

			if (this.Content.GetComponentsInChildren<SpawnPoint>().Length == 0)
			{
				MapObjectSpawner.SpawnObject<SpawnData>(this.Content, instance =>
				{
					GameObject.Destroy(instance.GetComponent<Visualizers.SpawnVisualizer>());
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
			MapsExtended.LoadMap(this.SimulatedContent, this.GetMapData(), () =>
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
					this.gameObject.GetComponent<Map>().allRigs = this.SimulatedContent.GetComponentsInChildren<Rigidbody2D>();

					foreach (var rope in this.SimulatedContent.GetComponentsInChildren<MapObjet_Rope>())
					{
						rope.Go();
					}
				});
			});
		}

		public void OnStopSimulation()
		{
			var gm = (GM_Test) GameModeManager.CurrentHandler.GameMode;
			gm.gameObject.GetComponentInChildren<CurveAnimation>(true).enabled = true;

			this.IsSimulating = false;
			GameModeManager.SetGameMode(null);
			PlayerManager.instance.RemovePlayers();
			CardBarHandler.instance.ResetCardBards();

			if (this._tempSpawn != null)
			{
				GameObjectUtils.DestroyImmediateSafe(this._tempSpawn);
				this._tempSpawn = null;
			}

			GameObjectUtils.DestroyChildrenImmediateSafe(this.SimulatedContent);

			this.Content.SetActive(true);
			this.SimulatedContent.SetActive(false);
			this.AnimationHandler.enabled = true;
		}

		public void LoadMap(string mapFilePath)
		{
			MapsExtended.LoadMap(this.Content, mapFilePath, MapsExtendedEditor.instance.MapObjectManager);

			string personalFolder = Path.Combine(BepInEx.Paths.GameRootPath, "maps" + Path.DirectorySeparatorChar);
			string mapName = mapFilePath.Substring(0, mapFilePath.Length - 4).Replace(personalFolder, "");

			this.CurrentMapName = mapFilePath.StartsWith(personalFolder) ? mapName : null;

			this.ExecuteAfterFrames(1, () =>
			{
				this._stateHistory = new StateHistory<CustomMap>(this.GetMapData());
				this.ResetSpawnLabels();
				this.ClearSelected();
				this.RefreshHandlers();
			});
		}

		public void OnSelectionStart()
		{
			this._selectionStartPosition = EditorInput.MousePosition;
			this._isCreatingSelection = true;
		}

		public void OnSelectionEnd()
		{
			if (this._selectionRect.width > 2 && this._selectionRect.height > 2)
			{
				var list = EditorUtils.GetContainedActionHandlers(UIUtils.GUIToWorldRect(this._selectionRect));
				this.ClearSelected();

				// When editing animation, don't allow selecting other map objects
				if (this.AnimationHandler.Animation != null && list.Any(obj => obj.gameObject == this.AnimationHandler.KeyframeMapObject))
				{
					this.AddSelected(this.AnimationHandler.KeyframeMapObject);
				}
				else if (this.AnimationHandler.Animation == null)
				{
					this.AddSelected(list.Select(h => h.gameObject).Distinct());
				}
			}

			this._isCreatingSelection = false;
			this._selectionRect = Rect.zero;
		}

		public void OnClickActionHandlers(List<ActionHandler> handlers)
		{
			var objects = handlers.Select(h => h.gameObject).Distinct().ToList();
			objects.Sort((a, b) => a.GetInstanceID() - b.GetInstanceID());
			GameObject selectedObject = null;

			// When editing animation, don't allow selecting other map objects
			if (this.AnimationHandler.Animation != null && objects.Any(obj => obj == this.AnimationHandler.KeyframeMapObject))
			{
				selectedObject = this.AnimationHandler.KeyframeMapObject;
			}
			else if (this.AnimationHandler.Animation == null && objects.Count > 0)
			{
				selectedObject = objects[0];

				if (this.SelectedObjects.Count == 1)
				{
					int currentIndex = objects.FindIndex(this.IsSelected);
					if (currentIndex != -1)
					{
						selectedObject = objects[(currentIndex + 1) % objects.Count];
					}
				}
			}

			int previouslySelectedCount = this.SelectedObjects.Count;
			bool clickedMapObjectIsSelected = this.IsSelected(selectedObject);
			this.ClearSelected();

			if (selectedObject == null)
			{
				return;
			}

			bool changeMultiSelectionToSingle = clickedMapObjectIsSelected && previouslySelectedCount > 1;
			bool selectUnselected = !clickedMapObjectIsSelected;

			if (changeMultiSelectionToSingle || selectUnselected)
			{
				this.AddSelected(selectedObject);
			}
		}

		public void OnPointerDown()
		{
			if (this.ActiveObject == null)
			{
				return;
			}

			foreach (var handler in this.ActiveObject.GetComponents<ActionHandler>())
			{
				handler.OnPointerDown();
			}
		}

		public void OnPointerUp()
		{
			if (this.ActiveObject == null)
			{
				return;
			}

			foreach (var handler in this.ActiveObject.GetComponents<ActionHandler>())
			{
				handler.OnPointerUp();
			}
		}

		public void OnKeyDown(KeyCode key)
		{
			if (this.ActiveObject == null)
			{
				return;
			}

			foreach (var handler in this.ActiveObject.GetComponents<ActionHandler>())
			{
				handler.OnKeyDown(key);
			}
		}

		public bool IsSelected(GameObject obj)
		{
			return this.SelectedObjects.Contains(obj);
		}

		public Rect GetSelection()
		{
			return this._selectionRect;
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
			if (this.ActiveObject != null)
			{
				foreach (var handler in this.ActiveObject.GetComponents<ActionHandler>())
				{
					handler.OnDeselect();
				}
			}

			if (this._dummyGroup != null)
			{
				GameObjectUtils.DestroyImmediateSafe(this._dummyGroup);
			}

			this.SelectedObjects.Clear();
			this.ActiveObject = null;
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

				var validGroupHandlerTypes = new List<Tuple<Type, Type>>();

				/* Find valid group action handlers.
				 * A group action handler is valid if all selected map objects have
				 * all action handlers that are required by the group action handler.
				 */
				foreach (var type in this._groupActionHandlers.Keys)
				{
					var requiredTypes = this._groupActionHandlers[type];
					if (list.All(obj => requiredTypes.All(t => obj.GetComponent(t) != null)))
					{
						var handler = (IGroupMapObjectActionHandler) this._dummyGroup.AddComponent(type);
						handler.Initialize(list);
					}
				}

				this._dummyGroup.SetActive(true);
				this.ActiveObject = this._dummyGroup;
			}
			else
			{
				this.ActiveObject = list.FirstOrDefault();
			}

			this.SelectedObjects.AddRange(list);

			if (this.ActiveObject != null)
			{
				foreach (var handler in this.ActiveObject.GetComponents<ActionHandler>())
				{
					handler.OnSelect();
				}
			}
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

		public void ResetSpawnLabels()
		{
			var spawns = this.Content.GetComponentsInChildren<SpawnPoint>().ToList();
			for (int i = 0; i < spawns.Count; i++)
			{
				spawns[i].ID = i;
				spawns[i].TEAMID = i;
				spawns[i].gameObject.name = $"SPAWN POINT {i}";
			}
		}

		public void RefreshHandlers()
		{
			foreach (var handler in this.Content.GetComponentsInChildren<ActionHandler>())
			{
				handler.OnRefresh();
			}
		}

		public void TakeSnaphot()
		{
			this._stateHistory.AddState(this.GetMapData());
		}
	}
}

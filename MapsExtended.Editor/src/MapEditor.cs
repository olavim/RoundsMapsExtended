using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.Serialization;
using UnboundLib;
using UnboundLib.GameModes;
using MapsExt.MapObjects;
using MapsExt.Editor.MapObjects;
using MapsExt.Editor.ActionHandlers;
using System;
using System.Collections;
using System.Reflection;

namespace MapsExt.Editor
{
	public class MapEditor : MonoBehaviour
	{
		public GameObject activeObject;
		public RangeObservableCollection<GameObject> selectedObjects;
		public string currentMapName;
		public bool isSimulating;
		public bool snapToGrid;
		public GameObject content;
		public GameObject simulatedContent;
		public MapEditorAnimationHandler animationHandler;
		public Grid grid;

		public float GridSize
		{
			get { return this.grid.cellSize.x; }
			set { this.grid.cellSize = Vector3.one * value; }
		}

		private StateHistory stateHistory;
		private bool isCreatingSelection;
		private Vector3 selectionStartPosition;
		private Rect selectionRect;
		private List<MapObjectData> clipboardMapObjects;
		private Dictionary<Type, Type[]> groupActionHandlers;

		private GameObject tempSpawn;
		private GameObject dummyGroup;

		protected virtual void Awake()
		{
			this.activeObject = null;
			this.selectedObjects = new RangeObservableCollection<GameObject>();
			this.groupActionHandlers = new Dictionary<Type, Type[]>();
			this.snapToGrid = true;
			this.isCreatingSelection = false;
			this.currentMapName = null;
			this.isSimulating = false;

			this.stateHistory = new StateHistory(this.GetMapData());

			this.gameObject.AddComponent<MapEditorInputHandler>();

			foreach (var type in typeof(MapsExtendedEditor).Assembly.GetTypes().Where(t => t.GetCustomAttribute<GroupMapObjectActionHandler>() != null))
			{
				this.groupActionHandlers[type] = type.GetCustomAttribute<GroupMapObjectActionHandler>().requiredHandlerTypes;
			}
		}

		protected virtual void Start()
		{
			MainCam.instance.cam.cullingMask &= ~(1 << MapsExtendedEditor.LAYER_ANIMATION_MAPOBJECT);
			MainCam.instance.cam.cullingMask &= ~(1 << MapsExtendedEditor.LAYER_MAPOBJECT_UI);
		}

		protected virtual void Update()
		{
			if (this.isCreatingSelection)
			{
				this.UpdateSelection();
			}
		}

		public void SaveMap(string filename)
		{
			var bytes = SerializationUtility.SerializeValue(this.GetMapData(filename), DataFormat.JSON);

			string path = filename == null
				? Path.GetTempFileName()
				: Path.Combine(Path.Combine(BepInEx.Paths.GameRootPath, "maps"), filename + ".map");

			File.WriteAllBytes(path, bytes);

			this.currentMapName = filename;
		}

		private CustomMap GetMapData(string name = null)
		{
			var mapData = new CustomMap
			{
				id = Guid.NewGuid().ToString(),
				name = name,
				mapObjects = new List<MapObjectData>()
			};

			foreach (var mapObject in this.content.GetComponentsInChildren<MapObjectInstance>(true))
			{
				var data = MapsExtendedEditor.instance.mapObjectManager.Serialize(mapObject);

				if (!data.active && mapObject.gameObject == this.animationHandler.animation?.gameObject)
				{
					data.active = true;
				}

				mapData.mapObjects.Add(data);
			}

			return mapData;
		}

		public void OnCopy()
		{
			this.clipboardMapObjects = new List<MapObjectData>();
			var mapObjectInstances = this.selectedObjects
				.Select(obj => obj.GetComponent<MapObjectInstance>() ?? obj.GetComponentInParent<MapObjectInstance>())
				.Distinct();

			foreach (var instance in mapObjectInstances)
			{
				this.clipboardMapObjects.Add(MapsExtendedEditor.instance.mapObjectManager.Serialize(instance));
			}
		}

		public void OnPaste()
		{
			this.StartCoroutine(this.OnPasteCoroutine());
		}

		public void OnUndo()
		{
			this.stateHistory.Undo();
			this.LoadState(this.stateHistory.CurrentState);
		}

		public void OnRedo()
		{
			this.stateHistory.Redo();
			this.LoadState(this.stateHistory.CurrentState);
		}

		// A more graceful version of LoadMap which makes an effort to maintain selections and such
		private void LoadState(CustomMap state)
		{
			var dict = this.content.GetComponentsInChildren<MapObjectInstance>(true).ToDictionary(item => item.mapObjectId, item => item.gameObject);

			foreach (var mapObject in state.mapObjects)
			{
				if (dict.ContainsKey(mapObject.mapObjectId))
				{
					// This map object already exists in the scene, so we just recover its state
					MapsExtendedEditor.instance.mapObjectManager.Deserialize(mapObject, dict[mapObject.mapObjectId]);

					// Mark a map object as "handled" by removing it from the dictionary
					dict.Remove(mapObject.mapObjectId);
				}
				else
				{
					MapsExtendedEditor.instance.SpawnObject(this.content, mapObject);
				}
			}

			// Destroy map objects remaining in the dictionary since they don't exist in the new state
			foreach (var id in dict.Keys)
			{
				if (dict[id] == this.animationHandler.animation?.gameObject)
				{
					this.animationHandler.SetAnimation(null);
				}

				GameObject.DestroyImmediate(dict[id]);
			}

			this.selectedObjects.Remove(this.selectedObjects.Where(obj => obj == null).ToList());

			if (this.selectedObjects.Count == 0)
			{
				this.activeObject = null;
			}

			this.animationHandler.Refresh();
		}

		public bool CanUndo()
		{
			return this.stateHistory.CanUndo();
		}

		public bool CanRedo()
		{
			return this.stateHistory.CanRedo();
		}

		private IEnumerator OnPasteCoroutine()
		{
			if (this.clipboardMapObjects == null || this.clipboardMapObjects.Count == 0)
			{
				yield break;
			}

			int waiting = this.clipboardMapObjects.Count;
			this.ClearSelected();

			foreach (var mapObject in this.clipboardMapObjects)
			{
				MapsExtendedEditor.instance.SpawnObject(this.content, mapObject, obj =>
				{
					foreach (var handler in obj.GetComponentsInChildren<PositionHandler>())
					{
						handler.Move(new Vector3(1, -1, 0));
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
			this.UpdateRopeAttachments();

			this.TakeSnaphot();
		}

		public void CreateMapObject(Type mapObjectDataType)
		{
			this.ClearSelected();

			MapsExtendedEditor.instance.SpawnObject(this.content, mapObjectDataType, obj =>
			{
				var objectsWithHandlers = obj
					.GetComponentsInChildren<MapObjectActionHandler>()
					.Select(h => h.gameObject)
					.Distinct();

				this.AddSelected(objectsWithHandlers);
				this.ResetSpawnLabels();
				this.UpdateRopeAttachments();
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
			this.snapToGrid = enabled;
		}

		public void OnDeleteSelectedMapObjects()
		{
			foreach (var instance in this.selectedObjects.Select(obj => obj.GetComponentInParent<MapObjectInstance>().gameObject).Distinct())
			{
				if (instance == this.animationHandler.animation?.gameObject)
				{
					this.animationHandler.SetAnimation(null);
				}

				GameObject.Destroy(instance);
			}

			this.ResetSpawnLabels();
			this.ClearSelected();
			this.UpdateRopeAttachments();

			this.TakeSnaphot();
		}

		public void OnStartSimulation()
		{
			this.ClearSelected();
			this.gameObject.GetComponent<Map>().SetFieldValue("spawnPoints", null);
			this.animationHandler.enabled = false;
			this.isSimulating = true;
			this.isCreatingSelection = false;

			if (this.content.GetComponentsInChildren<SpawnPoint>().Length == 0)
			{
				MapsExtendedEditor.instance.SpawnObject(this.content, new SpawnData(), instance =>
				{
					GameObject.Destroy(instance.GetComponent<Visualizers.SpawnVisualizer>());
					this.tempSpawn = instance;
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
			MapsExtended.LoadMap(this.simulatedContent, this.GetMapData(), MapsExtended.instance.mapObjectManager, () =>
			{
				this.content.SetActive(false);
				this.simulatedContent.SetActive(true);

				GameModeManager.SetGameMode("Sandbox");
				GameModeManager.CurrentHandler.StartGame();

				var gm = (GM_Test) GameModeManager.CurrentHandler.GameMode;
				gm.testMap = true;
				gm.gameObject.GetComponentInChildren<CurveAnimation>(true).enabled = false;

				this.ExecuteAfterFrames(1, () =>
				{
					MapsExtendedEditor.instance.SetMapPhysicsActive(this.simulatedContent, true);
					this.gameObject.GetComponent<Map>().allRigs = this.simulatedContent.GetComponentsInChildren<Rigidbody2D>();

					foreach (var rope in this.simulatedContent.GetComponentsInChildren<MapObjet_Rope>())
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

			this.isSimulating = false;
			GameModeManager.SetGameMode(null);
			PlayerManager.instance.RemovePlayers();
			CardBarHandler.instance.ResetCardBards();

			if (this.tempSpawn != null)
			{
				GameObject.Destroy(this.tempSpawn);
				this.tempSpawn = null;
			}

			foreach (Transform child in this.simulatedContent.transform)
			{
				GameObject.Destroy(child.gameObject);
			}

			this.content.SetActive(true);
			this.simulatedContent.SetActive(false);
			this.animationHandler.enabled = true;
		}

		public void LoadMap(string mapFilePath)
		{
			MapsExtendedEditor.instance.LoadMap(this.content, mapFilePath);

			string personalFolder = Path.Combine(BepInEx.Paths.GameRootPath, "maps" + Path.DirectorySeparatorChar);
			string mapName = mapFilePath.Substring(0, mapFilePath.Length - 4).Replace(personalFolder, "");

			this.currentMapName = mapFilePath.StartsWith(personalFolder) ? mapName : null;

			this.ExecuteAfterFrames(1, () =>
			{
				this.stateHistory = new StateHistory(this.GetMapData());
				this.ResetSpawnLabels();
				this.ClearSelected();
				this.UpdateRopeAttachments();
			});
		}

		public void OnSelectionStart()
		{
			this.selectionStartPosition = EditorInput.mousePosition;
			this.isCreatingSelection = true;
		}

		public void OnSelectionEnd()
		{
			if (this.selectionRect.width > 2 && this.selectionRect.height > 2)
			{
				var list = EditorUtils.GetContainedActionHandlers(UIUtils.GUIToWorldRect(this.selectionRect));
				this.ClearSelected();

				// When editing animation, don't allow selecting other map objects
				if (this.animationHandler.animation != null && list.Any(obj => obj.gameObject == this.animationHandler.keyframeMapObject))
				{
					this.AddSelected(this.animationHandler.keyframeMapObject);
				}
				else if (this.animationHandler.animation == null)
				{
					this.AddSelected(list.Select(h => h.gameObject).Distinct());
				}
			}

			this.isCreatingSelection = false;
			this.selectionRect = Rect.zero;
		}

		public void OnClickActionHandlers(List<MapObjectActionHandler> handlers)
		{
			var objects = handlers.Select(h => h.gameObject).Distinct().ToList();
			objects.Sort((a, b) => a.GetInstanceID() - b.GetInstanceID());
			GameObject selectedObject = null;

			// When editing animation, don't allow selecting other map objects
			if (this.animationHandler.animation != null && objects.Any(obj => obj == this.animationHandler.keyframeMapObject))
			{
				selectedObject = this.animationHandler.keyframeMapObject;
			}
			else if (this.animationHandler.animation == null && objects.Count > 0)
			{
				selectedObject = objects[0];

				if (this.selectedObjects.Count == 1)
				{
					int currentIndex = objects.FindIndex(this.IsSelected);
					if (currentIndex != -1)
					{
						selectedObject = objects[(currentIndex + 1) % objects.Count];
					}
				}
			}

			int previouslySelectedCount = this.selectedObjects.Count;
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
			if (this.activeObject == null)
			{
				return;
			}

			foreach (var handler in this.activeObject.GetComponents<MapObjectActionHandler>())
			{
				handler.OnPointerDown();
			}
		}

		public void OnPointerUp()
		{
			if (this.activeObject == null)
			{
				return;
			}

			foreach (var handler in this.activeObject.GetComponents<MapObjectActionHandler>())
			{
				handler.OnPointerUp();
			}
		}

		public void OnKeyDown(KeyCode key)
		{
			if (this.activeObject == null)
			{
				return;
			}

			foreach (var handler in this.activeObject.GetComponents<MapObjectActionHandler>())
			{
				handler.OnKeyDown(key);
			}
		}

		public bool IsSelected(GameObject obj)
		{
			return this.selectedObjects.Contains(obj);
		}

		public Rect GetSelection()
		{
			return this.selectionRect;
		}

		private void UpdateSelection()
		{
			var mousePos = EditorInput.mousePosition;

			float width = Mathf.Abs(this.selectionStartPosition.x - mousePos.x);
			float height = Mathf.Abs(this.selectionStartPosition.y - mousePos.y);
			float x = Mathf.Min(this.selectionStartPosition.x, mousePos.x);
			float y = Screen.height - Mathf.Min(this.selectionStartPosition.y, mousePos.y) - height;

			this.selectionRect = new Rect(x, y, width, height);
		}

		public void ClearSelected()
		{
			if (this.activeObject == null)
			{
				return;
			}

			foreach (var handler in this.activeObject.GetComponents<MapObjectActionHandler>())
			{
				handler.OnDeselect();
			}

			if (this.dummyGroup != null)
			{
				GameObject.Destroy(this.dummyGroup);
			}

			this.selectedObjects.Clear();
			this.activeObject = null;
		}

		public void AddSelected(GameObject obj)
		{
			this.AddSelected(new[] { obj });
		}

		public void AddSelected(IEnumerable<GameObject> list)
		{
			if (list.Count() >= 2)
			{
				this.dummyGroup = new GameObject("Group");
				this.dummyGroup.transform.SetParent(this.content.transform);
				this.dummyGroup.SetActive(false);

				var validGroupHandlerTypes = new List<Tuple<Type, Type>>();

				/* Find valid group action handlers.
				 * A group action handler is valid if all selected map objects have
				 * all action handlers that are required by the group action handler.
				 */
				foreach (var type in this.groupActionHandlers.Keys)
				{
					var requiredTypes = this.groupActionHandlers[type];
					if (list.All(obj => requiredTypes.All(t => obj.GetComponent(t) != null)))
					{
						var handler = (IGroupMapObjectActionHandler) this.dummyGroup.AddComponent(type);
						handler.Initialize(list);
					}
				}

				this.dummyGroup.SetActive(true);
				this.activeObject = this.dummyGroup;
			}
			else
			{
				this.activeObject = list.First();
			}

			this.selectedObjects.AddRange(list);

			foreach (var handler in this.activeObject.GetComponents<MapObjectActionHandler>())
			{
				handler.OnSelect();
			}
		}

		public void SelectAll()
		{
			this.ClearSelected();

			var list = new List<GameObject>();
			foreach (Transform child in this.content.transform)
			{
				list.Add(child.gameObject);
			}

			this.AddSelected(list);
		}

		public void ResetSpawnLabels()
		{
			var spawns = this.content.GetComponentsInChildren<SpawnPoint>().ToList();
			for (int i = 0; i < spawns.Count; i++)
			{
				spawns[i].ID = i;
				spawns[i].TEAMID = i;
				spawns[i].gameObject.name = $"SPAWN POINT {i}";
			}
		}

		public void UpdateRopeAttachments()
		{
			foreach (var rope in this.content.GetComponentsInChildren<EditorRopeInstance>())
			{
				rope.UpdateAttachments();
			}
		}

		public void TakeSnaphot()
		{
			this.stateHistory.AddState(this.GetMapData());
		}
	}
}

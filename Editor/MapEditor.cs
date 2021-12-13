using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Sirenix.Serialization;
using MapsExt.UI;
using UnboundLib;
using UnboundLib.GameModes;
using MapsExt.MapObjects;
using MapsExt.Editor.MapObjects;
using MapsExt.Editor.Commands;
using MapsExt.Editor.ActionHandlers;
using System;

namespace MapsExt.Editor
{
	public class MapEditor : MonoBehaviour
	{
		public ObservableCollection<EditorActionHandler> selectedActionHandlers;
		public string currentMapName;
		public bool isSimulating;
		public bool snapToGrid;
		public CommandHistory commandHistory;
		public GameObject content;
		public MapEditorAnimationHandler animationHandler;

		public readonly Material mat = new Material(Shader.Find("Sprites/Default"));

		public float GridSize
		{
			get { return this.grid.cellSize.x; }
			set { this.grid.cellSize = Vector3.one * value; }
		}

		public MapObjectInstance[] SelectedMapObjectInstances
		{
			get
			{
				// If a map object is being animated, it's also selected
				return this.animationHandler.animation
					? new MapObjectInstance[] { this.animationHandler.animation.gameObject.GetComponent<MapObjectInstance>() }
					: this.selectedActionHandlers.Select(obj => obj.GetComponentInParent<MapObjectInstance>()).Distinct().ToArray();
			}
		}

		public GameObject[] SelectedMapObjects
		{
			get
			{
				// If a map object is being animated, it's also selected
				return this.animationHandler.animation
					? new GameObject[] { this.animationHandler.animation.gameObject }
					: this.selectedActionHandlers.Select(obj => obj.GetComponentInParent<MapObjectInstance>().gameObject).Distinct().ToArray();
			}
		}

		private bool isCreatingSelection;
		private bool isDraggingMapObjects;
		private bool isResizingMapObject;
		private bool isRotatingMapObject;
		private int resizeDirection;
		private Vector3 selectionStartPosition;
		private Rect selectionRect;
		private Vector3 prevMouse;
		private Vector3Int prevCell;
		private Dictionary<GameObject, Vector3> selectionGroupGridOffsets;
		private Dictionary<GameObject, Vector3> selectionGroupPositionOffsets;
		private List<MapObject> clipboardMapObjects;

		private Grid grid;
		private GameObject tempSpawn;
		private GameObject tempContent;

		public void Awake()
		{
			this.selectedActionHandlers = new ObservableCollection<EditorActionHandler>();
			this.snapToGrid = true;
			this.isCreatingSelection = false;
			this.isDraggingMapObjects = false;
			this.isResizingMapObject = false;
			this.isRotatingMapObject = false;
			this.currentMapName = null;
			this.isSimulating = false;
			this.selectionGroupGridOffsets = new Dictionary<GameObject, Vector3>();
			this.selectionGroupPositionOffsets = new Dictionary<GameObject, Vector3>();

			var animationContainer = new GameObject("Animation Handler");
			animationContainer.transform.SetParent(this.transform);
			this.animationHandler = animationContainer.AddComponent<MapEditorAnimationHandler>();
			this.animationHandler.editor = this;

			var commandHandlerProvider = new CommandHandlerProvider();
			commandHandlerProvider.RegisterHandler(new CreateCommandHandler(this));
			commandHandlerProvider.RegisterHandler(new DeleteCommandHandler(this));
			commandHandlerProvider.RegisterHandler(new MoveCommandHandler(this));
			commandHandlerProvider.RegisterHandler(new ResizeCommandHandler(this));
			commandHandlerProvider.RegisterHandler(new RotateCommandHandler(this));
			commandHandlerProvider.RegisterHandler(new AddKeyframeCommandHandler(this));
			commandHandlerProvider.RegisterHandler(new DeleteKeyframeCommandHandler(this));
			commandHandlerProvider.RegisterHandler(new ChangeKeyframeDurationCommandHandler(this));
			commandHandlerProvider.RegisterHandler(new ChangeKeyframeEasingCommandHandler(this));

			this.commandHistory = new CommandHistory(commandHandlerProvider);

			this.content = new GameObject("Content");
			this.content.transform.SetParent(this.transform);

			this.tempContent = new GameObject("Simulated Content");
			this.tempContent.transform.SetParent(this.transform);
			this.tempContent.SetActive(false);

			this.gameObject.AddComponent<MapEditorInputHandler>();

			var gridGo = new GameObject("MapEditorGrid");
			gridGo.transform.SetParent(this.transform);

			this.grid = gridGo.AddComponent<Grid>();
			this.grid.cellSize = Vector3.one;

			var uiGo = new GameObject("MapEditorUI");
			uiGo.transform.SetParent(this.transform);

			uiGo.SetActive(false);
			uiGo.AddComponent<MapEditorUI>().editor = this;
			uiGo.SetActive(true);

			this.gameObject.AddComponent<MapBorder>();
		}

		public void Update()
		{
			if (this.isDraggingMapObjects)
			{
				this.DragMapObjects();
			}

			if (this.isCreatingSelection)
			{
				this.UpdateSelection();
			}

			if (this.isResizingMapObject)
			{
				this.ResizeMapObject();
			}

			if (this.isRotatingMapObject)
			{
				this.RotateMapObject();
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
				mapObjects = new List<MapObject>()
			};

			var mapObjects = this.content.GetComponentsInChildren<MapObjectInstance>(true);

			foreach (var mapObject in mapObjects)
			{
				if (mapObject.gameObject == this.animationHandler.keyframeMapObject)
				{
					continue;
				}

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
			this.clipboardMapObjects = new List<MapObject>();
			var mapObjectInstances = this.selectedActionHandlers
				.Select(obj => obj.GetComponent<MapObjectInstance>() ?? obj.GetComponentInParent<MapObjectInstance>())
				.Distinct();

			foreach (var instance in mapObjectInstances)
			{
				this.clipboardMapObjects.Add(MapsExtendedEditor.instance.mapObjectManager.Serialize(instance));
			}
		}

		public void OnPaste()
		{
			if (this.clipboardMapObjects == null || this.clipboardMapObjects.Count == 0)
			{
				return;
			}

			var cmd = new CreateCommand(this.clipboardMapObjects.Select(data => data.Move(new Vector3(1, -1, 0))));
			this.commandHistory.Add(cmd);
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
			var cmd = new DeleteCommand(this.SelectedMapObjectInstances);
			this.commandHistory.Add(cmd);
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
				MapsExtendedEditor.instance.SpawnObject(this.content, new Spawn(), instance =>
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
			MapsExtended.LoadMap(this.tempContent, this.GetMapData(), MapsExtended.instance.mapObjectManager, () =>
			{
				this.content.SetActive(false);
				this.tempContent.SetActive(true);

				GameModeManager.SetGameMode("Sandbox");
				GameModeManager.CurrentHandler.StartGame();

				var gm = (GM_Test) GameModeManager.CurrentHandler.GameMode;
				gm.testMap = true;
				gm.gameObject.GetComponentInChildren<CurveAnimation>(true).enabled = false;

				this.ExecuteAfterFrames(1, () =>
				{
					MapsExtendedEditor.instance.SetMapPhysicsActive(this.tempContent, true);
					this.gameObject.GetComponent<Map>().allRigs = this.tempContent.GetComponentsInChildren<Rigidbody2D>();

					var ropes = this.tempContent.GetComponentsInChildren<MapObjet_Rope>();
					foreach (var rope in ropes)
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

			this.content.SetActive(true);
			this.tempContent.SetActive(false);
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
				this.ResetSpawnLabels();
				this.ClearSelected();
				this.UpdateRopeAttachments();
			});
		}

		public void OnSelectionStart()
		{
			this.selectionStartPosition = Input.mousePosition;
			this.isCreatingSelection = true;
		}

		public void OnSelectionEnd()
		{
			if (this.selectionRect.width > 2 && this.selectionRect.height > 2)
			{
				var list = EditorUtils.GetContainedActionHandlers(UIUtils.GUIToWorldRect(this.selectionRect));
				this.ClearSelected();

				// When editing animation, don't allow selecting other map objects
				if (this.animationHandler.animation != null && list.Any(obj => obj == this.animationHandler.keyframeMapObject))
				{
					this.AddSelected(this.animationHandler.keyframeMapObject.GetComponent<EditorActionHandler>());
				}
				else if (this.animationHandler.animation == null)
				{
					this.AddSelected(list.SelectMany(go => go.GetComponentsInChildren<EditorActionHandler>()));
				}
			}

			this.isCreatingSelection = false;
			this.selectionRect = Rect.zero;
		}

		public void OnDragStart()
		{
			var mousePos = Input.mousePosition;
			var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));

			this.prevMouse = mouseWorldPos;
			this.isDraggingMapObjects = true;

			this.selectionGroupGridOffsets.Clear();
			this.selectionGroupPositionOffsets.Clear();

			if (this.selectedActionHandlers.Count == 1)
			{
				var handler = this.selectedActionHandlers[0];

				var referenceRotation = handler.transform.rotation;
				var referenceAngles = referenceRotation.eulerAngles;
				referenceRotation.eulerAngles = new Vector3(referenceAngles.x, referenceAngles.y, referenceAngles.z % 90);
				this.grid.transform.rotation = referenceRotation;

				var scaleOffset = Vector3.zero;
				var objectCell = this.grid.WorldToCell(handler.transform.position);
				var snappedPosition = this.grid.CellToWorld(objectCell);

				if (snappedPosition != handler.transform.position)
				{
					var diff = handler.transform.position - snappedPosition;
					var identityDiff = Quaternion.Inverse(referenceRotation) * diff;
					var identityDelta = new Vector3(this.GridSize / 2f, this.GridSize / 2f, 0);

					const float eps = 0.000005f;

					if ((Mathf.Abs(identityDiff.x) < eps || Mathf.Abs(identityDiff.x - identityDelta.x) < eps) &&
						(Mathf.Abs(identityDiff.y) < eps || Mathf.Abs(identityDiff.y - identityDelta.y) < eps))
					{
						scaleOffset = diff;
					}
				}

				this.selectionGroupGridOffsets.Add(handler.gameObject, Vector3.zero);
				this.selectionGroupPositionOffsets.Add(handler.gameObject, scaleOffset);
			}
			else
			{
				this.grid.transform.rotation = Quaternion.identity;

				foreach (var handler in this.selectedActionHandlers)
				{
					var objectCell = this.grid.WorldToCell(handler.transform.position);
					var snappedPosition = this.grid.CellToWorld(objectCell);
					this.selectionGroupGridOffsets.Add(handler.gameObject, handler.transform.position - snappedPosition);
					this.selectionGroupPositionOffsets.Add(handler.gameObject, Vector3.zero);
				}
			}

			this.prevCell = this.grid.WorldToCell(mouseWorldPos);
			this.DetachSelectedRopes();
			this.commandHistory.PreventNextMerge();
		}

		public void OnDragEnd()
		{
			this.isDraggingMapObjects = false;
			this.UpdateRopeAttachments();
		}

		public void OnClickActionHandlers(List<EditorActionHandler> handlers)
		{
			handlers.Sort((a, b) => a.GetInstanceID() - b.GetInstanceID());
			EditorActionHandler handler = null;

			// When editing animation, don't allow selecting other map objects
			if (this.animationHandler.animation != null && handlers.Any(obj => obj == this.animationHandler.keyframeMapObject))
			{
				handler = this.animationHandler.keyframeMapObject.GetComponent<EditorActionHandler>();
			}
			else if (this.animationHandler.animation == null && handlers.Count > 0)
			{
				handler = handlers[0];

				if (this.selectedActionHandlers.Count == 1)
				{
					int currentIndex = handlers.FindIndex(this.IsActionHandlerSelected);
					if (currentIndex != -1)
					{
						handler = handlers[(currentIndex + 1) % handlers.Count];
					}
				}
			}

			int previouslySelectedCount = this.selectedActionHandlers.Count;
			bool clickedMapObjectIsSelected = this.IsActionHandlerSelected(handler);
			this.ClearSelected();

			if (handler == null)
			{
				return;
			}

			bool changeMultiSelectionToSingle = clickedMapObjectIsSelected && previouslySelectedCount > 1;
			bool selectUnselected = !clickedMapObjectIsSelected;

			if (changeMultiSelectionToSingle || selectUnselected)
			{
				this.AddSelected(handler.GetComponentsInChildren<EditorActionHandler>());
			}
		}

		public void OnResizeStart(int resizeDirection)
		{
			var mousePos = Input.mousePosition;
			var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));

			this.grid.transform.rotation = this.SelectedMapObjects[0].transform.rotation;

			this.isResizingMapObject = true;
			this.resizeDirection = resizeDirection;
			this.prevMouse = mouseWorldPos;
			this.prevCell = this.grid.WorldToCell(mouseWorldPos);
			this.commandHistory.PreventNextMerge();
		}

		public void OnResizeEnd()
		{
			this.isResizingMapObject = false;
			this.UpdateRopeAttachments();
		}

		public void OnRotateStart()
		{
			var mousePos = Input.mousePosition;
			var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));

			this.isRotatingMapObject = true;
			this.prevMouse = mouseWorldPos;
			this.commandHistory.PreventNextMerge();
		}

		public void OnRotateEnd()
		{
			this.isRotatingMapObject = false;
			this.UpdateRopeAttachments();
		}

		public void OnNudgeSelectedMapObjects(Vector2 delta)
		{
			if (this.SelectedMapObjects.Length > 0)
			{
				var cmd = new MoveCommand(this.selectedActionHandlers, (Vector3) delta, this.animationHandler.KeyframeIndex);
				this.commandHistory.Add(cmd);
			}
		}

		public bool IsActionHandlerSelected(EditorActionHandler handler)
		{
			return this.selectedActionHandlers.Contains(handler);
		}

		public Rect GetSelection()
		{
			return this.selectionRect;
		}

		private void UpdateSelection()
		{
			var mousePos = Input.mousePosition;

			float width = Mathf.Abs(this.selectionStartPosition.x - mousePos.x);
			float height = Mathf.Abs(this.selectionStartPosition.y - mousePos.y);
			float x = Mathf.Min(this.selectionStartPosition.x, mousePos.x);
			float y = Screen.height - Mathf.Min(this.selectionStartPosition.y, mousePos.y) - height;

			this.selectionRect = new Rect(x, y, width, height);
		}

		private void DragMapObjects()
		{
			var mousePos = Input.mousePosition;
			var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));
			var mouseCell = this.grid.WorldToCell(mouseWorldPos);
			var mouseDelta = mouseWorldPos - this.prevMouse;
			var cellDelta = mouseCell - this.prevCell;

			var delta = mouseDelta;

			if (this.snapToGrid)
			{
				var handler = this.selectedActionHandlers[0];
				var groupOffset = this.selectionGroupGridOffsets[handler.gameObject];
				var positionOffset = this.selectionGroupPositionOffsets[handler.gameObject];
				var objectCell = this.grid.WorldToCell(handler.transform.position);
				var snappedPosition = this.grid.CellToWorld(objectCell + cellDelta);

				delta = (snappedPosition + groupOffset + positionOffset) - handler.transform.position;
			}

			if (delta != Vector3.zero)
			{
				var cmd = new MoveCommand(this.selectedActionHandlers, delta, this.animationHandler.KeyframeIndex);
				this.commandHistory.Add(cmd, true);
			}

			this.prevMouse += mouseDelta;
			this.prevCell = mouseCell;
		}

		private void ResizeMapObject()
		{
			var mousePos = Input.mousePosition;
			var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));
			var mouseCell = this.grid.WorldToCell(mouseWorldPos);
			var mouseDelta = mouseWorldPos - this.prevMouse;
			Vector3 cellDelta = mouseCell - this.prevCell;

			var sizeDelta = this.snapToGrid
				? cellDelta * this.GridSize
				: Quaternion.Inverse(this.grid.transform.rotation) * mouseDelta;

			if (sizeDelta != Vector3.zero)
			{
				var cmd = new ResizeCommand(this.selectedActionHandlers, sizeDelta, this.resizeDirection, this.animationHandler.KeyframeIndex);
				this.commandHistory.Add(cmd, true);

				this.prevMouse += mouseDelta;
				this.prevCell = mouseCell;
			}
		}

		private void RotateMapObject()
		{
			var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
			var handler = this.selectedActionHandlers[0];

			var mousePos = mouseWorldPos;
			var objectPos = handler.transform.position;
			mousePos.x -= objectPos.x;
			mousePos.y -= objectPos.y;

			float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg - 90;
			angle = EditorUtils.Snap(angle, this.snapToGrid ? 15f : 2f);
			var toRotation = Quaternion.AngleAxis(angle, Vector3.forward);

			var cmd = new RotateCommand(this.selectedActionHandlers, handler.transform.rotation, toRotation, this.animationHandler.KeyframeIndex);
			this.commandHistory.Add(cmd, true);
		}

		public void ClearSelected()
		{
			this.selectedActionHandlers.Clear();
		}

		public void AddSelected(IEnumerable<EditorActionHandler> list)
		{
			foreach (var handler in list)
			{
				this.AddSelected(handler);
			}
		}

		public void AddSelected(EditorActionHandler handler)
		{
			this.selectedActionHandlers.Add(handler);
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

		private void DetachSelectedRopes()
		{
			var anchors = this.selectedActionHandlers
				.Select(obj => obj.GetComponent<MapObjectAnchor>())
				.Where(handler => handler != null);

			foreach (var anchor in anchors)
			{
				anchor.Detach();
			}
		}
	}
}

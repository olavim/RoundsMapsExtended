using System.IO;
using System.Collections;
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
using System;

namespace MapsExt.Editor
{
	public class MapEditor : MonoBehaviour
	{
		public ObservableCollection<GameObject> selectedMapObjects;
		public string currentMapName;
		public bool isSimulating;
		public bool snapToGrid;
		public InteractionTimeline timeline;
		public GameObject content;
		public MapEditorAnimationHandler animationHandler;

		public readonly Material mat = new Material(Shader.Find("Sprites/Default"));

		public float GridSize
		{
			get { return this.grid.cellSize.x; }
			set { this.grid.cellSize = Vector3.one * value; }
		}

		public IEnumerable<MapObjectInstance> SelectedMapObjectInstances
		{
			get
			{
				// If a map object is being animated, it's also selected
				return this.animationHandler.animation
					? new MapObjectInstance[] { this.animationHandler.animation.gameObject.GetComponent<MapObjectInstance>() }
					: this.selectedMapObjects.Select(obj => obj.GetComponentInParent<MapObjectInstance>()).Distinct().ToArray();
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
		private MapEditorUI gui;
		private GameObject tempSpawn;
		private GameObject tempContent;

		public void Awake()
		{
			this.selectedMapObjects = new ObservableCollection<GameObject>();
			this.snapToGrid = true;
			this.isCreatingSelection = false;
			this.isDraggingMapObjects = false;
			this.isResizingMapObject = false;
			this.isRotatingMapObject = false;
			this.currentMapName = null;
			this.isSimulating = false;
			this.selectionGroupGridOffsets = new Dictionary<GameObject, Vector3>();
			this.selectionGroupPositionOffsets = new Dictionary<GameObject, Vector3>();
			this.timeline = new InteractionTimeline(MapsExtendedEditor.instance.mapObjectManager);

			var animationContainer = new GameObject("Animation Handler");
			animationContainer.transform.SetParent(this.transform);
			this.animationHandler = animationContainer.AddComponent<MapEditorAnimationHandler>();
			this.animationHandler.editor = this;

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
			var mapObjects = this.content.GetComponentsInChildren<MapObjectInstance>();

			foreach (var mapObject in mapObjects)
			{
				mapData.mapObjects.Add(MapsExtendedEditor.instance.mapObjectManager.Serialize(mapObject));
			}

			return mapData;
		}

		public void SpawnMapObject(Type type)
		{
			MapsExtendedEditor.instance.SpawnObject(this.content, type, instance =>
			{
				instance.SetActive(false);

				float scaleStep = Mathf.Max(1, this.GridSize * 2f);
				instance.transform.localScale = EditorUtils.SnapToGrid(instance.transform.localScale, scaleStep);
				instance.transform.position = Vector3.zero;

				var damageable = instance.GetComponent<DamagableEvent>();
				if (damageable != null)
				{
					damageable.disabled = true;
				}

				this.BeginInteraction(new MapObjectInstance[] { instance.GetComponent<MapObjectInstance>() }, true);
				instance.SetActive(true);
				this.EndInteraction();

				this.ClearSelected();
				this.AddSelected(instance);

				this.ResetSpawnLabels();
				this.UpdateRopeAttachments();
			});
		}

		public void Undo()
		{
			var animObject = this.animationHandler.animation?.gameObject;
			var keyframeCount = this.animationHandler.animation?.keyframes.Count;
			var interaction = this.timeline.Undo();

			if (interaction == null)
			{
				return;
			}

			if (animObject)
			{
				var transition = interaction.GetTransition(animObject);

				if (transition != null)
				{
					var fromState = (SpatialMapObject) transition.fromState;
					var toState = (SpatialMapObject) transition.toState;

					// If the map object being animated was destroyed because of undo, close animation windows
					if (!fromState.active && toState.active)
					{
						this.animationHandler.SetAnimation(null);
					}

					// If the last keyframe was destroyed because of undo, select the new last keyframe
					if (
						fromState.animationKeyframes.Count < toState.animationKeyframes.Count &&
						this.animationHandler.Keyframe >= this.animationHandler.animation.keyframes.Count
					)
					{
						this.animationHandler.SetKeyframe(this.animationHandler.animation.keyframes.Count - 1);
					}
				}

				animObject.SetActive(false);
			}

			this.ResetSpawnLabels();
			this.ClearSelected();
			this.UpdateRopeAttachments();

			this.animationHandler.RefreshCurrentFrame();
		}

		public void Redo()
		{
			var animObject = this.animationHandler.animation?.gameObject;
			var interaction = this.timeline.Redo();

			if (interaction == null)
			{
				return;
			}

			if (animObject)
			{
				var transition = interaction.GetTransition(animObject);

				if (transition != null)
				{
					var fromState = (SpatialMapObject) transition.fromState;
					var toState = (SpatialMapObject) transition.toState;

					// If the map object being animated was destroyed because of redo, close animation windows
					if (fromState.active && !toState.active)
					{
						this.animationHandler.SetAnimation(null);
					}

					// If a new keyframe was created because of redo, select the new keyframe
					if (fromState.animationKeyframes.Count < toState.animationKeyframes.Count)
					{
						this.animationHandler.SetKeyframe(this.animationHandler.animation.keyframes.Count - 1);
					}
				}

				animObject.SetActive(false);
			}

			this.ResetSpawnLabels();
			this.ClearSelected();
			this.UpdateRopeAttachments();

			this.animationHandler.RefreshCurrentFrame();
		}

		public void BeginInteraction(IEnumerable<MapObjectInstance> objects, bool isCreateInteraction = false)
		{
			// If a map object is being animated, we hide it in the editor but in reality it's active
			var animObject = this.animationHandler.animation?.gameObject;
			animObject?.SetActive(true);
			this.timeline.BeginInteraction(objects, isCreateInteraction);
			animObject?.SetActive(false);
		}

		public void EndInteraction()
		{
			// If a map object is being animated, we hide it in the editor but in reality it's active
			var animObject = this.animationHandler.animation?.gameObject;
			animObject?.SetActive(true);
			this.timeline.EndInteraction();
			animObject?.SetActive(false);
		}

		public void OnCopy()
		{
			this.clipboardMapObjects = new List<MapObject>();
			var mapObjectInstances = this.selectedMapObjects
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

			this.StartCoroutine(this.SpawnClipboardObjects());
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

		private IEnumerator SpawnClipboardObjects()
		{
			int waitingForSpawns = this.clipboardMapObjects.Count;
			var newInstances = new List<GameObject>();

			foreach (var data in this.clipboardMapObjects)
			{
				MapsExtendedEditor.instance.SpawnObject(this.content, data.Move(new Vector3(1, -1, 0)), instance =>
				{
					instance.SetActive(false);
					newInstances.Add(instance);
				});
			}

			while (newInstances.Count != waitingForSpawns)
			{
				yield return null;
			}

			var actionHandlers = newInstances
				.SelectMany(obj => obj.GetComponentsInChildren<EditorActionHandler>())
				.Select(h => h.gameObject);

			this.ClearSelected();
			this.AddSelected(actionHandlers);

			this.BeginInteraction(newInstances.Select(obj => obj.GetComponent<MapObjectInstance>()), true);
			foreach (var obj in newInstances)
			{
				obj.SetActive(true);
			}
			this.EndInteraction();

			this.ExecuteAfterFrames(1, () =>
			{
				this.ResetSpawnLabels();
				this.UpdateRopeAttachments();
			});
		}

		public void OnToggleSnapToGrid(bool enabled)
		{
			this.snapToGrid = enabled;
		}

		public void OnDeleteSelectedMapObjects()
		{
			this.BeginInteraction(this.SelectedMapObjectInstances);
			foreach (var obj in this.SelectedMapObjectInstances)
			{
				obj.gameObject.SetActive(false);
			}
			this.EndInteraction();

			this.ResetSpawnLabels();
			this.ClearSelected();
			this.UpdateRopeAttachments();
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
				var list = EditorUtils.GetContainedMapObjects(UIUtils.GUIToWorldRect(this.selectionRect));
				this.ClearSelected();

				// When editing animation, don't allow selecting other map objects
				if (this.animationHandler.animation != null && list.Any(obj => obj == this.animationHandler.keyframeMapObject))
				{
					this.AddSelected(this.animationHandler.keyframeMapObject);
				}
				else if (this.animationHandler.animation == null)
				{
					this.AddSelected(list);
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

			if (this.selectedMapObjects.Count == 1)
			{
				var obj = this.selectedMapObjects[0];

				var referenceRotation = obj.transform.rotation;
				var referenceAngles = referenceRotation.eulerAngles;
				referenceRotation.eulerAngles = new Vector3(referenceAngles.x, referenceAngles.y, referenceAngles.z % 90);
				this.grid.transform.rotation = referenceRotation;

				var scaleOffset = Vector3.zero;
				var objectCell = this.grid.WorldToCell(obj.transform.position);
				var snappedPosition = this.grid.CellToWorld(objectCell);

				if (snappedPosition != obj.transform.position)
				{
					var diff = obj.transform.position - snappedPosition;
					var identityDiff = Quaternion.Inverse(referenceRotation) * diff;
					var identityDelta = new Vector3(this.GridSize / 2f, this.GridSize / 2f, 0);

					const float eps = 0.000005f;

					if ((Mathf.Abs(identityDiff.x) < eps || Mathf.Abs(identityDiff.x - identityDelta.x) < eps) &&
						(Mathf.Abs(identityDiff.y) < eps || Mathf.Abs(identityDiff.y - identityDelta.y) < eps))
					{
						scaleOffset = diff;
					}
				}

				this.selectionGroupGridOffsets.Add(obj, Vector3.zero);
				this.selectionGroupPositionOffsets.Add(obj, scaleOffset);
			}
			else
			{
				this.grid.transform.rotation = Quaternion.identity;

				foreach (var mapObject in this.selectedMapObjects)
				{
					var objectCell = this.grid.WorldToCell(mapObject.transform.position);
					var snappedPosition = this.grid.CellToWorld(objectCell);
					this.selectionGroupGridOffsets.Add(mapObject, mapObject.transform.position - snappedPosition);
					this.selectionGroupPositionOffsets.Add(mapObject, Vector3.zero);
				}
			}

			this.prevCell = this.grid.WorldToCell(mouseWorldPos);
			this.BeginInteraction(this.SelectedMapObjectInstances);

			this.DetachSelectedRopes();
		}

		public void OnDragEnd()
		{
			this.isDraggingMapObjects = false;
			this.EndInteraction();

			this.UpdateRopeAttachments();
		}

		public void OnClickMapObjects(List<GameObject> mapObjects)
		{
			mapObjects.Sort((a, b) => a.GetInstanceID() - b.GetInstanceID());
			GameObject mapObject = null;

			// When editing animation, don't allow selecting other map objects
			if (this.animationHandler.animation != null && mapObjects.Any(obj => obj == this.animationHandler.keyframeMapObject))
			{
				mapObject = this.animationHandler.keyframeMapObject;
			}
			else if (this.animationHandler.animation == null && mapObjects.Count > 0)
			{
				mapObject = mapObjects[0];

				if (this.selectedMapObjects.Count == 1)
				{
					int currentIndex = mapObjects.FindIndex(this.IsMapObjectSelected);
					if (currentIndex != -1)
					{
						mapObject = mapObjects[(currentIndex + 1) % mapObjects.Count];
					}
				}
			}

			int previouslySelectedCount = this.selectedMapObjects.Count;
			bool clickedMapObjectIsSelected = this.IsMapObjectSelected(mapObject);
			this.ClearSelected();

			if (mapObject == null)
			{
				return;
			}

			bool changeMultiSelectionToSingle = clickedMapObjectIsSelected && previouslySelectedCount > 1;
			bool selectUnselected = !clickedMapObjectIsSelected;

			if (changeMultiSelectionToSingle || selectUnselected)
			{
				this.AddSelected(mapObject);
			}
		}

		public void OnResizeStart(int resizeDirection)
		{
			var mousePos = Input.mousePosition;
			var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));

			this.grid.transform.rotation = this.selectedMapObjects[0].transform.rotation;

			this.isResizingMapObject = true;
			this.resizeDirection = resizeDirection;
			this.prevMouse = mouseWorldPos;
			this.prevCell = this.grid.WorldToCell(mouseWorldPos);

			this.BeginInteraction(this.SelectedMapObjectInstances);
		}

		public void OnResizeEnd()
		{
			this.isResizingMapObject = false;
			this.EndInteraction();

			this.UpdateRopeAttachments();
		}

		public void OnRotateStart()
		{
			var mousePos = Input.mousePosition;
			var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));

			this.isRotatingMapObject = true;
			this.prevMouse = mouseWorldPos;

			this.BeginInteraction(this.SelectedMapObjectInstances);
		}

		public void OnRotateEnd()
		{
			this.isRotatingMapObject = false;
			this.EndInteraction();
		}

		public void OnNudgeSelectedMapObjects(Vector2 delta)
		{
			if (this.selectedMapObjects.Count > 0)
			{
				this.BeginInteraction(this.SelectedMapObjectInstances);

				foreach (var obj in this.selectedMapObjects)
				{
					obj.transform.position += new Vector3(delta.x, delta.y, 0);
				}

				this.EndInteraction();
			}
		}

		public bool IsMapObjectSelected(GameObject obj)
		{
			return this.selectedMapObjects.Contains(obj);
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

			foreach (var obj in this.selectedMapObjects)
			{
				if (this.snapToGrid)
				{
					Vector3 groupOffset = this.selectionGroupGridOffsets[obj];
					Vector3 positionOffset = this.selectionGroupPositionOffsets[obj];
					var objectCell = this.grid.WorldToCell(obj.transform.position);
					var snappedPosition = this.grid.CellToWorld(objectCell + cellDelta);

					obj.transform.position = snappedPosition + groupOffset + positionOffset;
				}
				else
				{
					obj.transform.position += mouseDelta;
				}

				foreach (var handler in obj.GetComponentsInChildren<EditorActionHandler>())
				{
					handler.onAction?.Invoke();
					handler.onMove?.Invoke();
				}
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
				bool resized = false;
				foreach (var mapObject in this.selectedMapObjects)
				{
					if (mapObject.GetComponent<EditorActionHandler>().Resize(sizeDelta, this.resizeDirection))
					{
						resized = true;

						foreach (var handler in mapObject.GetComponentsInChildren<EditorActionHandler>())
						{
							handler.onAction?.Invoke();
							handler.onResize?.Invoke();
						}
					}
				}

				if (resized)
				{
					this.prevMouse += mouseDelta;
					this.prevCell = mouseCell;
				}
			}
		}

		private void RotateMapObject()
		{
			var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));

			foreach (var mapObject in this.selectedMapObjects)
			{
				var mousePos = mouseWorldPos;
				var objectPos = mapObject.transform.position;
				mousePos.x -= objectPos.x;
				mousePos.y -= objectPos.y;

				float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg - 90;

				if (this.snapToGrid)
				{
					angle = EditorUtils.Snap(angle, 15f);
				}

				mapObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

				foreach (var handler in mapObject.GetComponentsInChildren<EditorActionHandler>())
				{
					handler.onAction?.Invoke();
					handler.onRotate?.Invoke();
				}
			}
		}

		public void ClearSelected()
		{
			this.selectedMapObjects.Clear();
		}

		public void AddSelected(IEnumerable<GameObject> list)
		{
			foreach (var go in list)
			{
				this.AddSelected(go);
			}
		}

		public void AddSelected(GameObject obj)
		{
			this.selectedMapObjects.Add(obj);
		}

		private void ResetSpawnLabels()
		{
			var spawns = this.content.GetComponentsInChildren<SpawnPoint>().ToList();
			for (int i = 0; i < spawns.Count; i++)
			{
				spawns[i].ID = i;
				spawns[i].TEAMID = i;
				spawns[i].gameObject.name = $"SPAWN POINT {i}";
			}
		}

		private void UpdateRopeAttachments()
		{
			foreach (var rope in this.content.GetComponentsInChildren<EditorRopeInstance>())
			{
				rope.UpdateAttachments();
			}
		}

		private void DetachSelectedRopes()
		{
			var anchors = this.selectedMapObjects
				.Select(obj => obj.GetComponent<MapObjectAnchor>())
				.Where(handler => handler != null);

			foreach (var anchor in anchors)
			{
				anchor.Detach();
			}
		}
	}
}

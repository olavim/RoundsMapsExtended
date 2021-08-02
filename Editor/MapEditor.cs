using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.Serialization;
using MapsExtended.UI;
using MapsExtended.Visualizers;
using UnboundLib;
using UnboundLib.GameModes;

namespace MapsExtended.Editor
{
    public class MapEditor : MonoBehaviour
    {
        public List<GameObject> selectedMapObjects;
        public string currentMapName;
        public bool isSimulating;
        public bool snapToGrid;
        public InteractionTimeline timeline;

        public float GridSize
        {
            get { return this.grid.cellSize.x; }
            set { this.grid.cellSize = Vector3.one * value; }
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
        private List<MapObjectData> clipboardMapObjects;
        private List<SpawnPointData> clipboardSpawns;

        private Grid grid;
        private MapEditorUI gui;
        private GameObject tempSpawn;

        public void Awake()
        {
            this.selectedMapObjects = new List<GameObject>();
            this.snapToGrid = true;
            this.isCreatingSelection = false;
            this.isDraggingMapObjects = false;
            this.isResizingMapObject = false;
            this.isRotatingMapObject = false;
            this.currentMapName = null;
            this.isSimulating = false;
            this.selectionGroupGridOffsets = new Dictionary<GameObject, Vector3>();
            this.selectionGroupPositionOffsets = new Dictionary<GameObject, Vector3>();
            this.timeline = new InteractionTimeline();

            this.gameObject.AddComponent<MapEditorInputHandler>();

            var gridGo = new GameObject("MapEditorGrid");
            gridGo.transform.SetParent(this.transform);

            this.grid = gridGo.AddComponent<Grid>();
            this.grid.cellSize = Vector3.one;

            var uiGo = new GameObject("MapEditorUI");
            uiGo.transform.SetParent(this.transform);

            uiGo.SetActive(false);
            this.gui = uiGo.AddComponent<MapEditorUI>();
            this.gui.editor = this;
            uiGo.SetActive(true);
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
            var mapData = new CustomMap
            {
                mapObjects = new List<MapObjectData>(),
                spawns = new List<SpawnPointData>()
            };

            var mapObjects = this.gameObject.GetComponentsInChildren<MapObject>();
            var spawns = this.gameObject.GetComponentsInChildren<SpawnPoint>();

            foreach (var mapObject in mapObjects)
            {
                mapData.mapObjects.Add(new MapObjectData(mapObject));
            }

            foreach (var spawn in spawns)
            {
                mapData.spawns.Add(new SpawnPointData(spawn));
            }

            var bytes = SerializationUtility.SerializeValue(mapData, DataFormat.JSON);

            string path = filename == null
                ? Path.GetTempFileName()
                : Path.Combine(Path.Combine(BepInEx.Paths.GameRootPath, "maps"), filename + ".map");

            File.WriteAllBytes(path, bytes);

            this.currentMapName = filename;
        }

        public void SpawnMapObject(string mapObjectName)
        {
            EditorMod.instance.SpawnObject(this.gameObject.GetComponent<Map>(), mapObjectName, instance =>
            {
                instance.SetActive(false);
                instance.transform.localScale = EditorUtils.SnapToGrid(instance.transform.localScale, this.GridSize * 2f);
                instance.transform.position = Vector3.zero;

                this.timeline.BeginInteraction(instance, true);
                instance.SetActive(true);
                this.timeline.EndInteraction();
            });
        }

        public void AddSpawn()
        {
            var spawn = EditorMod.instance.AddSpawn(this.gameObject.GetComponent<Map>());
            spawn.SetActive(false);
            spawn.transform.position = Vector3.zero;

            this.timeline.BeginInteraction(spawn, true);
            spawn.SetActive(true);
            this.timeline.EndInteraction();
        }

        public void OnUndo()
        {
            if (this.timeline.Undo())
            {
                this.ResetSpawnLabels();
                this.ClearSelected();
            }

        }

        public void OnRedo()
        {
            if (this.timeline.Redo())
            {
                this.ResetSpawnLabels();
                this.ClearSelected();
            }
        }

        public void OnCopy()
        {
            this.clipboardMapObjects = new List<MapObjectData>();
            this.clipboardSpawns = new List<SpawnPointData>();

            foreach (var obj in this.selectedMapObjects)
            {
                var mapObject = obj.GetComponent<MapObject>();
                var spawn = obj.GetComponent<SpawnPoint>();

                if (mapObject)
                {
                    this.clipboardMapObjects.Add(new MapObjectData(mapObject));
                }

                if (spawn)
                {
                    this.clipboardSpawns.Add(new SpawnPointData(spawn));
                }
            }
        }

        public void OnPaste()
        {
            if (this.clipboardMapObjects == null || 
                this.clipboardSpawns == null || 
                (this.clipboardMapObjects.Count == 0 && this.clipboardSpawns.Count == 0))
            {
                return;
            }

            this.ClearSelected();

            this.StartCoroutine(this.SpawnClipboardObjects());
        }

        private IEnumerator SpawnClipboardObjects()
        {
            int waitingForSpawns = this.clipboardMapObjects.Count + this.clipboardSpawns.Count;
            var map = this.gameObject.GetComponent<Map>();
            var newInstances = new List<GameObject>();

            foreach (var data in this.clipboardMapObjects)
            {
                EditorMod.instance.SpawnObject(map, data.mapObjectName, instance =>
                {
                    instance.SetActive(false);
                    instance.transform.position = data.position + new Vector3(1, -1, 0);
                    instance.transform.localScale = data.scale;
                    instance.transform.rotation = data.rotation;

                    newInstances.Add(instance);
                });
            }

            foreach (var data in this.clipboardSpawns)
            {
                var instance = EditorMod.instance.AddSpawn(map);
                instance.SetActive(false);
                instance.transform.position = data.position + new Vector3(1, -1, 0);

                newInstances.Add(instance);
            }

            while (newInstances.Count != waitingForSpawns)
            {
                yield return null;
            }

            this.AddSelected(newInstances);

            this.timeline.BeginInteraction(this.selectedMapObjects, true);
            foreach (var obj in this.selectedMapObjects)
            {
                obj.SetActive(true);
            }
            this.timeline.EndInteraction();

            this.ResetSpawnLabels();
        }

        public void OnToggleSnapToGrid(bool enabled)
        {
            this.snapToGrid = enabled;
        }

        public void OnDeleteSelectedMapObjects()
        {
            this.timeline.BeginInteraction(this.selectedMapObjects);
            foreach (var obj in this.selectedMapObjects)
            {
                obj.SetActive(false);
            }
            this.timeline.EndInteraction();

            this.ResetSpawnLabels();
            this.ClearSelected();
        }

        public void OnStartSimulation()
        {
            this.ClearSelected();
            this.gameObject.GetComponent<Map>().SetFieldValue("spawnPoints", null);

            this.isSimulating = true;
            this.isCreatingSelection = false;

            var objects = new List<GameObject>();
            objects.AddRange(this.gameObject.GetComponentsInChildren<MapObject>(true).Select(o => o.gameObject));
            objects.AddRange(this.gameObject.GetComponentsInChildren<SpawnPoint>(true).Select(o => o.gameObject));

            if (this.gameObject.GetComponentsInChildren<SpawnPoint>().Length == 0)
            {
                this.tempSpawn = EditorMod.instance.AddSpawn(this.gameObject.GetComponent<Map>());
                this.tempSpawn.transform.position = Vector3.zero;
            }

            MapObjectInteraction.BeginInteraction(objects);

            EditorMod.instance.SetMapPhysicsActive(this.gameObject.GetComponent<Map>(), true);
            GameModeManager.SetGameMode("Sandbox");
            GameModeManager.CurrentHandler.StartGame();

            var gm = (GM_Test) GameModeManager.CurrentHandler.GameMode;
            gm.testMap = true;
            gm.gameObject.GetComponentInChildren<CurveAnimation>(true).enabled = false;

            var visualizers = this.gameObject.GetComponentsInChildren<IMapObjectVisualizer>();
            foreach (var viz in visualizers)
            {
                viz.SetEnabled(false);
            }
        }

        public void OnStopSimulation()
        {
            var gm = (GM_Test) GameModeManager.CurrentHandler.GameMode;
            gm.gameObject.GetComponentInChildren<CurveAnimation>(true).enabled = true;

            this.isSimulating = false;
            GameModeManager.SetGameMode(null);
            PlayerManager.instance.RemovePlayers();
            CardBarHandler.instance.ResetCardBards();

            EditorMod.instance.SetMapPhysicsActive(this.gameObject.GetComponent<Map>(), false);

            var visualizers = this.gameObject.GetComponentsInChildren<IMapObjectVisualizer>();
            foreach (var viz in visualizers)
            {
                viz.SetEnabled(true);
            }

            var interaction = MapObjectInteraction.EndInteraction();
            interaction.Undo();

            if (this.tempSpawn != null)
            {
                GameObject.Destroy(this.tempSpawn);
                this.tempSpawn = null;
            }
        }

        public void OnClickOpen()
        {
            FileDialog.OpenDialog(file =>
            {
                this.LoadMap(file);

                string personalFolder = Path.Combine(BepInEx.Paths.GameRootPath, "maps" + Path.DirectorySeparatorChar);
                string mapName = file.Substring(0, file.Length - 4).Replace(personalFolder, "");

                this.currentMapName = file.StartsWith(personalFolder) ? mapName : null;
            });
        }

        private void LoadMap(string file)
        {
            this.gui.transform.SetParent(null);
            this.grid.transform.SetParent(null);

            var map = this.gameObject.GetComponent<Map>();
            EditorMod.instance.LoadMap(map, file);

            this.gui.transform.SetParent(this.transform);
            this.grid.transform.SetParent(this.transform);
        }

        public void OnClickSaveAs()
        {
            FileDialog.SaveDialog(filename => this.SaveMap(filename));
        }

        public void OnClickSave()
        {
            if (this.currentMapName?.Length > 0)
            {
                this.SaveMap(this.currentMapName);
            }
            else
            {
                this.OnClickSaveAs();
            }
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
                this.AddSelected(list);
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
            this.timeline.BeginInteraction(this.selectedMapObjects);
        }

        public void OnDragEnd()
        {
            this.isDraggingMapObjects = false;
            this.timeline.EndInteraction();
        }

        public void OnClickMapObjects(List<GameObject> mapObjects)
        {
            mapObjects.Sort((a, b) => a.GetInstanceID() - b.GetInstanceID());
            GameObject mapObject = null;

            if (mapObjects.Count > 0)
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

            this.timeline.BeginInteraction(this.selectedMapObjects);
        }

        public void OnResizeEnd()
        {
            this.isResizingMapObject = false;
            this.timeline.EndInteraction();
        }

        public void OnRotateStart()
        {
            var mousePos = Input.mousePosition;
            var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));

            this.isRotatingMapObject = true;
            this.prevMouse = mouseWorldPos;

            this.timeline.BeginInteraction(this.selectedMapObjects);
        }

        public void OnRotateEnd()
        {
            this.isRotatingMapObject = false;
            this.timeline.EndInteraction();
        }

        public void OnNudgeSelectedMapObjects(Vector2 delta)
        {
            if (this.selectedMapObjects.Count > 0)
            {
                this.timeline.BeginInteraction(this.selectedMapObjects);

                foreach (var obj in this.selectedMapObjects)
                {
                    obj.transform.position += new Vector3(delta.x, delta.y, 0);
                }

                this.timeline.EndInteraction();
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
                    resized |= mapObject.GetComponent<IEditorActionHandler>().Resize(sizeDelta, this.resizeDirection);
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
            }
        }

        private void ClearSelected()
        {
            this.selectedMapObjects.Clear();
            this.gui.OnChangeSelectedObjects(this.selectedMapObjects);
        }

        private void AddSelected(IEnumerable<GameObject> list)
        {
            this.selectedMapObjects.AddRange(list);
            this.gui.OnChangeSelectedObjects(this.selectedMapObjects);
        }

        private void AddSelected(GameObject obj)
        {
            this.selectedMapObjects.Add(obj);
            this.gui.OnChangeSelectedObjects(this.selectedMapObjects);
        }

        private void ResetSpawnLabels()
        {
            var spawns = this.gameObject.GetComponentsInChildren<SpawnPoint>().ToList();
            for (int i = 0; i < spawns.Count; i++)
            {
                spawns[i].ID = i;
                spawns[i].TEAMID = i;
                spawns[i].gameObject.name = $"SPAWN POINT {i}";
            }
        }
    }
}

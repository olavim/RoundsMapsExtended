using System.IO;
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
        public float gridSize;
        public string currentMapName;
        public bool isSimulating;
        public bool snapToGrid;

        private bool isCreatingSelection;
        private bool isDraggingMapObjects;
        private bool isResizingMapObject;
        private bool isRotatingMapObject;
        private int resizeDirection;
        private Vector3 selectionStartPosition;
        private Rect selectionRect;
        private Vector3 prevMouse;
        private string temporaryFile;

        private MapEditorUI gui;

        public void Awake()
        {
            this.selectedMapObjects = new List<GameObject>();
            this.gridSize = 1.0f;
            this.snapToGrid = true;
            this.isCreatingSelection = false;
            this.isDraggingMapObjects = false;
            this.isResizingMapObject = false;
            this.isRotatingMapObject = false;
            this.currentMapName = null;
            this.temporaryFile = null;
            this.isSimulating = false;

            this.gameObject.AddComponent<MapEditorInputHandler>();

            var uiGo = new GameObject("MapEditorUI");
            uiGo.transform.SetParent(this.transform);

            this.gui = uiGo.AddComponent<MapEditorUI>();
            this.gui.editor = this;
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

            if (filename == null)
            {
                this.temporaryFile = path;
            }
            else
            {
                this.currentMapName = filename;
            }
        }

        public void SpawnMapObject(string mapObjectName)
        {
            var mapObject = EditorMod.instance.SpawnObject(this.gameObject.GetComponent<Map>(), mapObjectName);
            mapObject.transform.localScale = EditorUtils.SnapToGrid(mapObject.transform.localScale, this.gridSize);
            mapObject.transform.position = Vector3.zero;
        }

        public void OnToggleSnapToGrid(bool enabled)
        {
            this.snapToGrid = enabled;
        }

        public void OnDeleteSelectedMapObjects()
        {
            foreach (var obj in this.selectedMapObjects)
            {
                GameObject.Destroy(obj);
            }

            // Reset spawn IDs
            this.ExecuteAfterFrames(1, () =>
            {
                var spawns = this.gameObject.GetComponentsInChildren<SpawnPoint>().Reverse().ToList();
                for (int i = 0; i < spawns.Count; i++)
                {
                    spawns[i].ID = i;
                    spawns[i].TEAMID = i;
                    spawns[i].gameObject.name = $"SPAWN POINT {i}";
                    spawns[i].transform.SetAsFirstSibling();
                }
            });

            this.ClearSelected();
        }

        public void OnStartSimulation()
        {
            if (this.gameObject.GetComponentsInChildren<SpawnPoint>().Length == 0)
            {
                return;
            }

            this.ClearSelected();

            this.isSimulating = true;
            this.SaveMap(null);

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
            this.LoadMap(this.temporaryFile);
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

            var map = this.gameObject.GetComponent<Map>();
            EditorMod.instance.LoadMap(map, file);

            this.gui.transform.SetParent(this.transform);
        }

        public void OnClickSaveAs()
        {
            FileDialog.SaveDialog(this.SaveMap);
        }

        public void OnClickSave()
        {
            if (this.currentMapName?.Length > 0)
            {
                this.SaveMap(this.currentMapName);
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
        }

        public void OnDragEnd()
        {
            this.isDraggingMapObjects = false;
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
            this.ClearSelected();

            if (mapObject == null)
            {
                return;
            }

            bool changeMultiSelectionToSingle = this.IsMapObjectSelected(mapObject) && previouslySelectedCount > 1;
            bool selectUnselected = !this.IsMapObjectSelected(mapObject);

            if (changeMultiSelectionToSingle || selectUnselected)
            {
                this.AddSelected(mapObject);
            }
        }

        public void OnResizeStart(int resizeDirection)
        {
            var mousePos = Input.mousePosition;
            var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));

            this.isResizingMapObject = true;
            this.resizeDirection = resizeDirection;
            this.prevMouse = mouseWorldPos;
        }

        public void OnResizeEnd()
        {
            this.isResizingMapObject = false;
        }

        public void OnRotateStart()
        {
            var mousePos = Input.mousePosition;
            var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));

            this.isRotatingMapObject = true;
            this.prevMouse = mouseWorldPos;
        }

        public void OnRotateEnd()
        {
            this.isRotatingMapObject = false;
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
            var delta = mouseWorldPos - this.prevMouse;

            if (this.snapToGrid)
            {
                delta = EditorUtils.SnapToGrid(delta, this.gridSize);
            }

            foreach (var obj in this.selectedMapObjects)
            {
                obj.transform.position += delta;
            }

            this.prevMouse += delta;
        }

        private void ResizeMapObject()
        {
            var mousePos = Input.mousePosition;
            var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));
            var mouseDelta = mouseWorldPos - this.prevMouse;

            if (this.snapToGrid)
            {
                mouseDelta = EditorUtils.SnapToGrid(mouseDelta, this.gridSize);
            }

            if (mouseDelta != Vector3.zero)
            {
                bool resized = false;
                foreach (var mapObject in this.selectedMapObjects)
                {
                    resized |= mapObject.GetComponent<IEditorActionHandler>().Resize(mouseDelta, this.resizeDirection);
                }

                if (resized)
                {
                    this.prevMouse += mouseDelta;
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
    }
}

using System;
using System.IO;
using System.Collections.Generic;
using UnboundLib;
using UnityEngine;
using Photon.Pun;
using Sirenix.Serialization;

namespace MapEditor
{
    public class MapEditor : MonoBehaviour
    {
        public List<GameObject> selectedMapObjects;
        public float gridSize;

        public bool SnapToGrid { get; private set; }

        private bool isCreatingSelection;
        private bool isDraggingMapObjects;
        private bool isResizingMapObject;
        private bool isRotatingMapObject;
        private int resizeDirection;
        private Vector3 selectionStartPosition;
        private Rect selectionRect;
        private Vector3 prevMouse;

        private MapEditorUI gui;

        public void Awake()
        {
            this.selectedMapObjects = new List<GameObject>();
            this.gridSize = 2.0f;
            this.SnapToGrid = true;
            this.isCreatingSelection = false;
            this.isDraggingMapObjects = false;
            this.isResizingMapObject = false;
            this.isRotatingMapObject = false;

            this.gameObject.AddComponent<MapEditorInputHandler>();
            this.gui = this.gameObject.AddComponent<MapEditorUI>();
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                var mapData = new CustomMap();
                mapData.mapObjects = new List<MapObjectData>();
                mapData.spawns = new List<SpawnPointData>();

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
                File.WriteAllBytes("map.json", bytes);
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                this.SnapToGrid = false;
            }

            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                this.SnapToGrid = true;
            }

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
                this.selectedMapObjects.Clear();
                this.selectedMapObjects.AddRange(list);
                this.gui.OnChangeSelectedObjects(this.selectedMapObjects);
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

            if (mapObject == null)
            {
                this.selectedMapObjects.Clear();
                this.gui.OnChangeSelectedObjects(this.selectedMapObjects);
                return;
            }

            if (this.IsMapObjectSelected(mapObject))
            {
                if (this.selectedMapObjects.Count > 1)
                {
                    this.selectedMapObjects.Clear();
                    this.selectedMapObjects.Add(mapObject);
                    this.gui.OnChangeSelectedObjects(this.selectedMapObjects);
                }
                else
                {
                    this.selectedMapObjects.Clear();
                    this.gui.OnChangeSelectedObjects(this.selectedMapObjects);
                }
            }
            else
            {
                // GameObject is not part of a selection group, so we want to select only this object
                this.selectedMapObjects.Clear();
                this.selectedMapObjects.Add(mapObject);
                this.gui.OnChangeSelectedObjects(this.selectedMapObjects);
            }
        }

        public void OnResizeStart(GameObject mapObject, int resizeDirection)
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

        public void OnRotateStart(GameObject mapObject)
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

            if (this.SnapToGrid)
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

            if (this.SnapToGrid)
            {
                mouseDelta = EditorUtils.SnapToGrid(mouseDelta, this.gridSize);
            }

            if (mouseDelta != Vector3.zero)
            {
                bool resized = false;
                foreach (var mapObject in this.selectedMapObjects)
                {
                    resized |= mapObject.GetComponent<Transformers.IMapObjectTransformer>().Resize(mouseDelta, this.resizeDirection);
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

                if (this.SnapToGrid)
                {
                    angle = EditorUtils.Snap(angle, 15f);
                }

                mapObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            }
        }
    }
}

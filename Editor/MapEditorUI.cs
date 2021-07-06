using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace MapsExtended.Editor
{
    public static class TogglePosition
    {
        public static readonly Dictionary<int, Vector2> directionMultipliers = new Dictionary<int, Vector2>()
        {
            { TogglePosition.TopLeft, new Vector2(-1f, 1f) },
            { TogglePosition.TopMiddle, new Vector2(0, 1f) },
            { TogglePosition.TopRight, new Vector2(1f, 1f) },
            { TogglePosition.MiddleRight, new Vector2(1f, 0) },
            { TogglePosition.BottomRight, new Vector2(1f, -1f) },
            { TogglePosition.BottomMiddle, new Vector2(0, -1f) },
            { TogglePosition.BottomLeft, new Vector2(-1f, -1f) },
            { TogglePosition.MiddleLeft, new Vector2(-1f, 0) }
        };

        public const int TopLeft = 0;
        public const int TopMiddle = 1;
        public const int TopRight = 2;
        public const int MiddleRight = 3;
        public const int BottomRight = 4;
        public const int BottomMiddle = 5;
        public const int BottomLeft = 6;
        public const int MiddleLeft = 7;
    }

    public class MapEditorUI : MonoBehaviour
    {
        public MapEditor editor;

        private GameObject controlledMapObject;
        private int resizeDirection;
        private bool isResizing;
        private bool isRotating;

        private Vector2 scrollPos;
        private List<GameObject> selectedMapObjects;

        public void Awake()
        {
            this.scrollPos = Vector2.zero;
            this.selectedMapObjects = new List<GameObject>();
            this.controlledMapObject = null;
            this.isResizing = false;
            this.isRotating = false;

            var canvas = this.gameObject.AddComponent<Canvas>();
            var scaler = this.gameObject.AddComponent<CanvasScaler>();
            this.gameObject.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(0) && this.isResizing)
            {
                this.editor.OnResizeStart(this.controlledMapObject, this.resizeDirection);
            }

            if (Input.GetMouseButtonDown(0) && this.isRotating)
            {
                this.editor.OnRotateStart(this.controlledMapObject);
            }

            if (Input.GetMouseButtonUp(0) && this.isResizing)
            {
                this.isResizing = false;
                this.controlledMapObject = null;
                this.editor.OnResizeEnd();
            }

            if (Input.GetMouseButtonUp(0) && this.isRotating)
            {
                this.isRotating = false;
                this.controlledMapObject = null;
                this.editor.OnRotateEnd();
            }
        }

        public void OnChangeSelectedObjects(List<GameObject> list)
        {
            foreach (Transform child in this.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            this.selectedMapObjects.Clear();
            this.selectedMapObjects.AddRange(list);

            if (list.Count == 1)
            {
                var mapObject = list[0];

                this.AddResizeHandle(mapObject, TogglePosition.TopLeft);
                this.AddResizeHandle(mapObject, TogglePosition.TopRight);
                this.AddResizeHandle(mapObject, TogglePosition.BottomLeft);
                this.AddResizeHandle(mapObject, TogglePosition.BottomRight);
                this.AddResizeHandle(mapObject, TogglePosition.MiddleLeft);
                this.AddResizeHandle(mapObject, TogglePosition.MiddleRight);
                this.AddResizeHandle(mapObject, TogglePosition.BottomMiddle);
                this.AddResizeHandle(mapObject, TogglePosition.TopMiddle);

                this.AddRotationHandle(mapObject);
            }

            foreach (var obj in list)
            {
                var go = new GameObject("SelectionBox");

                var canvas = go.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = go.AddComponent<UI.UIScaler>();
                scaler.referenceGameObject = obj;

                var image = go.AddComponent<Image>();
                image.color = new Color32(255, 255, 255, 5);

                go.transform.SetParent(this.transform);
            }
        }

        private void AddResizeHandle(GameObject mapObject, int direction)
        {
            if (!mapObject.GetComponent<IEditorActionHandler>().CanResize(direction))
            {
                return;
            }

            var go = new GameObject("Toggle");

            go.AddComponent<GraphicRaycaster>();
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var aligner = go.AddComponent<UI.UIAligner>();
            aligner.referenceGameObject = mapObject;
            aligner.position = direction;

            var image = go.AddComponent<Image>();
            image.rectTransform.sizeDelta = new Vector2(10f, 10f);

            var events = go.AddComponent<EditorPointerEvents>();

            events.pointerDown += hoveredObj =>
            {
                if (!this.isRotating && !this.isResizing)
                {
                    this.isResizing = true;
                    this.resizeDirection = direction;
                    this.controlledMapObject = mapObject;
                }
            };

            go.transform.SetParent(this.transform);
        }

        private void AddRotationHandle(GameObject mapObject)
        {
            if (!mapObject.GetComponent<IEditorActionHandler>().CanRotate())
            {
                return;
            }

            var go = new GameObject("RotationHandle");

            go.AddComponent<GraphicRaycaster>();
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var aligner = go.AddComponent<UI.UIAligner>();
            aligner.referenceGameObject = mapObject;
            aligner.position = TogglePosition.TopMiddle;
            aligner.padding = 32f;

            var image = go.AddComponent<Image>();
            image.rectTransform.sizeDelta = new Vector2(10f, 10f);

            var events = go.AddComponent<EditorPointerEvents>();

            events.pointerDown += hoveredObj =>
            {
                if (!this.isRotating && !this.isResizing)
                {
                    this.isRotating = true;
                    this.controlledMapObject = mapObject;
                }
            };

            go.transform.SetParent(this.transform);
        }

        public void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 200, 400));
            GUILayout.BeginVertical();

            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos, GUILayout.Width(200), GUILayout.Height(200));
            GUILayout.BeginVertical();

            foreach (string objectName in MapObjectManager.instance.GetMapObjects())
            {
                if (GUILayout.Button(objectName))
                {
                    EditorMod.instance.SpawnObject(this.editor.gameObject.GetComponent<Map>(), objectName);
                }
            }

            GUILayout.Label("Grid size: " + this.editor.gridSize);
            this.editor.gridSize = EditorUtils.Snap(GUILayout.HorizontalSlider(this.editor.gridSize, 0.5f, 4f), 0.5f);

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.BeginVertical();

            if (GUILayout.Button("Spawn Point"))
            {
                var spawn = MapsExtended.AddSpawn(this.editor.gameObject.GetComponent<Map>());
                spawn.AddComponent<Visualizers.SpawnVisualizer>();
                spawn.AddComponent<SpawnActionHandler>();
            }

            GUILayout.EndVertical();

            GUILayout.EndVertical();
            GUILayout.EndArea();

            var selectionStyle = new GUIStyle(GUI.skin.box);
            selectionStyle.normal.background = UIUtils.GetTexture(2, 2, new Color32(255, 255, 255, 20));

            var selectionRect = this.editor.GetSelection();

            if (selectionRect.width > 11 && selectionRect.height > 11)
            {
                GUI.Box(selectionRect, GUIContent.none, selectionStyle);
            }
        }
    }
}

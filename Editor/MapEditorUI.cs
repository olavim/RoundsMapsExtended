using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using MapsExtended.UI;

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

        private int resizeDirection;
        private bool isResizing;
        private bool isRotating;

        private List<GameObject> selectedMapObjects;

        public void Awake()
        {
            this.selectedMapObjects = new List<GameObject>();
            this.isResizing = false;
            this.isRotating = false;

            var canvas = this.gameObject.AddComponent<Canvas>();
            var scaler = this.gameObject.AddComponent<CanvasScaler>();
            this.gameObject.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            var toolbarGo = GameObject.Instantiate(Assets.ToolbarPrefab, this.transform);
            toolbarGo.name = "Toolbar";

            var toolbar = toolbarGo.GetComponent<Toolbar>();

            var ctrlKey = new NamedKeyCode(KeyCode.LeftControl, "Ctrl");
            var shiftKey = new NamedKeyCode(KeyCode.LeftShift, "Shift");
            var sKey = new NamedKeyCode(KeyCode.S, "S");
            var oKey = new NamedKeyCode(KeyCode.O, "O");
            var cKey = new NamedKeyCode(KeyCode.C, "C");
            var vKey = new NamedKeyCode(KeyCode.V, "V");
            var zKey = new NamedKeyCode(KeyCode.Z, "Z");

            var openItem = new MenuItem("Open...", this.editor.OnClickOpen, oKey, ctrlKey);
            var saveItem = new MenuItem("Save", this.editor.OnClickSave, sKey, ctrlKey);
            var saveAsItem = new MenuItem("Save As...", this.editor.OnClickSaveAs, sKey, ctrlKey, shiftKey);

            toolbar.fileMenu.AddMenuItem(openItem);
            toolbar.fileMenu.AddMenuItem(saveItem);
            toolbar.fileMenu.AddMenuItem(saveAsItem);

            var undoItem = new MenuItem("Undo", this.editor.OnUndo, zKey, ctrlKey);
            var redoItem = new MenuItem("Redo", this.editor.OnRedo, zKey, ctrlKey, shiftKey);
            var copyItem = new MenuItem("Copy", this.editor.OnCopy, cKey, ctrlKey);
            var pasteItem = new MenuItem("Paste", this.editor.OnPaste, vKey, ctrlKey);

            toolbar.editMenu.AddMenuItem(undoItem);
            toolbar.editMenu.AddMenuItem(redoItem);
            toolbar.editMenu.AddMenuItem(copyItem);
            toolbar.editMenu.AddMenuItem(pasteItem);

            var objGroundItem = new MenuItem("Ground", () => this.editor.SpawnMapObject("Ground"));
            var objSawItem = new MenuItem("Saw", () => this.editor.SpawnMapObject("Saw"));

            var objBoxItem = new MenuItem("Box", () => this.editor.SpawnMapObject("Box"));
            var objBoxDestructibleItem = new MenuItem("Box (Destructible)", () => this.editor.SpawnMapObject("Destructible Box"));
            var objBoxBgItem = new MenuItem("Box (Background)", () => this.editor.SpawnMapObject("Background Box"));

            var staticGroupItem = new MenuItem("Static", new MenuItem[] { objGroundItem, objSawItem });
            var dynamicGroupItem = new MenuItem("Dynamic", new MenuItem[] { objBoxItem, objBoxDestructibleItem, objBoxBgItem });
            var spawnItem = new MenuItem("Spawn Point", this.editor.AddSpawn);

            toolbar.mapObjectMenu.AddMenuItem(staticGroupItem);
            toolbar.mapObjectMenu.AddMenuItem(dynamicGroupItem);
            toolbar.mapObjectMenu.AddMenuItem(spawnItem);

            toolbar.gridSizeSlider.value = this.editor.GridSize;
            toolbar.gridSizeSlider.onValueChanged.AddListener(val => this.editor.GridSize = val);

            toolbar.onToggleSimulation += simulated =>
            {
                if (simulated)
                {
                    this.editor.OnStartSimulation();
                }
                else
                {
                    this.editor.OnStopSimulation();
                }

                toolbar.fileMenu.gameObject.SetActive(!simulated);
                toolbar.editMenu.gameObject.SetActive(!simulated);
                toolbar.mapObjectMenu.gameObject.SetActive(!simulated);
                toolbar.gridSizeSlider.transform.parent.parent.gameObject.SetActive(!simulated);
            };
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(0) && this.isResizing)
            {
                this.editor.OnResizeStart(this.resizeDirection);
            }

            if (Input.GetMouseButtonDown(0) && this.isRotating)
            {
                this.editor.OnRotateStart();
            }

            if (Input.GetMouseButtonUp(0) && this.isResizing)
            {
                this.isResizing = false;
                this.editor.OnResizeEnd();
            }

            if (Input.GetMouseButtonUp(0) && this.isRotating)
            {
                this.isRotating = false;
                this.editor.OnRotateEnd();
            }
        }

        public void OnChangeSelectedObjects(List<GameObject> list)
        {
            foreach (Transform child in this.transform)
            {
                if (child.name == "Toolbar")
                {
                    continue;
                }

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

            var button = go.AddComponent<Button>();
            button.colors = new ColorBlock()
            {
                colorMultiplier = 1,
                fadeDuration = 0.1f,
                normalColor = new Color(1, 1, 1),
                highlightedColor = new Color(0.8f, 0.8f, 0.8f),
                pressedColor = new Color(0.6f, 0.6f, 0.6f)
            };

            var events = go.AddComponent<EditorPointerEvents>();

            events.pointerDown += hoveredObj =>
            {
                if (!this.isRotating && !this.isResizing)
                {
                    this.isResizing = true;
                    this.resizeDirection = direction;
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
            aligner.padding = 48f;

            var image = go.AddComponent<ProceduralImage>();
            image.rectTransform.sizeDelta = new Vector2(12f, 12f);

            var modifier = go.AddComponent<UniformModifier>();
            modifier.Radius = 6;

            var button = go.AddComponent<Button>();
            button.colors = new ColorBlock()
            {
                colorMultiplier = 1,
                fadeDuration = 0.1f,
                normalColor = new Color(1, 1, 1),
                highlightedColor = new Color(0.8f, 0.8f, 0.8f),
                pressedColor = new Color(0.6f, 0.6f, 0.6f)
            };

            var events = go.AddComponent<EditorPointerEvents>();

            events.pointerDown += hoveredObj =>
            {
                if (!this.isRotating && !this.isResizing)
                {
                    this.isRotating = true;
                }
            };

            go.transform.SetParent(this.transform);
        }

        public void OnGUI()
        {
            if (this.editor.isSimulating)
            {
                GUI.enabled = false;
            }

            var selectionStyle = new GUIStyle(GUI.skin.box);
            selectionStyle.normal.background = UIUtils.GetTexture(2, 2, new Color32(255, 255, 255, 20));

            var selectionRect = this.editor.GetSelection();

            if (selectionRect.width > 11 && selectionRect.height > 11)
            {
                GUI.Box(selectionRect, GUIContent.none, selectionStyle);
            }

            GUI.enabled = true;
        }
    }
}

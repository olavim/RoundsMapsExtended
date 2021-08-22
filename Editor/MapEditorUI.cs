using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using MapsExt.UI;
using MapsExt.MapObjects;
using System;
using System.Linq;
using HarmonyLib;

namespace MapsExt.Editor
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
		private Toolbar toolbar;

		private List<GameObject> selectedMapObjects;
		private Window mapObjectWindow;
		private bool mapObjectWindowWasOpen;

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

			this.toolbar = toolbarGo.GetComponent<Toolbar>();

			var ctrlKey = new NamedKeyCode(KeyCode.LeftControl, "Ctrl");
			var shiftKey = new NamedKeyCode(KeyCode.LeftShift, "Shift");
			var sKey = new NamedKeyCode(KeyCode.S, "S");
			var oKey = new NamedKeyCode(KeyCode.O, "O");
			var cKey = new NamedKeyCode(KeyCode.C, "C");
			var vKey = new NamedKeyCode(KeyCode.V, "V");
			var zKey = new NamedKeyCode(KeyCode.Z, "Z");

			var openItem = new MenuItemBuilder().Label("Open...").Action(this.editor.OnClickOpen).KeyBinding(oKey, ctrlKey).Item();
			var saveItem = new MenuItemBuilder().Label("Save").Action(this.editor.OnClickSave).KeyBinding(sKey, ctrlKey).Item();
			var saveAsItem = new MenuItemBuilder().Label("Save As...").Action(this.editor.OnClickSaveAs).KeyBinding(sKey, ctrlKey, shiftKey).Item();
			var openMapFolderItem = new MenuItemBuilder().Label("Open Map Folder").Action(this.OpenMapFolder).Item();

			this.toolbar.fileMenu.AddItem(openItem);
			this.toolbar.fileMenu.AddItem(saveItem);
			this.toolbar.fileMenu.AddItem(saveAsItem);
			this.toolbar.fileMenu.AddItem(openMapFolderItem);

			var undoItem = new MenuItemBuilder().Label("Undo").Action(this.editor.OnUndo).KeyBinding(zKey, ctrlKey).Item();
			var redoItem = new MenuItemBuilder().Label("Redo").Action(this.editor.OnRedo).KeyBinding(zKey, ctrlKey, shiftKey).Item();
			var copyItem = new MenuItemBuilder().Label("Copy").Action(this.editor.OnCopy).KeyBinding(cKey, ctrlKey).Item();
			var pasteItem = new MenuItemBuilder().Label("Paste").Action(this.editor.OnPaste).KeyBinding(vKey, ctrlKey).Item();

			this.toolbar.editMenu.AddItem(undoItem);
			this.toolbar.editMenu.AddItem(redoItem);
			this.toolbar.editMenu.AddItem(copyItem);
			this.toolbar.editMenu.AddItem(pasteItem);

			var mapObjects = new Dictionary<string, List<Tuple<string, Type>>>();
			mapObjects.Add("", new List<Tuple<string, Type>>());

			foreach (var attr in MapsExtendedEditor.instance.mapObjectAttributes)
			{
				string category = attr.category ?? "";
				
				if (!mapObjects.ContainsKey(category))
				{
					mapObjects.Add(category, new List<Tuple<string, Type>>());
				}

				mapObjects[category].Add(new Tuple<string, Type>(attr.label, attr.dataType));
			}

			foreach (var category in mapObjects.Keys.Where(k => k != ""))
			{
				var builder = new MenuItemBuilder().Label(category);

				foreach (var entry in mapObjects[category])
				{
					Action action = () => this.editor.SpawnMapObject(entry.Item2);
					builder.SubItem(b => b.Label(entry.Item1).Action(action));
				}

				this.toolbar.mapObjectMenu.AddItem(builder.Item());
			}

			foreach (var entry in mapObjects[""])
			{
				Action action = () => this.editor.SpawnMapObject(entry.Item2);
				var builder = new MenuItemBuilder().Label(entry.Item1).Action(action);
				this.toolbar.mapObjectMenu.AddItem(builder.Item());
			}

			var mapObjectsWindowItem = new MenuItemBuilder().Label("Map Objects").Action(this.OpenMapObjectWindow).Item();

			this.toolbar.windowMenu.AddItem(mapObjectsWindowItem);

			this.toolbar.gridSizeSlider.value = this.editor.GridSize;
			this.toolbar.gridSizeSlider.onValueChanged.AddListener(val => this.editor.GridSize = val);

			this.toolbar.onToggleSimulation += simulated =>
			{
				if (simulated)
				{
					this.editor.OnStartSimulation();
					this.mapObjectWindowWasOpen = this.mapObjectWindow.gameObject.activeSelf;
					this.mapObjectWindow.gameObject.SetActive(false);
				}
				else
				{
					this.editor.OnStopSimulation();

					if (mapObjectWindowWasOpen)
					{
						this.mapObjectWindow.gameObject.SetActive(true);
					}
				}

				var menuState = simulated ? Menu.MenuState.DISABLED : Menu.MenuState.INACTIVE;
				this.toolbar.fileMenu.SetState(menuState);
				this.toolbar.editMenu.SetState(menuState);
				this.toolbar.mapObjectMenu.SetState(menuState);
				this.toolbar.windowMenu.SetState(menuState);
				this.toolbar.gridSizeSlider.transform.parent.parent.gameObject.SetActive(!simulated);
			};

			this.mapObjectWindow = GameObject.Instantiate(Assets.WindowPrefab, this.transform).GetComponent<Window>();
			this.mapObjectWindow.title.text = "Map Objects";

			var windowSize = this.mapObjectWindow.gameObject.GetComponent<RectTransform>().sizeDelta;
			this.mapObjectWindow.transform.position = new Vector3(Screen.width - (windowSize.x / 2f) - 5, Screen.height - (windowSize.y / 2f) - 35, 0);

			GameObject CreateButton(MenuItem item)
			{
				var go = new GameObject("Button");

				var image = go.AddComponent<Image>();
				image.color = Color.white;

				var button = go.AddComponent<Button>();
				button.onClick.AddListener(() => item.action?.Invoke());
				button.targetGraphic = image;
				button.colors = new ColorBlock
				{
					normalColor = new Color32(40, 40, 40, 255),
					highlightedColor = new Color32(50, 50, 50, 255),
					pressedColor = new Color32(60, 60, 60, 255),
					fadeDuration = 0.1f,
					colorMultiplier = 1
				};
				button.navigation = new Navigation { mode = Navigation.Mode.None };

				var layout = go.AddComponent<LayoutElement>();
				layout.preferredHeight = 20;

				var layoutGroup = go.AddComponent<HorizontalLayoutGroup>();
				layoutGroup.padding = new RectOffset(6, 0, 0, 0);
				layoutGroup.childAlignment = TextAnchor.MiddleLeft;
				layoutGroup.childControlWidth = true;
				layoutGroup.childControlHeight = true;
				layoutGroup.childForceExpandWidth = true;
				layoutGroup.childForceExpandHeight = false;

				var textGo = new GameObject("Text");
				textGo.transform.SetParent(go.transform);

				var text = textGo.AddComponent<Text>();
				text.fontSize = 12;
				text.font = Font.CreateDynamicFontFromOSFont("Arial", 12);
				text.color = new Color32(200, 200, 200, 255);
				text.text = item.label;

				return go;
			}

			foreach (var item in this.toolbar.mapObjectMenu.items)
			{
				if (item.items == null)
				{
					var button = CreateButton(item);
					button.transform.SetParent(this.mapObjectWindow.content.transform);
				}
				else
				{
					var foldout = GameObject.Instantiate(Assets.FoldoutPrefab, this.mapObjectWindow.content.transform).GetComponent<Foldout>();
					foldout.label.text = item.label;

					foreach (var subitem in item.items)
					{
						var button = CreateButton(subitem);
						button.transform.SetParent(foldout.content.transform);
					}
				}
			}
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

			this.toolbar.editMenu.SetItemEnabled("Undo", this.editor.timeline.CanUndo());
			this.toolbar.editMenu.SetItemEnabled("Redo", this.editor.timeline.CanRedo());
		}

		private void OpenMapFolder()
		{
			Application.OpenURL($"file://{BepInEx.Paths.GameRootPath}/maps");
		}

		private void OpenMapObjectWindow()
		{
			this.mapObjectWindow.gameObject.SetActive(true);
		}

		public void OnChangeSelectedObjects(List<GameObject> list)
		{
			foreach (Transform child in this.transform)
			{
				if (child == this.toolbar.transform || child == this.mapObjectWindow.transform)
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
			if (!mapObject.GetComponent<EditorActionHandler>().CanResize(direction))
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
			if (!mapObject.GetComponent<EditorActionHandler>().CanRotate())
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
			if (!this.editor.isSimulating)
			{
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
}

using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using UnityEngine.Events;
using MapsExt.Editor.UI;
using MapsExt.MapObjects;
using MapsExt.Editor.ActionHandlers;
using MapsExt.Editor.Commands;
using System;
using System.Linq;

namespace MapsExt.Editor
{
	public static class AnchorPosition
	{
		public static readonly Dictionary<int, Vector2> directionMultipliers = new Dictionary<int, Vector2>()
		{
			{ AnchorPosition.Middle, new Vector2(0, 0) },
			{ AnchorPosition.TopLeft, new Vector2(-1f, 1f) },
			{ AnchorPosition.TopMiddle, new Vector2(0, 1f) },
			{ AnchorPosition.TopRight, new Vector2(1f, 1f) },
			{ AnchorPosition.MiddleRight, new Vector2(1f, 0) },
			{ AnchorPosition.BottomRight, new Vector2(1f, -1f) },
			{ AnchorPosition.BottomMiddle, new Vector2(0, -1f) },
			{ AnchorPosition.BottomLeft, new Vector2(-1f, -1f) },
			{ AnchorPosition.MiddleLeft, new Vector2(-1f, 0) }
		};

		public static readonly Dictionary<int, Vector2> sizeMultipliers = new Dictionary<int, Vector2>()
		{
			{ AnchorPosition.Middle, new Vector2(1f, 1f) },
			{ AnchorPosition.TopLeft, new Vector2(-1f, 1f) },
			{ AnchorPosition.TopMiddle, new Vector2(0, 1f) },
			{ AnchorPosition.TopRight, new Vector2(1f, 1f) },
			{ AnchorPosition.MiddleRight, new Vector2(1f, 0) },
			{ AnchorPosition.BottomRight, new Vector2(1f, -1f) },
			{ AnchorPosition.BottomMiddle, new Vector2(0, -1f) },
			{ AnchorPosition.BottomLeft, new Vector2(-1f, -1f) },
			{ AnchorPosition.MiddleLeft, new Vector2(-1f, 0) }
		};

		public const int Middle = 0;
		public const int TopLeft = 1;
		public const int TopMiddle = 2;
		public const int TopRight = 3;
		public const int MiddleRight = 4;
		public const int BottomRight = 5;
		public const int BottomMiddle = 6;
		public const int BottomLeft = 7;
		public const int MiddleLeft = 8;
	}

	public class MapEditorUI : MonoBehaviour
	{
		public MapEditor editor;
		public Toolbar toolbar;
		public Window mapObjectWindow;
		public Window inspectorWindow;
		public AnimationWindow animationWindow;
		public MapObjectInspector inspector;

		private Texture2D selectionTexture;

		private int resizeDirection;
		private bool isResizing;
		private bool isRotating;
		private List<EditorActionHandler> selectedActionHandlers;
		private Window[] windows;
		private bool[] windowWasOpen;

		public void Awake()
		{
			this.selectedActionHandlers = new List<EditorActionHandler>();
			this.isResizing = false;
			this.isRotating = false;

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
					UnityAction action = () => this.editor.commandHistory.Add(new CreateCommand(entry.Item2));
					builder.SubItem(b => b.Label(entry.Item1).Action(action));
				}

				this.toolbar.mapObjectMenu.AddItem(builder.Item());
			}

			foreach (var entry in mapObjects[""])
			{
				UnityAction action = () => this.editor.commandHistory.Add(new CreateCommand(entry.Item2));
				var builder = new MenuItemBuilder().Label(entry.Item1).Action(action);
				this.toolbar.mapObjectMenu.AddItem(builder.Item());
			}

			this.toolbar.gridSizeSlider.value = this.editor.GridSize;
			this.toolbar.gridSizeSlider.onValueChanged.AddListener(val => this.editor.GridSize = val);

			this.toolbar.onToggleSimulation += simulated =>
			{
				if (simulated)
				{
					this.editor.OnStartSimulation();
					for (int i = 0; i < this.windows.Length; i++)
					{
						this.windowWasOpen[i] = this.windows[i].gameObject.activeSelf;
						this.windows[i].gameObject.SetActive(false);
					}
				}
				else
				{
					this.editor.OnStopSimulation();
					for (int i = 0; i < this.windows.Length; i++)
					{
						this.windows[i].gameObject.SetActive(this.windowWasOpen[i]);
					}
				}

				var menuState = simulated ? Menu.MenuState.DISABLED : Menu.MenuState.INACTIVE;
				this.toolbar.fileMenu.SetState(menuState);
				this.toolbar.editMenu.SetState(menuState);
				this.toolbar.mapObjectMenu.SetState(menuState);
				this.toolbar.windowMenu.SetState(menuState);
				this.toolbar.gridSizeSlider.transform.parent.parent.gameObject.SetActive(!simulated);
			};

			var mapObjectWindowSize = this.mapObjectWindow.gameObject.GetComponent<RectTransform>().sizeDelta;
			this.mapObjectWindow.transform.position = new Vector3(Screen.width - (mapObjectWindowSize.x / 2f) - 5, Screen.height - (mapObjectWindowSize.y / 2f) - 35, 0);

			var inspectorWindowSize = this.inspectorWindow.gameObject.GetComponent<RectTransform>().sizeDelta;
			this.inspectorWindow.transform.position = new Vector3(Screen.width - (inspectorWindowSize.x / 2f) - 5, this.mapObjectWindow.transform.position.y - inspectorWindowSize.y - 5, 0);

			var animationWindowSize = this.animationWindow.gameObject.GetComponent<RectTransform>().sizeDelta;
			this.animationWindow.transform.position = new Vector3((animationWindowSize.x / 2f) + 5, Screen.height - (animationWindowSize.y / 2f) - 35, 0);

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
					disabledColor = new Color32(40, 40, 40, 255),
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

		public void Start()
		{
			this.editor.selectedActionHandlers.CollectionChanged += this.HandleSelectedObjectsChanged;
			this.windows = new Window[] { this.mapObjectWindow, this.inspectorWindow, this.animationWindow };
			this.windowWasOpen = new bool[this.windows.Length];
			this.selectionTexture = UIUtils.GetTexture(2, 2, new Color32(255, 255, 255, 20));
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

			this.toolbar.editMenu.SetItemEnabled("Undo", this.editor.commandHistory.CanUndo());
			this.toolbar.editMenu.SetItemEnabled("Redo", this.editor.commandHistory.CanRedo());
			this.toolbar.editMenu.SetItemEnabled("Copy", this.editor.animationHandler.animation == null);
			this.toolbar.editMenu.SetItemEnabled("Paste", this.editor.animationHandler.animation == null);

			if (this.toolbar.mapObjectMenu.state == Menu.MenuState.DISABLED && this.editor.animationHandler.animation == null)
			{
				this.toolbar.mapObjectMenu.SetState(Menu.MenuState.INACTIVE);
			}

			if (this.toolbar.mapObjectMenu.state != Menu.MenuState.DISABLED && this.editor.animationHandler.animation != null)
			{
				this.toolbar.mapObjectMenu.SetState(Menu.MenuState.DISABLED);
			}

			if (this.animationWindow.gameObject.activeSelf && !this.editor.animationHandler.animation)
			{
				this.animationWindow.Close();
			}

			if (this.inspector?.visualTarget)
			{
				this.inspector.positionInput.SetWithoutEvent((Vector2) this.inspector.visualTarget.transform.position);
				this.inspector.sizeInput.SetWithoutEvent((Vector2) this.inspector.visualTarget.transform.localScale);
				this.inspector.rotationInput.SetWithoutEvent(this.inspector.visualTarget.transform.rotation.eulerAngles.z);
			}

			if (this.inspector.gameObject.activeSelf && !this.inspector.interactionTarget)
			{
				this.inspector.Unlink();
			}

			bool animWindowOpen = this.animationWindow.gameObject.activeSelf;
			byte alpha = (byte) (animWindowOpen ? 100 : 255);
			foreach (var btn in this.mapObjectWindow.content.GetComponentsInChildren<Button>())
			{
				btn.interactable = !animWindowOpen;
				btn.gameObject.GetComponentInChildren<Text>().color = new Color32(200, 200, 200, alpha);
			}
		}

		public void OnClickOpen()
		{
			FileDialog.OpenDialog(file =>
			{
				this.animationWindow.Close();
				this.editor.LoadMap(file);
			});
		}

		public void OnClickSaveAs()
		{
			FileDialog.SaveDialog(filename => this.editor.SaveMap(filename));
		}

		public void OnClickSave()
		{
			if (this.editor.currentMapName?.Length > 0)
			{
				this.editor.SaveMap(this.editor.currentMapName);
			}
			else
			{
				this.OnClickSaveAs();
			}
		}

		public void OnClickOpenMapFolder()
		{
			Application.OpenURL($"file://{BepInEx.Paths.GameRootPath}/maps");
		}

		public void OnClickUndo()
		{
			this.editor.commandHistory.Undo();
			this.animationWindow.Refresh();
		}

		public void OnClickRedo()
		{
			this.editor.commandHistory.Execute();
			this.animationWindow.Refresh();
		}

		public void OnClickCopy()
		{
			this.editor.OnCopy();
		}

		public void OnClickPaste()
		{
			this.editor.OnPaste();
		}

		public void OpenMapObjectWindow()
		{
			this.mapObjectWindow.gameObject.SetActive(true);
		}

		public void OpenInspectorWindow()
		{
			this.inspectorWindow.gameObject.SetActive(true);
		}

		public void HandleSelectedObjectsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var list = this.editor.selectedActionHandlers;

			foreach (Transform child in this.transform)
			{
				if (child == this.toolbar.transform || this.windows.Any(w => w.transform == child))
				{
					continue;
				}

				GameObject.Destroy(child.gameObject);
			}

			this.inspector.Unlink();

			this.selectedActionHandlers.Clear();
			this.selectedActionHandlers.AddRange(list);

			if (list.Count == 1)
			{
				var handlerGameObject = list[0].gameObject;

				this.AddResizeHandle(handlerGameObject, AnchorPosition.TopLeft);
				this.AddResizeHandle(handlerGameObject, AnchorPosition.TopRight);
				this.AddResizeHandle(handlerGameObject, AnchorPosition.BottomLeft);
				this.AddResizeHandle(handlerGameObject, AnchorPosition.BottomRight);
				this.AddResizeHandle(handlerGameObject, AnchorPosition.MiddleLeft);
				this.AddResizeHandle(handlerGameObject, AnchorPosition.MiddleRight);
				this.AddResizeHandle(handlerGameObject, AnchorPosition.BottomMiddle);
				this.AddResizeHandle(handlerGameObject, AnchorPosition.TopMiddle);
				this.AddRotationHandle(handlerGameObject);

				this.inspector.Link(handlerGameObject);
			}

			if (list.Count == 0 && this.editor.animationHandler.animation && this.editor.animationHandler.enabled)
			{
				this.inspector.Link(this.editor.animationHandler.animation.gameObject, this.editor.animationHandler.keyframeMapObject);
			}

			bool canAnimate = list.Count == 1 && list[0].GetComponent<SpatialMapObjectInstance>();
			this.toolbar.editMenu.SetItemEnabled("Animation...", canAnimate);

			foreach (var handler in list)
			{
				var go = new GameObject("SelectionBox");

				var canvas = go.AddComponent<Canvas>();
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;

				var scaler = go.AddComponent<UI.UIScaler>();
				scaler.referenceGameObject = handler.gameObject;

				var image = go.AddComponent<Image>();
				image.color = new Color32(255, 255, 255, 5);

				go.transform.SetParent(this.transform);
			}
		}

		private void AddResizeHandle(GameObject mapObject, int direction)
		{
			if (!mapObject.GetComponent<EditorActionHandler>().CanResize())
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
			aligner.position = AnchorPosition.TopMiddle;
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
				selectionStyle.normal.background = this.selectionTexture;
				var selectionRect = this.editor.GetSelection();

				if (selectionRect.width > 11 && selectionRect.height > 11)
				{
					GUI.Box(selectionRect, GUIContent.none, selectionStyle);
				}
			}
		}
	}
}

using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using MapsExt.UI;
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

		private int resizeDirection;
		private bool isResizing;
		private bool isRotating;
		private Toolbar toolbar;

		private List<EditorActionHandler> selectedActionHandlers;
		private Window mapObjectWindow;
		private Window inspectorWindow;
		private MapObjectInspector inspector;
		private AnimationWindow animationWindow;
		private bool mapObjectWindowWasOpen;
		private bool animationWindowWasOpen;
		private bool inspectorWindowWasOpen;

		public void Awake()
		{
			this.selectedActionHandlers = new List<EditorActionHandler>();
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
			var toolbarCanvas = toolbarGo.AddComponent<Canvas>();
			toolbarCanvas.overrideSorting = true;
			toolbarCanvas.sortingOrder = 10;

			toolbarGo.AddComponent<GraphicRaycaster>();

			this.toolbar = toolbarGo.GetComponent<Toolbar>();

			var ctrlKey = new NamedKeyCode(KeyCode.LeftControl, "Ctrl");
			var shiftKey = new NamedKeyCode(KeyCode.LeftShift, "Shift");
			var sKey = new NamedKeyCode(KeyCode.S, "S");
			var oKey = new NamedKeyCode(KeyCode.O, "O");
			var cKey = new NamedKeyCode(KeyCode.C, "C");
			var vKey = new NamedKeyCode(KeyCode.V, "V");
			var zKey = new NamedKeyCode(KeyCode.Z, "Z");
			var tKey = new NamedKeyCode(KeyCode.T, "T");

			var openItem = new MenuItemBuilder().Label("Open...").Action(this.OnClickOpen).KeyBinding(oKey, ctrlKey).Item();
			var saveItem = new MenuItemBuilder().Label("Save").Action(this.OnClickSave).KeyBinding(sKey, ctrlKey).Item();
			var saveAsItem = new MenuItemBuilder().Label("Save As...").Action(this.OnClickSaveAs).KeyBinding(sKey, ctrlKey, shiftKey).Item();
			var openMapFolderItem = new MenuItemBuilder().Label("Open Map Folder").Action(this.OpenMapFolder).Item();

			this.toolbar.fileMenu.AddItem(openItem);
			this.toolbar.fileMenu.AddItem(saveItem);
			this.toolbar.fileMenu.AddItem(saveAsItem);
			this.toolbar.fileMenu.AddItem(openMapFolderItem);

			var undoItem = new MenuItemBuilder().Label("Undo").Action(this.OnClickUndo).KeyBinding(zKey, ctrlKey).Item();
			var redoItem = new MenuItemBuilder().Label("Redo").Action(this.OnClickRedo).KeyBinding(zKey, ctrlKey, shiftKey).Item();
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
					Action action = () => this.editor.commandHistory.Add(new CreateCommand(entry.Item2));
					builder.SubItem(b => b.Label(entry.Item1).Action(action));
				}

				this.toolbar.mapObjectMenu.AddItem(builder.Item());
			}

			foreach (var entry in mapObjects[""])
			{
				Action action = () => this.editor.commandHistory.Add(new CreateCommand(entry.Item2));
				var builder = new MenuItemBuilder().Label(entry.Item1).Action(action);
				this.toolbar.mapObjectMenu.AddItem(builder.Item());
			}

			var mapObjectsWindowItem = new MenuItemBuilder().Label("Map Objects").Action(this.OpenMapObjectWindow).Item();
			var inspectorWindowItem = new MenuItemBuilder().Label("Map Object Inspector").Action(this.OpenInspectorWindow).Item();

			this.toolbar.windowMenu.AddItem(mapObjectsWindowItem);
			this.toolbar.windowMenu.AddItem(inspectorWindowItem);

			this.toolbar.gridSizeSlider.value = this.editor.GridSize;
			this.toolbar.gridSizeSlider.onValueChanged.AddListener(val => this.editor.GridSize = val);

			this.toolbar.onToggleSimulation += simulated =>
			{
				if (simulated)
				{
					this.editor.OnStartSimulation();
					this.mapObjectWindowWasOpen = this.mapObjectWindow.gameObject.activeSelf;
					this.inspectorWindowWasOpen = this.inspectorWindow.gameObject.activeSelf;
					this.animationWindowWasOpen = this.animationWindow.gameObject.activeSelf;
					this.mapObjectWindow.gameObject.SetActive(false);
					this.inspectorWindow.gameObject.SetActive(false);
					this.animationWindow.gameObject.SetActive(false);
				}
				else
				{
					this.editor.OnStopSimulation();
					this.mapObjectWindow.gameObject.SetActive(mapObjectWindowWasOpen);
					this.inspectorWindow.gameObject.SetActive(inspectorWindowWasOpen);
					this.animationWindow.gameObject.SetActive(animationWindowWasOpen);
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
			var mapObjectWindowSize = new Vector2(300, 300);
			this.mapObjectWindow.gameObject.GetComponent<RectTransform>().sizeDelta = mapObjectWindowSize;
			this.mapObjectWindow.transform.position = new Vector3(Screen.width - (mapObjectWindowSize.x / 2f) - 5, Screen.height - (mapObjectWindowSize.y / 2f) - 35, 0);

			this.inspectorWindow = GameObject.Instantiate(Assets.WindowPrefab, this.transform).GetComponent<Window>();
			this.inspectorWindow.title.text = "Inspector";
			var inspectorWindowSize = new Vector2(300, 300);
			this.inspectorWindow.gameObject.GetComponent<RectTransform>().sizeDelta = inspectorWindowSize;
			this.inspectorWindow.transform.position = new Vector3(Screen.width - (inspectorWindowSize.x / 2f) - 5, this.mapObjectWindow.transform.position.y - inspectorWindowSize.y - 5, 0);

			this.inspector = GameObject.Instantiate(Assets.MapObjectInspectorPrefab, this.inspectorWindow.content.transform).GetComponent<MapObjectInspector>();

			this.inspector.animationButton.onClick.AddListener(this.OnClickAnimationButton);

			this.animationWindow = GameObject.Instantiate(Assets.AnimationWindowPrefab, this.transform).GetComponent<AnimationWindow>();
			this.animationWindow.title.text = "Animation";
			var animationWindowSize = new Vector2(300, 300);
			this.animationWindow.gameObject.GetComponent<RectTransform>().sizeDelta = animationWindowSize;
			this.animationWindow.transform.position = new Vector3((animationWindowSize.x / 2f) + 5, Screen.height - (animationWindowSize.y / 2f) - 35, 0);
			this.animationWindow.gameObject.SetActive(false);

			this.animationWindow.deleteButton.onClick.AddListener(this.HandleDeleteAnimationKeyframe);
			this.animationWindow.addButton.onClick.AddListener(this.HandleAddAnimationKeyframe);
			this.animationWindow.closeButton.onClick.AddListener(this.CloseAnimationWindow);

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
			this.inspector.gameObject.SetActive(false);
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
				this.CloseAnimationWindow();
			}

			if (this.inspector?.visualTarget)
			{
				this.inspector.positionInput.SetWithoutEvent((Vector2) this.inspector.visualTarget.transform.position);
				this.inspector.sizeInput.SetWithoutEvent((Vector2) this.inspector.visualTarget.transform.localScale);
				this.inspector.rotationInput.SetWithoutEvent(this.inspector.visualTarget.transform.rotation.eulerAngles.z);
			}

			if (this.inspector.gameObject.activeSelf && !this.inspector.interactionTarget)
			{
				this.UnlinkInspector();
			}
		}

		private void OnClickOpen()
		{
			FileDialog.OpenDialog(file =>
			{
				this.CloseAnimationWindow();
				this.editor.LoadMap(file);
			});
		}

		private void OnClickSaveAs()
		{
			FileDialog.SaveDialog(filename => this.editor.SaveMap(filename));
		}

		private void OnClickSave()
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

		private void OnClickUndo()
		{
			this.editor.commandHistory.Undo();
			this.RefreshAnimationWindow();
		}

		private void OnClickRedo()
		{
			this.editor.commandHistory.Execute();
			this.RefreshAnimationWindow();
		}

		private void OnClickAnimationButton()
		{
			if (this.animationWindow.gameObject.activeSelf)
			{
				this.CloseAnimationWindow();
			}
			else
			{
				this.OpenAnimationWindow();
			}
		}

		private void OpenAnimationWindow()
		{
			var handler = this.selectedActionHandlers[0];
			var anim = handler.GetComponent<MapObjectAnimation>();

			if (anim)
			{
				this.editor.animationHandler.SetAnimation(anim);
			}
			else
			{
				this.editor.animationHandler.AddAnimation(handler.GetComponentInParent<MapObjectInstance>().gameObject);
				anim = this.editor.animationHandler.animation;
			}

			this.RefreshAnimationWindow();
			this.animationWindow.gameObject.SetActive(true);

			this.inspector.animationButton.gameObject.GetComponentInChildren<Text>().text = "Close Animation";

			foreach (var btn in this.mapObjectWindow.content.GetComponentsInChildren<Button>())
			{
				btn.interactable = false;
				btn.gameObject.GetComponentInChildren<Text>().color = new Color32(200, 200, 200, 100);
			}
		}

		public void CloseAnimationWindow()
		{
			this.animationWindow.gameObject.SetActive(false);
			this.editor.animationHandler.SetAnimation(null);

			foreach (Transform child in this.animationWindow.content.transform)
			{
				GameObject.Destroy(child.gameObject);
			}

			this.inspector.animationButton.gameObject.GetComponentInChildren<Text>().text = "Edit Animation";

			foreach (var btn in this.mapObjectWindow.content.GetComponentsInChildren<Button>())
			{
				btn.interactable = true;
				btn.gameObject.GetComponentInChildren<Text>().color = new Color32(200, 200, 200, 255);
			}
		}

		private KeyframeSettings AddAnimationKeyframeSettings(int keyframe)
		{
			var anim = this.editor.animationHandler.animation;
			var keyframeSettings = GameObject.Instantiate(Assets.KeyframeSettingsPrefab, this.animationWindow.content.transform).GetComponent<KeyframeSettings>();

			keyframeSettings.contentFoldout.label.text = keyframe == 0 ? "Base" : $"Keyframe {keyframe}";

			if (keyframe == 0)
			{
				keyframeSettings.contentFoldout.label.text = "Base";
				GameObject.Destroy(keyframeSettings.contentFoldout.content);
			}
			else
			{
				keyframeSettings.contentFoldout.label.text = $"Keyframe {keyframe}";
			}

			keyframeSettings.onDurationChanged += (value, type) =>
			{
				if (type == TextSliderInput.ChangeType.ChangeStart)
				{
					this.editor.commandHistory.PreventNextMerge();
				}

				float durationDelta = value - anim.keyframes[keyframe].duration;
				var cmd = new ChangeKeyframeDurationCommand(anim.gameObject, durationDelta, keyframe);
				this.editor.commandHistory.Add(cmd, true);

				anim.keyframes[keyframe].UpdateCurve();
			};

			keyframeSettings.onEasingChanged += value =>
			{
				var curveType =
					value == "In" ? AnimationKeyframe.CurveType.EaseIn :
					value == "Out" ? AnimationKeyframe.CurveType.EaseOut :
					value == "In and Out" ? AnimationKeyframe.CurveType.EaseInOut :
					AnimationKeyframe.CurveType.Linear;

				var cmd = new ChangeKeyframeEasingCommand(anim.gameObject, curveType, keyframe);
				this.editor.commandHistory.Add(cmd);
			};

			keyframeSettings.onClick += () =>
			{
				foreach (var settings in this.animationWindow.content.GetComponentsInChildren<KeyframeSettings>())
				{
					settings.SetSelected(settings == keyframeSettings);
				}

				this.editor.animationHandler.SetKeyframe(keyframe);
				this.animationWindow.deleteButton.interactable = keyframe > 0;
			};

			keyframeSettings.durationInput.Value = anim.keyframes[keyframe].duration;
			keyframeSettings.easingDropdown.value = (int) anim.keyframes[keyframe].curveType;

			return keyframeSettings;
		}

		private void HandleAddAnimationKeyframe()
		{
			var anim = this.editor.animationHandler.animation;
			var cmd = new AddKeyframeCommand(anim.gameObject, new AnimationKeyframe(anim.keyframes[anim.keyframes.Count - 1]), anim.keyframes.Count);
			this.editor.commandHistory.Add(cmd);

			this.UnlinkInspector();
			this.LinkInspector(this.editor.animationHandler.keyframeMapObject);
			this.RefreshAnimationWindow();
		}

		private void HandleDeleteAnimationKeyframe()
		{
			var cmd = new DeleteKeyframeCommand(this.editor.animationHandler.animation.gameObject, this.editor.animationHandler.KeyframeIndex);
			this.editor.commandHistory.Add(cmd);

			this.UnlinkInspector();
			this.LinkInspector(this.editor.animationHandler.keyframeMapObject);
			this.RefreshAnimationWindow();
		}

		public void RefreshAnimationWindow()
		{
			foreach (Transform child in this.animationWindow.content.transform)
			{
				GameObject.Destroy(child.gameObject);
			}

			var anim = this.editor.animationHandler.animation;

			if (!anim)
			{
				return;
			}

			for (int i = 0; i < anim.keyframes.Count; i++)
			{
				var settings = this.AddAnimationKeyframeSettings(i);
				settings.SetSelected(i == this.editor.animationHandler.KeyframeIndex);
			}

			this.animationWindow.deleteButton.interactable = this.editor.animationHandler.KeyframeIndex > 0;
		}

		private void OpenMapFolder()
		{
			Application.OpenURL($"file://{BepInEx.Paths.GameRootPath}/maps");
		}

		private void OpenMapObjectWindow()
		{
			this.mapObjectWindow.gameObject.SetActive(true);
		}

		private void OpenInspectorWindow()
		{
			this.inspectorWindow.gameObject.SetActive(true);
		}

		private void InspectorPositionChanged(Vector2 value)
		{
			var delta = (Vector3) value - this.inspector.visualTarget.transform.position;
			var cmd = new MoveCommand(this.inspector.visualTarget.GetComponent<EditorActionHandler>(), delta, this.editor.animationHandler.KeyframeIndex);
			this.editor.commandHistory.Add(cmd);
			this.editor.UpdateRopeAttachments(false);
		}

		private void InspectorSizeChanged(Vector2 value)
		{
			var delta = (Vector3) value - this.inspector.visualTarget.transform.localScale;
			var cmd = new ResizeCommand(this.inspector.visualTarget.GetComponent<EditorActionHandler>(), delta, 0, this.editor.animationHandler.KeyframeIndex);
			this.editor.commandHistory.Add(cmd);
			this.editor.UpdateRopeAttachments(false);
		}

		private void InspectorRotationChanged(float value, TextSliderInput.ChangeType type)
		{
			if (type == TextSliderInput.ChangeType.ChangeStart)
			{
				this.editor.commandHistory.PreventNextMerge();
			}

			var fromRotation = this.inspector.visualTarget.transform.rotation;
			var toRotation = Quaternion.AngleAxis(value, Vector3.forward);
			var actionHandler = this.inspector.visualTarget.GetComponent<EditorActionHandler>();
			var cmd = new RotateCommand(actionHandler, fromRotation, toRotation, this.editor.animationHandler.KeyframeIndex);
			this.editor.commandHistory.Add(cmd, true);

			if (type == TextSliderInput.ChangeType.ChangeEnd)
			{
				this.editor.UpdateRopeAttachments(false);
			}
		}

		private void LinkInspector(GameObject interactionTarget)
		{
			this.LinkInspector(interactionTarget, interactionTarget);
		}

		private void LinkInspector(GameObject interactionTarget, GameObject visualTarget)
		{
			this.inspector.gameObject.SetActive(true);

			var actionHandler = visualTarget.GetComponent<EditorActionHandler>();

			this.inspector.positionInput.Value = visualTarget.transform.position;
			this.inspector.sizeInput.Value = visualTarget.transform.localScale;
			this.inspector.rotationInput.Value = visualTarget.transform.rotation.eulerAngles.z;

			this.inspector.positionInput.onChanged += this.InspectorPositionChanged;
			this.inspector.sizeInput.onChanged += this.InspectorSizeChanged;
			this.inspector.rotationInput.onChanged += this.InspectorRotationChanged;

			this.inspector.visualTarget = visualTarget;
			this.inspector.interactionTarget = interactionTarget;

			this.inspector.sizeInput.SetEnabled(actionHandler.CanResize());
			this.inspector.rotationInput.SetEnabled(actionHandler.CanRotate());
		}

		private void UnlinkInspector()
		{
			this.inspector.positionInput.onChanged -= this.InspectorPositionChanged;
			this.inspector.sizeInput.onChanged -= this.InspectorSizeChanged;
			this.inspector.rotationInput.onChanged -= this.InspectorRotationChanged;

			this.inspector.visualTarget = null;
			this.inspector.interactionTarget = null;
			this.inspector.gameObject.SetActive(false);
		}

		public void HandleSelectedObjectsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var list = this.editor.selectedActionHandlers;

			foreach (Transform child in this.transform)
			{
				if (child == this.toolbar.transform || child == this.mapObjectWindow.transform || child == this.inspectorWindow.transform || child == this.animationWindow.transform)
				{
					continue;
				}

				GameObject.Destroy(child.gameObject);
			}

			this.UnlinkInspector();

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

				this.LinkInspector(handlerGameObject);
			}

			if (list.Count == 0 && this.editor.animationHandler.animation && this.editor.animationHandler.enabled)
			{
				this.LinkInspector(this.editor.animationHandler.animation.gameObject, this.editor.animationHandler.keyframeMapObject);
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

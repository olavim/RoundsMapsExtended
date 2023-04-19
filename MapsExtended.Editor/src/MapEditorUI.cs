using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MapsExt.Editor.UI;
using System;
using System.Linq;

namespace MapsExt.Editor
{
	public static class AnchorPosition
	{
		public static readonly Dictionary<int, Vector2> directionMultipliers = new()
		{
			{ AnchorPosition.Middle, new(0, 0) },
			{ AnchorPosition.TopLeft, new(-1f, 1f) },
			{ AnchorPosition.TopMiddle, new(0, 1f) },
			{ AnchorPosition.TopRight, new(1f, 1f) },
			{ AnchorPosition.MiddleRight, new(1f, 0) },
			{ AnchorPosition.BottomRight, new(1f, -1f) },
			{ AnchorPosition.BottomMiddle, new(0, -1f) },
			{ AnchorPosition.BottomLeft, new(-1f, -1f) },
			{ AnchorPosition.MiddleLeft, new(-1f, 0) }
		};

		public static readonly Dictionary<int, Vector2> sizeMultipliers = new()
		{
			{ AnchorPosition.Middle, new(1f, 1f) },
			{ AnchorPosition.TopLeft, new(-1f, 1f) },
			{ AnchorPosition.TopMiddle, new(0, 1f) },
			{ AnchorPosition.TopRight, new(1f, 1f) },
			{ AnchorPosition.MiddleRight, new(1f, 0) },
			{ AnchorPosition.BottomRight, new(1f, -1f) },
			{ AnchorPosition.BottomMiddle, new(0, -1f) },
			{ AnchorPosition.BottomLeft, new(-1f, -1f) },
			{ AnchorPosition.MiddleLeft, new(-1f, 0) }
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
		[SerializeField] private MapEditor _editor;
		[SerializeField] private Toolbar _toolbar;
		[SerializeField] private Window _mapObjectWindow;
		[SerializeField] private Window _inspectorWindow;
		[SerializeField] private AnimationWindow _animationWindow;
		[SerializeField] private MapObjectInspector _inspector;
		private Texture2D _selectionTexture;
		private Window[] _windows;
		private bool[] _windowWasOpen;
		private Vector2 _resolution;

		public MapEditor Editor { get => this._editor; set => this._editor = value; }
		public Toolbar Toolbar { get => this._toolbar; set => this._toolbar = value; }
		public Window MapObjectWindow { get => this._mapObjectWindow; set => this._mapObjectWindow = value; }
		public Window InspectorWindow { get => this._inspectorWindow; set => this._inspectorWindow = value; }
		public AnimationWindow AnimationWindow { get => this._animationWindow; set => this._animationWindow = value; }
		public MapObjectInspector Inspector { get => this._inspector; set => this._inspector = value; }

		protected virtual void Awake()
		{
			this._resolution = new Vector2(Screen.width, Screen.height);

			this.Toolbar.FileMenu.AddItem(new MenuItemBuilder().Label("Open...").Action(this.OnClickOpen).KeyBinding(NamedKeyCode.O, NamedKeyCode.Ctrl));
			this.Toolbar.FileMenu.AddItem(new MenuItemBuilder().Label("Save").Action(this.OnClickSave).KeyBinding(NamedKeyCode.S, NamedKeyCode.Ctrl));
			this.Toolbar.FileMenu.AddItem(new MenuItemBuilder().Label("Save As...").Action(this.OnClickSaveAs).KeyBinding(NamedKeyCode.S, NamedKeyCode.Ctrl, NamedKeyCode.Shift));
			this.Toolbar.FileMenu.AddItem(new MenuItemBuilder().Label("Open Map Folder").Action(this.OnClickOpenMapFolder).Item());

			this.Toolbar.EditMenu.AddItem(new MenuItemBuilder().Label("Copy").Action(this.OnClickCopy).KeyBinding(NamedKeyCode.C, NamedKeyCode.Ctrl));
			this.Toolbar.EditMenu.AddItem(new MenuItemBuilder().Label("Paste").Action(this.OnClickPaste).KeyBinding(NamedKeyCode.V, NamedKeyCode.Ctrl));
			this.Toolbar.EditMenu.AddItem(new MenuItemBuilder().Label("Undo").Action(this.OnClickUndo).KeyBinding(NamedKeyCode.Z, NamedKeyCode.Ctrl));
			this.Toolbar.EditMenu.AddItem(new MenuItemBuilder().Label("Redo").Action(this.OnClickRedo).KeyBinding(NamedKeyCode.Z, NamedKeyCode.Ctrl, NamedKeyCode.Shift));

			this.Toolbar.WindowMenu.AddItem(new MenuItemBuilder().Label("Map Objects").Action(this.OpenMapObjectWindow));
			this.Toolbar.WindowMenu.AddItem(new MenuItemBuilder().Label("Inspector").Action(this.OpenInspectorWindow));

			var mapObjects = new Dictionary<string, List<(string, Type)>>
			{
				{ "", new List<(string, Type)>() }
			};

			foreach (var (type, label, category) in MapsExtendedEditor.MapObjectAttributes)
			{
				if (!mapObjects.ContainsKey(category))
				{
					mapObjects.Add(category, new List<(string, Type)>());
				}

				mapObjects[category].Add((label, type));
			}

			foreach (var category in mapObjects.Keys.Where(k => k != ""))
			{
				var builder = new MenuItemBuilder().Label(category);

				foreach (var entry in mapObjects[category])
				{
					void Action() => this.Editor.CreateMapObject(entry.Item2);
					builder.SubItem(b => b.Label(entry.Item1).Action(Action));
				}

				this.Toolbar.MapObjectMenu.AddItem(builder.Item());
			}

			foreach (var entry in mapObjects[""])
			{
				void Action() => this.Editor.CreateMapObject(entry.Item2);
				var builder = new MenuItemBuilder().Label(entry.Item1).Action(Action);
				this.Toolbar.MapObjectMenu.AddItem(builder.Item());
			}

			this.Toolbar.GridSizeSlider.value = this.Editor.GridSize;
			this.Toolbar.GridSizeSlider.onValueChanged.AddListener(val => this.Editor.GridSize = val);

			this.Toolbar.OnToggleSimulation += simulated =>
			{
				if (simulated)
				{
					this.Editor.OnStartSimulation();
					for (int i = 0; i < this._windows.Length; i++)
					{
						this._windowWasOpen[i] = this._windows[i].gameObject.activeSelf;
						this._windows[i].gameObject.SetActive(false);
					}
				}
				else
				{
					this.Editor.OnStopSimulation();
					for (int i = 0; i < this._windows.Length; i++)
					{
						this._windows[i].gameObject.SetActive(this._windowWasOpen[i]);
					}
				}

				var menuState = simulated ? Menu.MenuState.DISABLED : Menu.MenuState.INACTIVE;
				this.Toolbar.FileMenu.SetState(menuState);
				this.Toolbar.EditMenu.SetState(menuState);
				this.Toolbar.MapObjectMenu.SetState(menuState);
				this.Toolbar.WindowMenu.SetState(menuState);
				this.Toolbar.GridSizeSlider.transform.parent.parent.gameObject.SetActive(!simulated);
			};

			var mapObjectWindowSize = this.MapObjectWindow.gameObject.GetComponent<RectTransform>().sizeDelta;
			this.MapObjectWindow.transform.position = new Vector3(Screen.width - (mapObjectWindowSize.x / 2f) - 5, Screen.height - (mapObjectWindowSize.y / 2f) - 35, 0);

			var inspectorWindowSize = this.InspectorWindow.gameObject.GetComponent<RectTransform>().sizeDelta;
			this.InspectorWindow.transform.position = new Vector3(Screen.width - (inspectorWindowSize.x / 2f) - 5, this.MapObjectWindow.transform.position.y - inspectorWindowSize.y - 5, 0);

			var animationWindowSize = this.AnimationWindow.gameObject.GetComponent<RectTransform>().sizeDelta;
			this.AnimationWindow.transform.position = new Vector3((animationWindowSize.x / 2f) + 5, Screen.height - (animationWindowSize.y / 2f) - 35, 0);

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

			foreach (var item in this.Toolbar.MapObjectMenu.Items)
			{
				if (item.items == null)
				{
					var button = CreateButton(item);
					button.transform.SetParent(this.MapObjectWindow.Content.transform);
				}
				else
				{
					var foldout = GameObject.Instantiate(Assets.FoldoutPrefab, this.MapObjectWindow.Content.transform).GetComponent<Foldout>();
					foldout.Label.text = item.label;

					foreach (var subitem in item.items)
					{
						var button = CreateButton(subitem);
						button.transform.SetParent(foldout.Content.transform);
					}
				}
			}
		}

		protected virtual void Start()
		{
			this._windows = new Window[] { this.MapObjectWindow, this.InspectorWindow, this.AnimationWindow };
			this._windowWasOpen = new bool[this._windows.Length];
			this._selectionTexture = UIUtils.GetTexture(2, 2, new Color32(255, 255, 255, 20));
		}

		protected virtual void Update()
		{
			if (this.Editor.IsSimulating)
			{
				return;
			}

			var newResolution = this.GetComponent<RectTransform>().sizeDelta;

			if (newResolution != this._resolution)
			{
				float extraOffset = 35;

				foreach (var window in this._windows)
				{
					var size = window.GetComponent<RectTransform>().sizeDelta;

					window.transform.position = new Vector2(
						newResolution.x - (size.x / 2f) - 5,
						newResolution.y - (size.y / 2f) - extraOffset
					);

					extraOffset += size.y + 5;
				}

				this._resolution = newResolution;
			}

			this.Toolbar.EditMenu.SetItemEnabled("Undo", this.Editor.CanUndo());
			this.Toolbar.EditMenu.SetItemEnabled("Redo", this.Editor.CanRedo());
			this.Toolbar.EditMenu.SetItemEnabled("Copy", this.Editor.AnimationHandler.Animation == null);
			this.Toolbar.EditMenu.SetItemEnabled("Paste", this.Editor.AnimationHandler.Animation == null);

			if (this.Toolbar.MapObjectMenu.State == Menu.MenuState.DISABLED && this.Editor.AnimationHandler.Animation == null)
			{
				this.Toolbar.MapObjectMenu.SetState(Menu.MenuState.INACTIVE);
			}

			if (this.Toolbar.MapObjectMenu.State != Menu.MenuState.DISABLED && this.Editor.AnimationHandler.Animation != null)
			{
				this.Toolbar.MapObjectMenu.SetState(Menu.MenuState.DISABLED);
			}

			if (!this.AnimationWindow.gameObject.activeSelf && this.Editor.AnimationHandler.Animation != null)
			{
				this.AnimationWindow.Open();
			}
			else if (this.AnimationWindow.gameObject.activeSelf && this.Editor.AnimationHandler.Animation == null)
			{
				this.AnimationWindow.Close();
			}

			bool animWindowOpen = this.AnimationWindow.gameObject.activeSelf;
			byte alpha = (byte) (animWindowOpen ? 100 : 255);
			foreach (var btn in this.MapObjectWindow.Content.GetComponentsInChildren<Button>())
			{
				btn.interactable = !animWindowOpen;
				btn.gameObject.GetComponentInChildren<Text>().color = new Color32(200, 200, 200, alpha);
			}
		}

		public void OnClickOpen()
		{
			FileDialog.OpenDialog((Action<string>) (file =>
			{
				this.AnimationWindow.Close();
				this.Editor.LoadMap(file);
			}));
		}

		public void OnClickSaveAs()
		{
			FileDialog.SaveDialog(filename => this.Editor.SaveMap(filename));
		}

		public void OnClickSave()
		{
			if (this.Editor.CurrentMapName?.Length > 0)
			{
				this.Editor.SaveMap(this.Editor.CurrentMapName);
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
			this.Editor.OnUndo();
			this.AnimationWindow.Refresh();
		}

		public void OnClickRedo()
		{
			this.Editor.OnRedo();
			this.AnimationWindow.Refresh();
		}

		public void OnClickCopy()
		{
			this.Editor.OnCopy();
		}

		public void OnClickPaste()
		{
			this.StartCoroutine(this.Editor.OnPaste());
		}

		public void OpenMapObjectWindow()
		{
			this.MapObjectWindow.gameObject.SetActive(true);
		}

		public void OpenInspectorWindow()
		{
			this.InspectorWindow.gameObject.SetActive(true);
		}

		public void OnGUI()
		{
			if (!this.Editor.IsSimulating)
			{
				var selectionStyle = new GUIStyle(GUI.skin.box);
				selectionStyle.normal.background = this._selectionTexture;
				var selectionRect = this.Editor.GetSelection();

				if (selectionRect.width > 11 && selectionRect.height > 11)
				{
					GUI.Box(selectionRect, GUIContent.none, selectionStyle);
				}
			}
		}
	}
}

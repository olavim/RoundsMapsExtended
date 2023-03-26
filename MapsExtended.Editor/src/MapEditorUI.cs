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
		private Window[] windows;
		private bool[] windowWasOpen;
		private Vector2 resolution;

		protected virtual void Awake()
		{
			this.resolution = new Vector2(Screen.width, Screen.height);

			this.toolbar.fileMenu.AddItem(new MenuItemBuilder().Label("Open...").Action(this.OnClickOpen).KeyBinding(NamedKeyCode.O, NamedKeyCode.Ctrl));
			this.toolbar.fileMenu.AddItem(new MenuItemBuilder().Label("Save").Action(this.OnClickSave).KeyBinding(NamedKeyCode.S, NamedKeyCode.Ctrl));
			this.toolbar.fileMenu.AddItem(new MenuItemBuilder().Label("Save As...").Action(this.OnClickSaveAs).KeyBinding(NamedKeyCode.S, NamedKeyCode.Ctrl, NamedKeyCode.Shift));
			this.toolbar.fileMenu.AddItem(new MenuItemBuilder().Label("Open Map Folder").Action(this.OnClickOpenMapFolder).Item());

			this.toolbar.editMenu.AddItem(new MenuItemBuilder().Label("Copy").Action(this.OnClickCopy).KeyBinding(NamedKeyCode.C, NamedKeyCode.Ctrl));
			this.toolbar.editMenu.AddItem(new MenuItemBuilder().Label("Paste").Action(this.OnClickPaste).KeyBinding(NamedKeyCode.V, NamedKeyCode.Ctrl));
			this.toolbar.editMenu.AddItem(new MenuItemBuilder().Label("Undo").Action(this.OnClickUndo).KeyBinding(NamedKeyCode.Z, NamedKeyCode.Ctrl));
			this.toolbar.editMenu.AddItem(new MenuItemBuilder().Label("Redo").Action(this.OnClickRedo).KeyBinding(NamedKeyCode.Z, NamedKeyCode.Ctrl, NamedKeyCode.Shift));

			this.toolbar.windowMenu.AddItem(new MenuItemBuilder().Label("Map Objects").Action(this.OpenMapObjectWindow));
			this.toolbar.windowMenu.AddItem(new MenuItemBuilder().Label("Inspector").Action(this.OpenInspectorWindow));

			var mapObjects = new Dictionary<string, List<Tuple<string, Type>>>();
			mapObjects.Add("", new List<Tuple<string, Type>>());

			foreach (var attr in MapsExtendedEditor.instance.mapObjectAttributes)
			{
				string category = attr.Item2.category ?? "";

				if (!mapObjects.ContainsKey(category))
				{
					mapObjects.Add(category, new List<Tuple<string, Type>>());
				}

				mapObjects[category].Add(new Tuple<string, Type>(attr.Item2.label, attr.Item1));
			}

			foreach (var category in mapObjects.Keys.Where(k => k != ""))
			{
				var builder = new MenuItemBuilder().Label(category);

				foreach (var entry in mapObjects[category])
				{
					void Action() => this.editor.CreateMapObject(entry.Item2);
					builder.SubItem(b => b.Label(entry.Item1).Action(Action));
				}

				this.toolbar.mapObjectMenu.AddItem(builder.Item());
			}

			foreach (var entry in mapObjects[""])
			{
				void Action() => this.editor.CreateMapObject(entry.Item2);
				var builder = new MenuItemBuilder().Label(entry.Item1).Action(Action);
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

		protected virtual void Start()
		{
			this.windows = new Window[] { this.mapObjectWindow, this.inspectorWindow, this.animationWindow };
			this.windowWasOpen = new bool[this.windows.Length];
			this.selectionTexture = UIUtils.GetTexture(2, 2, new Color32(255, 255, 255, 20));
		}

		protected virtual void Update()
		{
			if (this.editor.isSimulating)
			{
				return;
			}

			var newResolution = this.GetComponent<RectTransform>().sizeDelta;

			if (newResolution != this.resolution)
			{
				float extraOffset = 35;

				foreach (var window in this.windows)
				{
					var size = window.GetComponent<RectTransform>().sizeDelta;

					window.transform.position = new Vector2(
						newResolution.x - (size.x / 2f) - 5,
						newResolution.y - (size.y / 2f) - extraOffset
					);

					extraOffset += size.y + 5;
				}

				this.resolution = newResolution;
			}

			this.toolbar.editMenu.SetItemEnabled("Undo", this.editor.CanUndo());
			this.toolbar.editMenu.SetItemEnabled("Redo", this.editor.CanRedo());
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

			if (!this.animationWindow.gameObject.activeSelf && this.editor.animationHandler.animation != null)
			{
				this.animationWindow.Open();
			}
			else if (this.animationWindow.gameObject.activeSelf && this.editor.animationHandler.animation == null)
			{
				this.animationWindow.Close();
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
			this.editor.OnUndo();
			this.animationWindow.Refresh();
		}

		public void OnClickRedo()
		{
			this.editor.OnRedo();
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

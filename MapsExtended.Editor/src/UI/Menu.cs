using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Sirenix.Serialization;

namespace MapsExt.Editor.UI
{
	public enum MenuPosition
	{
		DOWN,
		RIGHT
	}

	public enum MenuTrigger
	{
		CLICK,
		HOVER
	}

	public class Menu : MonoBehaviour, ISerializationCallbackReceiver, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
	{
		public enum MenuState
		{
			INACTIVE,
			HIGHLIGHTED,
			ACTIVE,
			DISABLED
		}

		[SerializeField] private GameObject _contentTemplate;
		[SerializeField] private Graphic _graphic;
		[SerializeField] private Button _itemButton;
		[SerializeField] private Text _itemLabel;
		[SerializeField] private Text _itemHotkeyLabel;
		[SerializeField] private Text _label;
		[SerializeField] private Color _normal;
		[SerializeField] private Color _highlighted;
		[SerializeField] private Color _active;
		[SerializeField] private MenuPosition _position;
		[SerializeField] private MenuTrigger _openTrigger;
		[SerializeField] private MenuState _state = MenuState.INACTIVE;
		[SerializeField] private List<MenuItem> _items = new();
		private readonly Dictionary<string, MenuItem> _itemsByKey = new();
		private GameObject _content;

		public GameObject ContentTemplate { get => this._contentTemplate; set => this._contentTemplate = value; }
		public Graphic Graphic { get => this._graphic; set => this._graphic = value; }
		public Button ItemButton { get => this._itemButton; set => this._itemButton = value; }
		public Text ItemLabel { get => this._itemLabel; set => this._itemLabel = value; }
		public Text ItemHotkeyLabel { get => this._itemHotkeyLabel; set => this._itemHotkeyLabel = value; }
		public Text Label { get => this._label; set => this._label = value; }
		public Color Normal { get => this._normal; set => this._normal = value; }
		public Color Highlighted { get => this._highlighted; set => this._highlighted = value; }
		public Color Active { get => this._active; set => this._active = value; }
		public MenuPosition Position { get => this._position; set => this._position = value; }
		public MenuTrigger OpenTrigger { get => this._openTrigger; set => this._openTrigger = value; }
		public MenuState State { get => this._state; set => this._state = value; }
		public List<MenuItem> Items { get => this._items; set => this._items = value; }

		public Action OnOpen { get; set; }
		public Action OnClose { get; set; }
		public Action OnHighlight { get; set; }

		[SerializeField, HideInInspector]
		private byte[] _serializationData;
		[SerializeField, HideInInspector]
		private List<UnityEngine.Object> _serializationDataRefs;

		public void OnAfterDeserialize()
		{
			this.Items = SerializationUtility.DeserializeValue<List<MenuItem>>(this._serializationData, DataFormat.Binary, this._serializationDataRefs) ?? new();

			foreach (var item in this.Items)
			{
				this._itemsByKey.Add(item.label, item);
			}
		}

		public void OnBeforeSerialize()
		{
			this._serializationData = SerializationUtility.SerializeValue(this.Items, DataFormat.Binary, out this._serializationDataRefs);
		}

		protected virtual void Start()
		{
			this._content = GameObject.Instantiate(this.ContentTemplate, this.transform);

			this.SetState(MenuState.INACTIVE);

			var rectTransform = this._content.GetComponent<RectTransform>();

			if (this.Position == MenuPosition.DOWN)
			{
				rectTransform.anchorMin = new Vector2(0, 0);
				rectTransform.anchorMax = new Vector2(0, 0);
				rectTransform.pivot = new Vector2(0, 1);
			}
			else if (this.Position == MenuPosition.RIGHT)
			{
				rectTransform.anchorMin = new Vector2(1, 1);
				rectTransform.anchorMax = new Vector2(1, 1);
				rectTransform.pivot = new Vector2(0, 1);
			}

			this.ContentTemplate.SetActive(false);
			this.ItemButton.gameObject.SetActive(false);

			this.RedrawContent();
		}

		protected virtual void Update()
		{
			if (this.State == MenuState.DISABLED)
			{
				return;
			}

			bool isHovered = this.IsMouseInsideMenu();

			if (this.OpenTrigger == MenuTrigger.CLICK && EditorInput.GetMouseButtonDown(0) && !isHovered)
			{
				this.SetState(MenuState.INACTIVE);
			}

			if (this.OpenTrigger == MenuTrigger.HOVER)
			{
				this.SetState(isHovered ? MenuState.ACTIVE : MenuState.INACTIVE);
			}

			var sortedItems = this.Items
				.Where(item => item.keyBinding?.key != null && item.keyBinding.key.code != KeyCode.None)
				.ToList();

			sortedItems.Sort((a, b) => b.keyBinding.modifiers.Count - a.keyBinding.modifiers.Count);

			var calledItem = sortedItems.Find(item => EditorInput.GetKeyDown(item.keyBinding.key.code) && item.keyBinding.modifiers.All(m => EditorInput.GetKey(m.code)));

			if (calledItem?.disabled == false)
			{
				calledItem.action?.Invoke();
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (this.State == MenuState.DISABLED || this.OpenTrigger == MenuTrigger.HOVER)
			{
				return;
			}

			if (this.State == MenuState.INACTIVE)
			{
				this.SetState(MenuState.HIGHLIGHTED);
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (this.State == MenuState.DISABLED || this.OpenTrigger == MenuTrigger.HOVER)
			{
				return;
			}

			if (this.State == MenuState.HIGHLIGHTED)
			{
				this.SetState(MenuState.INACTIVE);
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (this.State == MenuState.DISABLED || this.OpenTrigger == MenuTrigger.HOVER)
			{
				return;
			}

			var rectTransform = this.gameObject.GetComponent<RectTransform>();
			bool pressedButton = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, eventData.position);

			if (this.State == MenuState.ACTIVE && pressedButton)
			{
				this.SetState(MenuState.HIGHLIGHTED);
			}
			else
			{
				this.SetState(MenuState.ACTIVE);
			}
		}

		public void SetState(MenuState state)
		{
			bool wasOpened = this.State != MenuState.ACTIVE && state == MenuState.ACTIVE;
			bool wasClosed = this.State == MenuState.ACTIVE && state != MenuState.ACTIVE;
			bool wasHighlighted = this.State == MenuState.INACTIVE && state == MenuState.HIGHLIGHTED;

			this.State = state;
			Color newGraphicColor = this.Normal;
			Color newLabelColor = Color.white;

			if (state == MenuState.INACTIVE)
			{
				newGraphicColor = this.Normal;
				this._content.SetActive(false);
			}

			if (state == MenuState.HIGHLIGHTED)
			{
				newGraphicColor = this.Highlighted;
				this._content.SetActive(false);
			}

			if (state == MenuState.ACTIVE)
			{
				newGraphicColor = this.Active;
				this._content.SetActive(true);
			}

			if (state == MenuState.DISABLED)
			{
				newLabelColor.a = 0.3f;
				this._content.SetActive(false);
			}

			if (this.Graphic != null)
			{
				this.Graphic.color = newGraphicColor;
			}

			if (this.Label != null)
			{
				this.Label.color = newLabelColor;
			}

			if (wasOpened)
			{
				this.OnOpen?.Invoke();
			}

			if (wasClosed)
			{
				this.OnClose?.Invoke();
			}

			if (wasHighlighted)
			{
				this.OnHighlight?.Invoke();
			}
		}

		private bool IsMouseInsideMenu()
		{
			var rectTransforms = this.gameObject.GetComponentsInChildren<RectTransform>().ToList();
			rectTransforms.Add(this.gameObject.GetComponent<RectTransform>());
			return rectTransforms.Any(rt => RectTransformUtility.RectangleContainsScreenPoint(rt, EditorInput.MousePosition));
		}

		public void AddItem(MenuItemBuilder builder)
		{
			this.AddItem(builder.Item());
		}

		public void AddItem(MenuItem item)
		{
			this.PatchMenuItemActions(item);
			this.RegisterItem(item);
			this.Items.Add(item);
			this.RedrawContent();
		}

		public void SetItemEnabled(string key, bool enabled)
		{
			if (this._itemsByKey.TryGetValue(key, out MenuItem item))
			{
				if (item.disabled == enabled)
				{
					item.disabled = !enabled;
					this.RedrawContent();
				}
			}
		}

		private void PatchMenuItemActions(MenuItem item)
		{
			var oldAction = item.action;
			if (oldAction != null)
			{
				item.action = new UnityEvent();
				item.action.AddListener(() =>
				{
					oldAction.Invoke();
					this.SetState(MenuState.INACTIVE);
				});
			}

			var subitems = item.items ?? new List<MenuItem>() { };
			foreach (var subitem in subitems)
			{
				this.PatchMenuItemActions(subitem);
			}
		}

		private void RegisterItem(MenuItem item, string key = "")
		{
			key += item.label;
			this._itemsByKey.Add(key, item);

			if (item.items != null)
			{
				key += ".";
				foreach (var subitem in item.items)
				{
					this.RegisterItem(subitem, key);
				}
			}
		}

		public void RedrawContent()
		{
			if (this._content == null)
			{
				return;
			}

			GameObjectUtils.DestroyChildrenImmediateSafe(this._content);

			foreach (var item in this.Items)
			{
				this.ItemLabel.text = item.label;

				if (item.keyBinding != null)
				{
					var keys = item.keyBinding.modifiers.ToList();
					keys.Add(item.keyBinding.key);
					keys = keys.Where(k => k != null).ToList();
					this.ItemHotkeyLabel.text = string.Join("+", keys.Select(k => k.name));
				}
				else if (item.items != null)
				{
					this.ItemHotkeyLabel.text = "<color=#aaaaaa>></color>";
				}
				else
				{
					this.ItemHotkeyLabel.text = "";
				}

				var icol = this.ItemLabel.color;
				var hcol = this.ItemHotkeyLabel.color;
				this.ItemLabel.color = item.disabled ? new Color(icol.r, icol.g, icol.b, 0.5f) : new Color(icol.r, icol.g, icol.b, 1);
				this.ItemHotkeyLabel.color = item.disabled ? new Color(hcol.r, hcol.g, hcol.b, 0.5f) : new Color(hcol.r, hcol.g, hcol.b, 1);
				this.ItemButton.interactable = !item.disabled;

				var instance = GameObject.Instantiate(this.ItemButton.gameObject, this._content.transform);
				instance.SetActive(true);

				if (item.disabled)
				{
					continue;
				}

				var button = instance.GetComponent<Button>();
				button.onClick.AddListener(() => item.action?.Invoke());

				if (item.items != null)
				{
					instance.SetActive(false);

					var submenu = instance.AddComponent<Menu>();
					submenu.ContentTemplate = this.ContentTemplate;
					submenu.Graphic = null;
					submenu.ItemButton = this.ItemButton;
					submenu.ItemLabel = this.ItemLabel;
					submenu.ItemHotkeyLabel = this.ItemHotkeyLabel;
					submenu.Label = this.Label;
					submenu.Normal = this.Normal;
					submenu.Highlighted = this.Highlighted;
					submenu.Active = this.Active;
					submenu.Position = MenuPosition.RIGHT;
					submenu.OpenTrigger = MenuTrigger.HOVER;
					submenu.Items = item.items.ToList();

					instance.SetActive(true);
				}
			}
		}
	}

	[Serializable]
	public class MenuItem
	{
		public string label;
		public UnityEvent action;
		public KeyCombination keyBinding;
		public List<MenuItem> items;
		public bool disabled;
	}

	public class MenuItemBuilder
	{
		private readonly MenuItem item;

		public MenuItemBuilder()
		{
			this.item = new MenuItem();
		}

		public MenuItemBuilder Label(string label)
		{
			this.item.label = label;
			return this;
		}

		public MenuItemBuilder Action(UnityAction action)
		{
			this.item.action = new UnityEvent();
			this.item.action.AddListener(action);
			return this;
		}

		public MenuItemBuilder KeyBinding(NamedKeyCode hotkey, params NamedKeyCode[] modifiers)
		{
			this.item.keyBinding = new KeyCombination
			{
				key = hotkey,
				modifiers = modifiers.ToList()
			};
			return this;
		}

		public MenuItem Item()
		{
			return this.item;
		}

		public MenuItemBuilder SubItem(Action<MenuItemBuilder> cb)
		{
			var subBuilder = new MenuItemBuilder();
			cb(subBuilder);

			this.item.items ??= new List<MenuItem>();
			this.item.items.Add(subBuilder.Item());
			return this;
		}
	}

	[Serializable]
	public class KeyCombination : ISerializationCallbackReceiver
	{
		public List<NamedKeyCode> modifiers = new();
		public NamedKeyCode key;

		// Why is Unity's default serialization so helpless?
		[SerializeField, HideInInspector]
		private string[] _serializedModifiers;
		[SerializeField, HideInInspector]
		private string _serializedKey;

		public void OnAfterDeserialize()
		{
			foreach (string mod in this._serializedModifiers)
			{
				this.modifiers.Add(JsonUtility.FromJson<NamedKeyCode>(mod));
			}

			this.key = JsonUtility.FromJson<NamedKeyCode>(this._serializedKey);
		}

		public void OnBeforeSerialize()
		{
			this._serializedModifiers = new string[this.modifiers.Count];
			for (int i = 0; i < this.modifiers.Count; i++)
			{
				this._serializedModifiers[i] = JsonUtility.ToJson(this.modifiers[i]);
			}

			this._serializedKey = JsonUtility.ToJson(this.key);
		}
	}

	[Serializable]
	public class NamedKeyCode
	{
		public static readonly NamedKeyCode A = new(KeyCode.A, "A");
		public static readonly NamedKeyCode B = new(KeyCode.B, "B");
		public static readonly NamedKeyCode C = new(KeyCode.C, "C");
		public static readonly NamedKeyCode D = new(KeyCode.D, "D");
		public static readonly NamedKeyCode E = new(KeyCode.E, "E");
		public static readonly NamedKeyCode F = new(KeyCode.F, "F");
		public static readonly NamedKeyCode G = new(KeyCode.G, "G");
		public static readonly NamedKeyCode H = new(KeyCode.H, "H");
		public static readonly NamedKeyCode I = new(KeyCode.I, "I");
		public static readonly NamedKeyCode J = new(KeyCode.J, "J");
		public static readonly NamedKeyCode K = new(KeyCode.K, "K");
		public static readonly NamedKeyCode L = new(KeyCode.L, "L");
		public static readonly NamedKeyCode M = new(KeyCode.M, "M");
		public static readonly NamedKeyCode N = new(KeyCode.N, "N");
		public static readonly NamedKeyCode O = new(KeyCode.O, "O");
		public static readonly NamedKeyCode P = new(KeyCode.P, "P");
		public static readonly NamedKeyCode Q = new(KeyCode.Q, "Q");
		public static readonly NamedKeyCode R = new(KeyCode.R, "R");
		public static readonly NamedKeyCode S = new(KeyCode.S, "S");
		public static readonly NamedKeyCode T = new(KeyCode.T, "T");
		public static readonly NamedKeyCode U = new(KeyCode.U, "U");
		public static readonly NamedKeyCode V = new(KeyCode.V, "V");
		public static readonly NamedKeyCode W = new(KeyCode.W, "W");
		public static readonly NamedKeyCode X = new(KeyCode.X, "X");
		public static readonly NamedKeyCode Y = new(KeyCode.Y, "Y");
		public static readonly NamedKeyCode Z = new(KeyCode.Z, "Z");
		public static readonly NamedKeyCode Ctrl = new(KeyCode.LeftControl, "Ctrl");
		public static readonly NamedKeyCode Shift = new(KeyCode.LeftShift, "Shift");
		public static readonly NamedKeyCode Alt = new(KeyCode.LeftAlt, "Alt");

		public KeyCode code;
		public string name;

		public NamedKeyCode(KeyCode code, string name)
		{
			this.code = code;
			this.name = name;
		}
	}
}

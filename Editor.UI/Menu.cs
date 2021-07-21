using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MapsExtended.UI
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

    public class Menu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        public GameObject contentTemplate;
        public Graphic graphic;
        public Button itemButton;
        public Text itemLabel;
        public Text itemHotkeyLabel;
        public Text label;
        public Color normal;
        public Color highlighted;
        public Color active;
        public MenuPosition position;
        public MenuTrigger openTrigger;
        public MenuState state = MenuState.INACTIVE;
        public List<MenuItem> items = new List<MenuItem>();
        public Action onOpen;
        public Action onClose;
        public Action onHighlight;

        public enum MenuState
        {
            INACTIVE,
            HIGHLIGHTED,
            ACTIVE
        }

        private GameObject content;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (this.openTrigger == MenuTrigger.HOVER)
            {
                return;
            }

            if (this.state == MenuState.INACTIVE)
            {
                this.SetState(MenuState.HIGHLIGHTED);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (this.openTrigger == MenuTrigger.HOVER)
            {
                return;
            }

            if (this.state == MenuState.HIGHLIGHTED)
            {
                this.SetState(MenuState.INACTIVE);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (this.openTrigger == MenuTrigger.HOVER)
            {
                return;
            }

            var rectTransform = this.gameObject.GetComponent<RectTransform>();
            bool pressedButton = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, eventData.position);

            if (this.state == MenuState.ACTIVE && pressedButton)
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
            bool wasOpened = this.state != MenuState.ACTIVE && state == MenuState.ACTIVE;
            bool wasClosed = this.state == MenuState.ACTIVE && state != MenuState.ACTIVE;
            bool wasHighlighted = this.state == MenuState.INACTIVE && state == MenuState.HIGHLIGHTED;

            this.state = state;
            Color newGraphicColor = this.normal;

            if (state == MenuState.INACTIVE)
            {
                newGraphicColor = this.normal;
                this.content.SetActive(false);
            }

            if (state == MenuState.HIGHLIGHTED)
            {
                newGraphicColor = this.highlighted;
                this.content.SetActive(false);
            }

            if (state == MenuState.ACTIVE)
            {
                newGraphicColor = this.active;
                this.content.SetActive(true);
            }

            if (this.graphic != null)
            {
                this.graphic.color = newGraphicColor;
            }

            if (wasOpened)
            {
                this.onOpen?.Invoke();
            }

            if (wasClosed)
            {
                this.onClose?.Invoke();
            }

            if (wasHighlighted)
            {
                this.onHighlight?.Invoke();
            }
        }

        public void Start()
        {
            this.content = GameObject.Instantiate(this.contentTemplate, this.transform);

            this.SetState(MenuState.INACTIVE);

            var rectTransform = this.content.GetComponent<RectTransform>();

            if (this.position == MenuPosition.DOWN)
            {
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);
                rectTransform.pivot = new Vector2(0, 1);
            }
            else if (this.position == MenuPosition.RIGHT)
            {
                rectTransform.anchorMin = new Vector2(1, 1);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.pivot = new Vector2(0, 1);
            }

            this.contentTemplate.SetActive(false);
            this.itemButton.gameObject.SetActive(false);

            this.RedrawContent();
        }

        public void Update()
        {
            bool isHovered = this.IsMouseInsideMenu();

            if (this.openTrigger == MenuTrigger.CLICK && Input.GetMouseButtonDown(0) && !isHovered)
            {
                this.SetState(MenuState.INACTIVE);
            }

            if (this.openTrigger == MenuTrigger.HOVER)
            {
                this.SetState(isHovered ? MenuState.ACTIVE : MenuState.INACTIVE);
            }

            var sortedItems = this.items.Where(item => item.keyBinding != null && item.keyBinding.key != null && item.keyBinding.key.code != KeyCode.None).ToList();
            sortedItems.Sort((a, b) =>
            {
                return b.keyBinding.modifiers.Length - a.keyBinding.modifiers.Length;
            });

            var calledItem = sortedItems.Find(item =>
            {
                return Input.GetKeyDown(item.keyBinding.key.code) && item.keyBinding.modifiers.All(m => Input.GetKey(m.code));
            });

            if (calledItem != null)
            {
                calledItem.action?.Invoke();
            }
        }

        private bool IsMouseInsideMenu()
        {
            var rectTransforms = this.gameObject.GetComponentsInChildren<RectTransform>().ToList();
            rectTransforms.Add(this.gameObject.GetComponent<RectTransform>());
            return rectTransforms.Any(rt => RectTransformUtility.RectangleContainsScreenPoint(rt, Input.mousePosition));
        }

        public void AddMenuItem(MenuItem item)
        {
            this.PatchMenuItemActions(item);
            this.items.Add(item);
            this.RedrawContent();
        }

        private void PatchMenuItemActions(MenuItem item)
        {
            var oldAction = item.action;
            if (oldAction != null)
            {
                item.action = () =>
                {
                    oldAction();
                    this.SetState(MenuState.INACTIVE);
                };
            }

            var subitems = item.items ?? new MenuItem[] { };
            foreach (var subitem in subitems)
            {
                this.PatchMenuItemActions(subitem);
            }
        }

        public void RedrawContent()
        {
            if (this.content == null)
            {
                return;
            }

            foreach (Transform child in this.content.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            foreach (var item in this.items)
            {
                this.itemLabel.text = item.label;

                if (item.keyBinding != null)
                {
                    var keys = item.keyBinding.modifiers.ToList();
                    keys.Add(item.keyBinding.key);
                    keys = keys.Where(k => k != null).ToList();
                    this.itemHotkeyLabel.text = string.Join("+", keys.Select(k => k.name));
                }
                else if (item.items != null)
                {
                    this.itemHotkeyLabel.text = "<color=#aaaaaa>></color>";
                }

                var instance = GameObject.Instantiate(this.itemButton.gameObject, this.content.transform);
                instance.SetActive(true);

                var button = instance.GetComponent<Button>();
                button.onClick.AddListener(() => item.action?.Invoke());

                if (item.items != null)
                {
                    instance.SetActive(false);

                    var submenu = instance.AddComponent<Menu>();
                    submenu.contentTemplate = this.contentTemplate;
                    submenu.graphic = null;
                    submenu.itemButton = this.itemButton;
                    submenu.itemLabel = this.itemLabel;
                    submenu.itemHotkeyLabel = this.itemHotkeyLabel;
                    submenu.label = this.label;
                    submenu.normal = this.normal;
                    submenu.highlighted = this.highlighted;
                    submenu.active = this.active;
                    submenu.position = MenuPosition.RIGHT;
                    submenu.openTrigger = MenuTrigger.HOVER;
                    submenu.items = item.items.ToList();

                    instance.SetActive(true);
                }
            }
        }
    }

    [Serializable]
    public class MenuItem
    {
        public string label;
        public Action action;
        public KeyCombination keyBinding;
        public IEnumerable<MenuItem> items;

        public MenuItem(string label, Action action = null, NamedKeyCode hotkey = null, params NamedKeyCode[] modifiers)
        {
            this.label = label;
            this.action = action;
            this.keyBinding = new KeyCombination
            {
                key = hotkey,
                modifiers = modifiers
            };
            this.items = null;
        }

        public MenuItem(string label, IEnumerable<MenuItem> items)
        {
            this.label = label;
            this.action = null;
            this.keyBinding = null;
            this.items = items;
        }
    }

    [Serializable]
    public class KeyCombination
    {
        public NamedKeyCode[] modifiers;
        public NamedKeyCode key;
    }

    [Serializable]
    public class NamedKeyCode
    {
        public KeyCode code;
        public string name;

        public NamedKeyCode(KeyCode code, string name)
        {
            this.code = code;
            this.name = name;
        }
    }
}

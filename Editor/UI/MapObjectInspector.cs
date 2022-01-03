using UnityEngine.UI;
using UnityEngine;
using MapsExt.Editor.ActionHandlers;
using MapsExt.Editor.Commands;
using MapsExt.Editor.MapObjects;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine.Events;
using System.Collections.Generic;
using MapsExt.MapObjects;

namespace MapsExt.Editor.UI
{
	public class MapObjectInspector : MonoBehaviour
	{
		public class InspectorPropertyEntry : Attribute
		{
			public string name;
			public Type commandType;
			public int handlerIndex;

			public InspectorPropertyEntry(string name, Type commandType, int handlerIndex)
			{
				this.name = name;
				this.commandType = commandType;
				this.handlerIndex = handlerIndex;
			}
		}

		[AttributeUsage(AttributeTargets.Property, Inherited = true)]
		public class Vector2Property : InspectorPropertyEntry
		{
			public Vector2Property(string name, Type commandType, int handlerIndex = 0) : base(name, commandType, handlerIndex) { }
		}

		[AttributeUsage(AttributeTargets.Property, Inherited = true)]
		public class QuaternionProperty : InspectorPropertyEntry
		{
			public QuaternionProperty(string name, Type commandType, int handlerIndex = 0) : base(name, commandType, handlerIndex) { }
		}

		[AttributeUsage(AttributeTargets.Property, Inherited = true)]
		public class BooleanProperty : InspectorPropertyEntry
		{
			public BooleanProperty(string name, Type commandType, int handlerIndex = 0) : base(name, commandType, handlerIndex) { }
		}

		[AttributeUsage(AttributeTargets.Method, Inherited = true)]
		public class ButtonBuilder : Attribute { }

		public MapObjectInstance target;
		public EditorActionHandler targetHandler;
		public MapEditor editor;

		public Action onUpdate;

		private void Update()
		{
			this.onUpdate?.Invoke();
		}

		public void Link(MapObjectInstance target, EditorActionHandler targetHandler)
		{
			this.target = target;
			this.targetHandler = targetHandler;
			this.gameObject.SetActive(true);

			foreach (Transform child in this.transform)
			{
				GameObject.Destroy(child.gameObject);
			}

			var blueprint = MapsExtendedEditor.instance.mapObjectManager.blueprints[this.target.dataType];

			if (blueprint is IInspectable)
			{
				var builder = new InspectorLayoutBuilder();
				((IInspectable) blueprint).OnInspectorLayout(this, builder);

				foreach (var elem in builder.layout.elements)
				{
					var instance = this.GetLayoutElementInstance(elem);

					if (!instance)
					{
						throw new NotSupportedException($"Unknown inspector element: {elem.GetType()}");
					}

					instance.transform.SetParent(this.transform);
				}
			}
		}

		public void Unlink()
		{
			this.onUpdate = null;

			foreach (Transform child in this.transform)
			{
				GameObject.Destroy(child.gameObject);
			}

			this.target = null;
			this.targetHandler = null;
			this.gameObject.SetActive(false);
		}

		private GameObject GetLayoutElementInstance(ILayoutElement elem)
		{
			var propertyVector2 = elem as InspectorLayoutProperty<Vector2>;
			var propertyQuaternion = elem as InspectorLayoutProperty<Quaternion>;
			var propertyBool = elem as InspectorLayoutProperty<bool>;
			var buttonElem = elem as InspectorLayoutButton;
			var divider = elem as InspectorDivider;

			if (propertyVector2 != null)
			{
				var instance = GameObject.Instantiate(Assets.InspectorVector2Prefab);
				var c = instance.GetComponent<InspectorVector2>();
				c.label.text = propertyVector2.name;
				c.input.SetWithoutEvent(propertyVector2.getValue());

				c.input.onChanged += value =>
				{
					var cmd = propertyVector2.getCommand(value);
					this.editor.ExecuteCommand(cmd);
					propertyVector2.onChanged?.Invoke();
				};

				this.onUpdate += () => c.input.SetWithoutEvent(propertyVector2.getValue());
				return instance;
			}

			if (propertyQuaternion != null)
			{
				var instance = GameObject.Instantiate(Assets.InspectorQuaternionPrefab);
				var c = instance.GetComponent<InspectorQuaternion>();
				c.label.text = propertyQuaternion.name;
				c.input.SetWithoutEvent(propertyQuaternion.getValue().eulerAngles.z);

				c.input.onChanged += (value, changeType) =>
				{
					if (changeType == TextSliderInput.ChangeType.ChangeStart)
					{
						this.editor.PreventNextCommandMerge();
					}

					var cmd = propertyQuaternion.getCommand(Quaternion.Euler(0, 0, value));
					this.editor.ExecuteCommand(cmd, true);

					if (changeType == TextSliderInput.ChangeType.ChangeEnd)
					{
						this.editor.UpdateRopeAttachments();
						propertyQuaternion.onChanged?.Invoke();
					}
				};

				this.onUpdate += () => c.input.SetWithoutEvent(propertyQuaternion.getValue().eulerAngles.z);
				return instance;
			}

			if (propertyBool != null)
			{
				var instance = GameObject.Instantiate(Assets.InspectorBooleanPrefab);
				var prop = instance.GetComponent<InspectorBoolean>();
				prop.label.text = propertyBool.name;
				prop.input.isOn = propertyBool.getValue();

				UnityAction<bool> onValueChanged = value =>
				{
					var cmd = propertyBool.getCommand(value);
					this.editor.ExecuteCommand(cmd);
				};

				prop.input.onValueChanged.AddListener(onValueChanged);

				this.onUpdate += () =>
				{
					prop.input.onValueChanged.RemoveListener(onValueChanged);
					prop.input.isOn = propertyBool.getValue();
					prop.input.onValueChanged.AddListener(onValueChanged);
				};

				return instance;
			}

			if (buttonElem != null)
			{
				var instance = GameObject.Instantiate(Assets.InspectorButtonPrefab);
				var button = instance.GetComponentInChildren<Button>();

				button.onClick.AddListener(() => buttonElem.onClick());
				this.onUpdate += () =>
				{
					if (button)
					{
						buttonElem.onUpdate(button);
					}
				};

				return instance;
			}

			if (divider != null)
			{
				return GameObject.Instantiate(Assets.InspectorDividerPrefab);
			}

			return null;
		}
	}

	public class InspectorLayout
	{
		public List<ILayoutElement> elements = new List<ILayoutElement>();
	}

	public class InspectorLayoutBuilder
	{
		public InspectorLayout layout
		{
			get
			{
				var l = new InspectorLayout();
				l.elements = this.propertyBuilders.Select(b => b.element).ToList();
				return l;
			}
		}

		public List<ILayoutElementBuilder> propertyBuilders = new List<ILayoutElementBuilder>();

		public InspectorPropertyBuilder<T> Property<T>(string name)
		{
			var builder = new InspectorPropertyBuilder<T>();
			this.propertyBuilders.Add(builder.Name(name));
			return builder;
		}

		public InspectorButtonBuilder Button()
		{
			var builder = new InspectorButtonBuilder();
			this.propertyBuilders.Add(builder);
			return builder;
		}

		public InspectorDividerBuilder Divider()
		{
			var builder = new InspectorDividerBuilder();
			this.propertyBuilders.Add(builder);
			return builder;
		}
	}

	public interface ILayoutElement { }
	public interface ILayoutElementBuilder
	{
		ILayoutElement element { get; }
	}

	public class InspectorPropertyBuilder<T> : ILayoutElementBuilder
	{
		public ILayoutElement element { get; private set; }

		public InspectorPropertyBuilder()
		{
			this.element = new InspectorLayoutProperty<T>();
		}

		public InspectorPropertyBuilder<T> Name(string name)
		{
			(this.element as InspectorLayoutProperty<T>).name = name;
			return this;
		}

		public InspectorPropertyBuilder<T> CommandGetter(Func<T, ICommand> getCommand)
		{
			(this.element as InspectorLayoutProperty<T>).getCommand = getCommand;
			return this;
		}

		public InspectorPropertyBuilder<T> ValueGetter(Func<T> getValue)
		{
			(this.element as InspectorLayoutProperty<T>).getValue = getValue;
			return this;
		}

		public InspectorPropertyBuilder<T> ChangeEvent(Action onChanged)
		{
			(this.element as InspectorLayoutProperty<T>).onChanged = onChanged;
			return this;
		}
	}

	public class InspectorButtonBuilder : ILayoutElementBuilder
	{
		public ILayoutElement element { get; private set; }

		public InspectorButtonBuilder()
		{
			this.element = new InspectorLayoutButton();
		}

		public InspectorButtonBuilder ClickEvent(Action onClick)
		{
			(this.element as InspectorLayoutButton).onClick = onClick;
			return this;
		}

		public InspectorButtonBuilder UpdateEvent(Action<Button> onUpdate)
		{
			(this.element as InspectorLayoutButton).onUpdate = onUpdate;
			return this;
		}
	}

	public class InspectorDividerBuilder : ILayoutElementBuilder
	{
		public ILayoutElement element { get; private set; }

		public InspectorDividerBuilder()
		{
			this.element = new InspectorDivider();
		}
	}

	public class InspectorLayoutProperty<T> : ILayoutElement
	{
		public string name;
		public Func<T, ICommand> getCommand;
		public Func<T> getValue;
		public Action onChanged { get; set; }

		public InspectorLayoutProperty() { }

		public InspectorLayoutProperty(string name, Func<T, ICommand> getCommand, Func<T> getValue, Action onChanged = null)
		{
			this.name = name;
			this.getCommand = getCommand;
			this.getValue = getValue;
			this.onChanged = onChanged;
		}
	}

	public class InspectorLayoutButton : ILayoutElement
	{
		public Action onClick;
		public Action<Button> onUpdate;

		public InspectorLayoutButton() { }

		public InspectorLayoutButton(Action onClick, Action<Button> onUpdate)
		{
			this.onClick = onClick;
			this.onUpdate = onUpdate;
		}
	}

	public class InspectorDivider : ILayoutElement { }

	public class InspectorVector2 : MonoBehaviour
	{
		public Text label;
		public Vector2Input input;
	}

	public class InspectorQuaternion : MonoBehaviour
	{
		public Text label;
		public TextSliderInput input;
	}

	public class InspectorBoolean : MonoBehaviour
	{
		public Text label;
		public Toggle input;
	}
}

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

		public GameObject visualTarget;
		public GameObject interactionTarget;
		public MapEditor editor;
		public MapEditorUI editorUI;

		public Action onUpdate;

		private void Update()
		{
			this.onUpdate?.Invoke();
		}

		public void Link(GameObject interactionTarget)
		{
			this.Link(interactionTarget, interactionTarget);
		}

		public void Link(GameObject interactionTarget, GameObject visualTarget)
		{
			this.interactionTarget = interactionTarget;
			this.visualTarget = visualTarget;
			this.gameObject.SetActive(true);

			foreach (Transform child in this.transform)
			{
				GameObject.Destroy(child.gameObject);
			}

			var specs = visualTarget.GetComponentsInParent<InspectorSpec>();

			var builder = new InspectorLayoutBuilder();
			foreach (var spec in specs)
			{
				spec.OnInspectorLayout(builder, this.editor, this.editorUI);
			}

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

		public void Unlink()
		{
			this.onUpdate = null;

			foreach (Transform child in this.transform)
			{
				GameObject.Destroy(child.gameObject);
			}

			this.visualTarget = null;
			this.interactionTarget = null;
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
					this.editor.commandHistory.Add(cmd);
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
						this.editor.commandHistory.PreventNextMerge();
					}

					var cmd = propertyQuaternion.getCommand(Quaternion.Euler(0, 0, value));
					this.editor.commandHistory.Add(cmd, true);

					if (changeType == TextSliderInput.ChangeType.ChangeEnd)
					{
						this.editor.UpdateRopeAttachments(false);
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
					this.editor.commandHistory.Add(cmd);
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
		public InspectorLayout layout = new InspectorLayout();

		public InspectorLayoutBuilder Property<T>(string name, Func<T, ICommand> getCommand, Func<T> getValue)
		{
			this.layout.elements.Add(new InspectorLayoutProperty<T>(name, getCommand, getValue));
			return this;
		}

		public InspectorLayoutBuilder Button(Action onClick, Action<Button> onUpdate)
		{
			this.layout.elements.Add(new InspectorLayoutButton(onClick, onUpdate));
			return this;
		}

		public InspectorLayoutBuilder Divider()
		{
			this.layout.elements.Add(new InspectorDivider());
			return this;
		}
	}

	public interface ILayoutElement { }

	public class InspectorLayoutProperty<T> : ILayoutElement
	{
		public string name;
		public Func<T, ICommand> getCommand;
		public Func<T> getValue;

		public InspectorLayoutProperty(string name, Func<T, ICommand> getCommand, Func<T> getValue)
		{
			this.name = name;
			this.getCommand = getCommand;
			this.getValue = getValue;
		}
	}

	public class InspectorLayoutButton : ILayoutElement
	{
		public Action onClick;
		public Action<Button> onUpdate;

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

using UnityEngine.UI;
using UnityEngine;
using MapsExt.Editor.ActionHandlers;
using System;
using UnityEngine.Events;
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

using UnityEngine.UI;
using UnityEngine;
using MapsExt.Editor.ActionHandlers;
using System;
using UnityEngine.Events;
using MapsExt.MapObjects;
using System.Collections.Generic;
using HarmonyLib;

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
		public GameObject selectedObject;
		public MapEditor editor;

		public Action onUpdate;

		private void Update()
		{
			this.onUpdate?.Invoke();
		}

		public void Link(MapObjectInstance target, GameObject selectedObject)
		{
			this.target = target;
			this.selectedObject = selectedObject;
			this.gameObject.SetActive(true);

			foreach (Transform child in this.transform)
			{
				GameObject.Destroy(child.gameObject);
			}

			var dataTypeProperties = MapsExtendedEditor.instance.mapObjectManager.dataTypeProperties;
			var builder = new InspectorLayoutBuilder();

			foreach (var prop in dataTypeProperties.GetValueOrDefault(this.target.dataType, new List<Type>()))
			{
				if (typeof(IInspectable).IsAssignableFrom(prop))
				{
					var inspectable = (IInspectable) AccessTools.CreateInstance(prop);
					inspectable.OnInspectorLayout(this, builder);
				}
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

			this.target = null;
			this.selectedObject = null;
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

				var onChanged = this.GetPropertyChangeEvent(propertyVector2);
				c.input.onChanged += (value) => onChanged(value, ChangeType.All);

				this.onUpdate += () => c.input.SetWithoutEvent(propertyVector2.getValue());
				return instance;
			}

			if (propertyQuaternion != null)
			{
				var instance = GameObject.Instantiate(Assets.InspectorQuaternionPrefab);
				var c = instance.GetComponent<InspectorQuaternion>();
				c.label.text = propertyQuaternion.name;
				c.input.SetWithoutEvent(propertyQuaternion.getValue().eulerAngles.z);

				var onChanged = this.GetPropertyChangeEvent(propertyQuaternion);
				c.input.onChanged += (value, type) => onChanged(Quaternion.Euler(0, 0, value), type);

				// c.input.onChanged += (value, changeType) =>
				// {
				// 	if (changeType == TextSliderInput.ChangeType.ChangeStart)
				// 	{
				// 		this.editor.PreventNextCommandMerge();
				// 	}

				// 	var cmd = propertyQuaternion.getCommand(Quaternion.Euler(0, 0, value));
				// 	var delegates = propertyQuaternion.getCommandDelegates();
				// 	this.editor.ExecuteCommand(cmd, delegates, true);

				// 	if (changeType == TextSliderInput.ChangeType.ChangeEnd)
				// 	{
				// 		this.editor.UpdateRopeAttachments();
				// 		propertyQuaternion.onChanged?.Invoke();
				// 	}
				// };

				this.onUpdate += () => c.input.SetWithoutEvent(propertyQuaternion.getValue().eulerAngles.z);
				return instance;
			}

			if (propertyBool != null)
			{
				var instance = GameObject.Instantiate(Assets.InspectorBooleanPrefab);
				var prop = instance.GetComponent<InspectorBoolean>();
				prop.label.text = propertyBool.name;
				prop.input.isOn = propertyBool.getValue();

				var onChanged = this.GetPropertyChangeEvent(propertyBool);
				UnityAction<bool> onValueChanged = value => onChanged(value, ChangeType.All);

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

		private Action<T, ChangeType> GetPropertyChangeEvent<T>(InspectorLayoutProperty<T> prop)
		{
			return (value, type) =>
			{
				if (type == ChangeType.All)
				{
					prop.onChangeStart?.Invoke(value);
					prop.setValue?.Invoke(value);
					prop.onChanged?.Invoke(value);
				}

				if (type == ChangeType.ChangeStart)
				{
					prop.onChangeStart?.Invoke(value);
				}

				if (type == ChangeType.Change)
				{
					prop.setValue?.Invoke(value);
				}

				if (type == ChangeType.ChangeEnd)
				{
					prop.onChanged?.Invoke(value);
				}
			};
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

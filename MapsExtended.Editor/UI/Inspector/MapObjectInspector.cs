using UnityEngine.UI;
using UnityEngine;
using System;
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
		public MapEditor editor;

		public Action onUpdate;

		private void Update()
		{
			var instance = this.editor.activeObject?.GetComponent<MapObjectInstance>();

			if (instance != this.target)
			{
				this.Unlink();

				if (instance != null)
				{
					this.Link(instance);
				}
			}

			this.onUpdate?.Invoke();
		}

		private void Link(MapObjectInstance target)
		{
			this.target = target;

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

		private void Unlink()
		{
			this.onUpdate = null;

			foreach (Transform child in this.transform)
			{
				GameObject.Destroy(child.gameObject);
			}

			this.target = null;
		}

		private GameObject GetLayoutElementInstance(ILayoutElement elem)
		{
			if (elem is InspectorLayoutProperty<Vector2> propertyVector2)
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

			if (elem is InspectorLayoutProperty<Quaternion> propertyQuaternion)
			{
				var instance = GameObject.Instantiate(Assets.InspectorQuaternionPrefab);
				var c = instance.GetComponent<InspectorQuaternion>();
				c.label.text = propertyQuaternion.name;
				c.input.SetWithoutEvent(propertyQuaternion.getValue().eulerAngles.z);

				var onChanged = this.GetPropertyChangeEvent(propertyQuaternion);
				c.input.onChanged += (value, type) => onChanged(Quaternion.Euler(0, 0, value), type);

				this.onUpdate += () => c.input.SetWithoutEvent(propertyQuaternion.getValue().eulerAngles.z);
				return instance;
			}

			if (elem is InspectorLayoutProperty<bool> propertyBool)
			{
				var instance = GameObject.Instantiate(Assets.InspectorBooleanPrefab);
				var prop = instance.GetComponent<InspectorBoolean>();
				prop.label.text = propertyBool.name;
				prop.input.isOn = propertyBool.getValue();

				var onChanged = this.GetPropertyChangeEvent(propertyBool);
				void OnValueChanged(bool value) => onChanged(value, ChangeType.All);

				prop.input.onValueChanged.AddListener(OnValueChanged);

				this.onUpdate += () =>
				{
					prop.input.onValueChanged.RemoveListener(OnValueChanged);
					prop.input.isOn = propertyBool.getValue();
					prop.input.onValueChanged.AddListener(OnValueChanged);
				};

				return instance;
			}

			if (elem is InspectorLayoutButton buttonElem)
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

			if (elem is InspectorDivider divider)
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

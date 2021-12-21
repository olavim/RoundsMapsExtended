using UnityEngine.UI;
using UnityEngine;
using MapsExt.Editor.ActionHandlers;
using MapsExt.Editor.Commands;
using MapsExt.Editor.MapObjects;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine.Events;

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

		private Action onUpdate;

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

			foreach (var spec in specs)
			{
				foreach (var prop in spec.GetType().GetProperties())
				{
					var attr = prop.GetCustomAttributes(true).FirstOrDefault();

					if (attr is Vector2Property)
					{
						this.AddVector2Property((Vector2Property) attr, prop, spec);
					}

					if (attr is QuaternionProperty)
					{
						this.AddQuaternionProperty((QuaternionProperty) attr, prop, spec);
					}

					if (attr is BooleanProperty && this.editor.animationHandler.animation == null)
					{
						this.AddBooleanProperty((BooleanProperty) attr, prop, spec);
					}
				}
			}

			GameObject.Instantiate(Assets.InspectorDividerPrefab, this.transform);

			foreach (var spec in specs)
			{
				foreach (var method in spec.GetType().GetMethods())
				{
					var attr = method.GetCustomAttributes(true).FirstOrDefault();
					if (attr is ButtonBuilder)
					{
						this.AddButton((ButtonBuilder) attr, method, spec);
					}
				}
			}
		}

		private void AddVector2Property(Vector2Property attribute, PropertyInfo propInfo, object target)
		{
			var instance = GameObject.Instantiate(Assets.InspectorVector2Prefab, this.transform);
			var prop = instance.GetComponent<InspectorVector2>();
			prop.label.text = attribute.name;
			prop.input.SetWithoutEvent((Vector2) propInfo.GetValue(target));

			prop.input.onChanged += value =>
			{
				var prevValue = (Vector2) propInfo.GetValue(target);
				var delta = value - prevValue;
				var handler = this.interactionTarget.GetComponentsInChildren<EditorActionHandler>()[attribute.handlerIndex];

				var cmdCtor = attribute.commandType.GetConstructor(new[] { typeof(EditorActionHandler), typeof(Vector3) });
				var cmd = (ICommand) cmdCtor.Invoke(new object[] { handler, (Vector3) delta });
				this.editor.commandHistory.Add(cmd);
				this.editor.UpdateRopeAttachments(false);
			};

			this.onUpdate += () =>
			{
				prop.input.SetWithoutEvent((Vector2) propInfo.GetValue(target));
			};
		}

		private void AddQuaternionProperty(QuaternionProperty attribute, PropertyInfo propInfo, object target)
		{
			var instance = GameObject.Instantiate(Assets.InspectorQuaternionPrefab, this.transform);
			var prop = instance.GetComponent<InspectorQuaternion>();
			prop.label.text = attribute.name;

			var initialValue = (Quaternion) propInfo.GetValue(target);
			prop.input.SetWithoutEvent(initialValue.eulerAngles.z);

			prop.input.onChanged += (value, type) =>
			{
				if (type == TextSliderInput.ChangeType.ChangeStart)
				{
					this.editor.commandHistory.PreventNextMerge();
				}

				var prevValue = (Quaternion) propInfo.GetValue(target);
				var delta = Quaternion.Euler(0, 0, value) * Quaternion.Inverse(prevValue);
				var handler = this.interactionTarget.GetComponentsInChildren<EditorActionHandler>()[attribute.handlerIndex];

				var cmdCtor = attribute.commandType.GetConstructor(new[] { typeof(EditorActionHandler), typeof(Quaternion) });
				var cmd = (ICommand) cmdCtor.Invoke(new object[] { handler, delta });
				this.editor.commandHistory.Add(cmd, true);

				if (type == TextSliderInput.ChangeType.ChangeEnd)
				{
					this.editor.UpdateRopeAttachments(false);
				}
			};

			this.onUpdate += () =>
			{
				var value = (Quaternion) propInfo.GetValue(target);
				prop.input.SetWithoutEvent(value.eulerAngles.z);
			};
		}

		private void AddBooleanProperty(BooleanProperty attribute, PropertyInfo propInfo, object target)
		{
			var instance = GameObject.Instantiate(Assets.InspectorBooleanPrefab, this.transform);
			var prop = instance.GetComponent<InspectorBoolean>();
			prop.label.text = attribute.name;
			prop.input.isOn = (bool) propInfo.GetValue(target);

			UnityAction<bool> onValueChanged = value =>
			{
				var handler = this.interactionTarget.GetComponentsInChildren<EditorActionHandler>()[attribute.handlerIndex];
				var cmdCtor = attribute.commandType.GetConstructor(new[] { typeof(EditorActionHandler), typeof(bool) });
				var cmd = (ICommand) cmdCtor.Invoke(new object[] { handler, value });
				this.editor.commandHistory.Add(cmd);
			};

			prop.input.onValueChanged.AddListener(onValueChanged);

			this.onUpdate += () =>
			{
				prop.input.onValueChanged.RemoveListener(onValueChanged);
				prop.input.isOn = (bool) propInfo.GetValue(target);
				prop.input.onValueChanged.AddListener(onValueChanged);
			};
		}

		private void AddButton(ButtonBuilder attribute, MethodInfo methodInfo, object target)
		{
			var instance = (GameObject) methodInfo.Invoke(target, new object[] { this.editor, this.editorUI });
			instance.transform.SetParent(this.transform);
		}

		public void Unlink()
		{
			foreach (Transform child in this.transform)
			{
				GameObject.Destroy(child.gameObject);
			}

			this.onUpdate = null;
			this.visualTarget = null;
			this.interactionTarget = null;
			this.gameObject.SetActive(false);
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

	public class InspectorButton : MonoBehaviour
	{
		public Button button;
	}
}

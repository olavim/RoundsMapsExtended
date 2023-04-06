using UnityEngine.UI;
using UnityEngine;
using System;
using MapsExt.MapObjects;
using Sirenix.Utilities;
using System.Collections.Specialized;
using System.Collections;
using MapsExt.Properties;
using System.Linq;

namespace MapsExt.Editor.UI
{
	public sealed class MapObjectInspector : MonoBehaviour
	{
		[SerializeField] private MapEditor _editor;

		private bool _isLinked;
		private OrderedDictionary _elements;

		private Action OnUpdate { get; set; }

		private InspectorContext Context => new()
		{
			InspectorTarget = this.Target,
			Editor = this.Editor
		};

		public MapEditor Editor { get => this._editor; set => this._editor = value; }
		public GameObject Target { get; private set; }

		public IInspectorElement GetElement<T>() where T : IProperty
		{
			return (IInspectorElement) this._elements[typeof(T)];
		}

		private void Update()
		{
			var selectedInstances = this.Editor.SelectedObjects.Select(x => x.GetComponentInParent<MapObjectInstance>()).Distinct().ToList();
			GameObject instance = selectedInstances.Count == 1 ? selectedInstances[0].gameObject : null;

			if (instance != this.Target || (this._isLinked && this.Target == null))
			{
				this.Unlink();

				if (instance != null)
				{
					this.Link(instance);
				}
			}

			if (this.Target != null)
			{
				this.OnUpdate?.Invoke();
			}
		}

		private void Link(GameObject target)
		{
			this._isLinked = true;
			this.Target = target;

			GameObjectUtils.DestroyChildrenImmediateSafe(this.gameObject);

			var targetDataType = this.Target.GetComponent<MapObjectInstance>().DataType;

			this._elements = new();

			foreach (var member in MapsExtendedEditor.instance._propertyManager.GetSerializableMembers(targetDataType))
			{
				var propertyType = member.GetReturnType();
				var elementType = MapsExtendedEditor.instance._propertyInspectorElements.GetValueOrDefault(propertyType, null);

				if (elementType != null)
				{
					this._elements[propertyType] = (IInspectorElement) Activator.CreateInstance(elementType);
				}
			}

			foreach (DictionaryEntry entry in this._elements)
			{
				var elem = (IInspectorElement) entry.Value;
				var instance = elem.Instantiate(this.Context);

				if (!instance)
				{
					throw new NotSupportedException($"Unknown inspector element: {elem.GetType()}");
				}

				this.OnUpdate += elem.OnUpdate;
				instance.transform.SetParent(this.transform);
			}
		}

		private void Unlink()
		{
			this.OnUpdate = null;

			GameObjectUtils.DestroyChildrenImmediateSafe(this.gameObject);

			this.Target = null;
			this._isLinked = false;
		}
	}

	public class InspectorVector2 : MonoBehaviour
	{
		[SerializeField] private Text _label;
		[SerializeField] private Vector2Input _input;

		public Text Label { get => this._label; set => this._label = value; }
		public Vector2Input Input { get => this._input; set => this._input = value; }
	}

	public class InspectorQuaternion : MonoBehaviour
	{
		[SerializeField] private Text _label;
		[SerializeField] private TextSliderInput _input;

		public Text Label { get => this._label; set => this._label = value; }
		public TextSliderInput Input { get => this._input; set => this._input = value; }
	}

	public class InspectorBoolean : MonoBehaviour
	{
		[SerializeField] private Text _label;
		[SerializeField] private Toggle _input;

		public Text Label { get => this._label; set => this._label = value; }
		public Toggle Input { get => this._input; set => this._input = value; }
	}
}

﻿using UnityEngine;
using System;
using MapsExt.MapObjects;
using Sirenix.Utilities;
using System.Collections.Specialized;
using System.Collections;
using MapsExt.Properties;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using MapsExt.Utils;

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
			var instance = this.Editor.ActiveMapObject;

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

			MapsExt.Utils.GameObjectUtils.DestroyChildrenImmediateSafe(this.gameObject);

			var targetDataType = this.Target.GetComponent<MapObjectInstance>().DataType;

			this._elements = new();

			var members = new List<MemberInfo>();

			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
			members.AddRange(targetDataType.GetProperties(flags).Where(p => p.GetReturnType() != null));
			members.AddRange(targetDataType.GetFields(flags).Where(p => p.GetReturnType() != null));

			foreach (var member in members)
			{
				var propertyType = member.GetReturnType();
				var elementType = MapsExtendedEditor.PropertyInspectorElements.GetValueOrDefault(propertyType, null);

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

			MapsExt.Utils.GameObjectUtils.DestroyChildrenImmediateSafe(this.gameObject);

			this.Target = null;
			this._isLinked = false;
		}
	}
}

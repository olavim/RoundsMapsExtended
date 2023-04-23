using System;
using System.Collections.Generic;
using MapsExt.Properties;
using System.Reflection;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;
using MapsExt.MapObjects;

namespace MapsExt
{
	public class PropertyManager
	{
		public static PropertyManager Current { get; set; }

		private readonly Dictionary<Type, IPropertySerializer<IProperty>> _serializers = new();

		public void RegisterProperty(Type propertyType, IPropertySerializer<IProperty> serializer)
		{
			if (this._serializers.ContainsKey(propertyType))
			{
				throw new ArgumentException($"Property {propertyType.Name} is already registered", nameof(propertyType));
			}

			this._serializers[propertyType] = serializer;
		}

		public void Write(IProperty property, GameObject target)
		{
			var propertyType = property.GetType();
			var dataType = (target.GetComponent<MapObjectInstance>()?.DataType) ?? throw new ArgumentException("Target is not a map object", nameof(target));

			if (!this._serializers.ContainsKey(propertyType))
			{
				throw new ArgumentException($"Property {propertyType.Name} is not registered");
			}

			if (dataType.HasFieldOrProperty(propertyType))
			{
				this._serializers[propertyType].WriteProperty(property, target);
			}
		}

		public void Write(MapObjectData data, GameObject target)
		{
			foreach (var memberInfo in this.GetRegisteredMembers(data.GetType()))
			{
				var prop = (IProperty) memberInfo.GetFieldOrPropertyValue(data);
				this.Write(prop, target);
			}
		}

		public IProperty Read(GameObject instance, Type propertyType)
		{
			if (!this._serializers.ContainsKey(propertyType))
			{
				throw new ArgumentException($"Property {propertyType.Name} is not registered");
			}

			var dataType = instance.GetComponent<MapObjectInstance>()?.DataType ?? throw new ArgumentException("Not a map object", nameof(instance));

			if (!dataType.HasFieldOrProperty(propertyType))
			{
				return null;
			}

			return this._serializers[propertyType].ReadProperty(instance);
		}

		public IEnumerable<IProperty> ReadAll(GameObject instance, Type propertyType)
		{
			return this._serializers.Keys.Where(k => propertyType.IsAssignableFrom(k)).Select(k => this._serializers[k].ReadProperty(instance));
		}

		public IEnumerable<IProperty> ReadAll(GameObject instance)
		{
			var dataType = instance.GetComponent<MapObjectInstance>()?.DataType ?? throw new ArgumentException("Not a map object", nameof(instance));
			return this.GetRegisteredMembers(dataType).Select(m => this.Read(instance.gameObject, m.GetReturnType()));
		}

		private IEnumerable<MemberInfo> GetRegisteredMembers(Type dataType)
		{
			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

			var list = new List<MemberInfo>();
			list.AddRange(dataType.GetProperties(flags).Where(p => p.GetReturnType() != null && this._serializers.ContainsKey(p.GetReturnType())));
			list.AddRange(dataType.GetFields(flags).Where(p => p.GetReturnType() != null && this._serializers.ContainsKey(p.GetReturnType())));
			return list;
		}
	}
}

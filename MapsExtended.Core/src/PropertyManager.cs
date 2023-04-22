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
		private readonly Dictionary<Type, IPropertyWriter<IProperty>> _writers = new();

		public void RegisterWriter(Type propertyType, IPropertyWriter<IProperty> writer)
		{
			if (this._writers.ContainsKey(propertyType))
			{
				throw new ArgumentException($"Property writer for {propertyType.Name} is already registered", nameof(propertyType));
			}

			this._writers[propertyType] = writer;
		}

		public void Write(IProperty property, GameObject target)
		{
			var propertyType = property.GetType();
			var dataType = target.GetComponent<MapObjectInstance>()?.DataType;
			if (dataType == null)
			{
				throw new ArgumentException("Target is not a map object", nameof(target));
			}

			if (!this._writers.ContainsKey(propertyType))
			{
				throw new ArgumentException($"No property writer registered for {propertyType.Name}", nameof(property));
			}

			if (dataType.HasFieldOrProperty(propertyType))
			{
				this._writers[propertyType].WriteProperty(property, target);
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

		private IEnumerable<MemberInfo> GetRegisteredMembers(Type dataType)
		{
			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

			var list = new List<MemberInfo>();
			list.AddRange(dataType.GetProperties(flags).Where(p => p.GetReturnType() != null && this._writers.ContainsKey(p.GetReturnType())));
			list.AddRange(dataType.GetFields(flags).Where(p => p.GetReturnType() != null && this._writers.ContainsKey(p.GetReturnType())));
			return list;
		}
	}
}

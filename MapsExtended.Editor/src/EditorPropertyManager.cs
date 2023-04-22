using System;
using System.Linq;
using System.Collections.Generic;
using MapsExt.Properties;
using MapsExt.Editor.Properties;
using UnityEngine;
using System.Reflection;
using Sirenix.Utilities;
using MapsExt.MapObjects;

namespace MapsExt.Editor
{
	public class EditorPropertyManager : PropertyManager
	{
		private readonly Dictionary<Type, IPropertyReader<IProperty>> _readers = new();

		public void RegisterReader(Type propertyType, IPropertyReader<IProperty> reader)
		{
			if (this._readers.ContainsKey(propertyType))
			{
				throw new ArgumentException($"Property reader for {propertyType.Name} is already registered");
			}

			this._readers[propertyType] = reader;
		}

		public IProperty Read(GameObject instance, Type propertyType)
		{
			if (!this._readers.ContainsKey(propertyType))
			{
				throw new ArgumentException($"No property reader for {propertyType.Name} is registered");
			}

			var dataType = instance.GetComponent<MapObjectInstance>()?.DataType;
			if (dataType == null)
			{
				throw new ArgumentException("Instance is not a map object", nameof(instance));
			}

			if (!dataType.HasFieldOrProperty(propertyType))
			{
				return null;
			}

			return this._readers[propertyType].ReadProperty(instance);
		}

		public IEnumerable<IProperty> ReadAll(GameObject instance, Type propertyType)
		{
			return this._readers.Keys.Where(k => propertyType.IsAssignableFrom(k)).Select(k => this._readers[k].ReadProperty(instance));
		}

		public MapObjectData ReadMapObject(MapObjectInstance instance)
		{
			var data = (MapObjectData) Activator.CreateInstance(instance.DataType);
			foreach (var memberInfo in this.GetRegisteredMembers(instance.DataType))
			{
				var prop = this.Read(instance.gameObject, memberInfo.GetReturnType());
				memberInfo.SetMemberValue(data, prop);
			}
			return data;
		}

		private IEnumerable<MemberInfo> GetRegisteredMembers(Type dataType)
		{
			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

			var list = new List<MemberInfo>();
			list.AddRange(dataType.GetProperties(flags).Where(p => p.GetReturnType() != null && this._readers.ContainsKey(p.GetReturnType())));
			list.AddRange(dataType.GetFields(flags).Where(p => p.GetReturnType() != null && this._readers.ContainsKey(p.GetReturnType())));
			return list;
		}
	}
}

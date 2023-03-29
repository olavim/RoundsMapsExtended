using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using MapsExt.MapObjects.Properties;
using System.Reflection;
using System.Linq;
using Sirenix.Utilities;
using MapsExt.MapObjects;

namespace MapsExt
{
	public class PropertyManager
	{
		private readonly Dictionary<Type, IPropertySerializer> _serializers = new Dictionary<Type, IPropertySerializer>();

		public void RegisterProperty(Type propertyType, Type propertySerializerType)
		{
			var serializer = (IPropertySerializer) AccessTools.CreateInstance(propertySerializerType);
			this._serializers.Add(propertyType, serializer);
		}

		public IPropertySerializer GetSerializer(Type type)
		{
			return this._serializers[type];
		}

		public T GetProperty<T>(object obj) where T : IProperty
		{
			return (T) this.GetSerializableMember<T>(obj.GetType()).GetMemberValue(obj);
		}

		public T[] GetProperties<T>(MapObjectData data) where T : IProperty
		{
			return this.GetSerializableMembers<T>(data.GetType()).Select(m => (T) m.GetMemberValue(data)).ToArray();
		}

		public void SetProperty<T>(object obj, T property) where T : IProperty
		{
			this.GetSerializableMember<T>(obj.GetType()).SetMemberValue(obj, property);
		}

		public List<MemberInfo> GetSerializableMembers(Type type)
		{
			var list = new List<MemberInfo>();

			var props = type
				.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
				.Where(p => p.GetReturnType() != null && this._serializers.ContainsKey(p.GetReturnType()));
			var fields = type
				.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
				.Where(p => p.GetReturnType() != null && this._serializers.ContainsKey(p.GetReturnType()));

			list.AddRange(props);
			list.AddRange(fields);
			return list;
		}

		private MemberInfo GetSerializableMember<T>(Type type) where T : IProperty
		{
			return this.GetSerializableMembers(type).Find(m => typeof(T).IsAssignableFrom(m.GetReturnType()));
		}

		private MemberInfo[] GetSerializableMembers<T>(Type type) where T : IProperty
		{
			return this.GetSerializableMembers(type).Where(m => typeof(T).IsAssignableFrom(m.GetReturnType())).ToArray();
		}
	}
}

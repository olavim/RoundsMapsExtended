using System;
using System.Collections.Generic;
using HarmonyLib;
using MapsExt.Properties;
using System.Reflection;
using System.Linq;
using Sirenix.Utilities;
using MapsExt.MapObjects;

namespace MapsExt
{
	public class PropertyManager
	{
		private readonly Dictionary<Type, IPropertySerializer> _serializers = new();

		public void RegisterProperty(Type propertyType, Type propertySerializerType)
		{
			if (this._serializers.ContainsKey(propertyType))
			{
				throw new ArgumentException($"{propertyType.Name} is already registered");
			}

			this._serializers[propertyType] = (IPropertySerializer) AccessTools.CreateInstance(propertySerializerType);
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

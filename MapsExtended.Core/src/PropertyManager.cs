using System;
using System.Collections.Generic;
using HarmonyLib;
using MapsExt.Properties;
using System.Reflection;
using System.Linq;
using Sirenix.Utilities;

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

		public IPropertySerializer GetSerializer(Type propertyType)
		{
			return this._serializers[propertyType];
		}

		public IPropertySerializer GetSerializer<TProperty>() where TProperty : IProperty
		{
			return this.GetSerializer(typeof(TProperty));
		}

		public List<MemberInfo> GetSerializableMembers(Type dataType)
		{
			var list = new List<MemberInfo>();

			var props = dataType
				.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
				.Where(p => p.GetReturnType() != null && this._serializers.ContainsKey(p.GetReturnType()));
			var fields = dataType
				.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
				.Where(p => p.GetReturnType() != null && this._serializers.ContainsKey(p.GetReturnType()));

			list.AddRange(props);
			list.AddRange(fields);
			return list;
		}

		public MemberInfo[] GetSerializableMembers<TProperty>(Type dataType) where TProperty : IProperty
		{
			return this.GetSerializableMembers(dataType, typeof(TProperty));
		}

		public MemberInfo[] GetSerializableMembers(Type dataType, Type propertyType)
		{
			return this.GetSerializableMembers(dataType).Where(m => propertyType.IsAssignableFrom(m.GetReturnType())).ToArray();
		}

		public MemberInfo GetSerializableMember<TProperty>(Type dataType) where TProperty : IProperty
		{
			return this.GetSerializableMember(dataType, typeof(TProperty));
		}

		public MemberInfo GetSerializableMember(Type dataType, Type propertyType)
		{
			return this.GetSerializableMembers(dataType).Find(m => propertyType.IsAssignableFrom(m.GetReturnType()));
		}
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using MapsExt.MapObjects.Properties;
using System.Reflection;
using System.Linq;
using Sirenix.Utilities;

namespace MapsExt
{
	public class PropertyManager
	{
		private readonly Dictionary<Type, IPropertySerializer> serializers = new Dictionary<Type, IPropertySerializer>();

		public void RegisterProperty(Type propertyType, Type propertySerializerType)
		{
			var serializer = (IPropertySerializer) AccessTools.CreateInstance(propertySerializerType);
			this.serializers.Add(propertyType, serializer);
		}

		public void Serialize(GameObject instance, IProperty property)
		{
			var serializer = this.serializers[property.GetType()];
			serializer.Serialize(instance, property);
		}

		public void Deserialize(IProperty property, GameObject target)
		{
			var serializer = this.serializers[property.GetType()];
			serializer.Deserialize(property, target);
		}

		public IPropertySerializer GetSerializer(Type type)
		{
			return this.serializers[type];
		}

		public List<MemberInfo> GetSerializableMembers(Type type)
		{
			var list = new List<MemberInfo>();

			var props = type
				.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
				.Where(p => p.GetReturnType() != null && this.serializers.ContainsKey(p.GetReturnType()));
			var fields = type
				.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
				.Where(p => p.GetReturnType() != null && this.serializers.ContainsKey(p.GetReturnType()));

			list.AddRange(props);
			list.AddRange(fields);
			return list;
		}
	}
}

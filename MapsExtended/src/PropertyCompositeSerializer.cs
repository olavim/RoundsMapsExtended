using HarmonyLib;
using MapsExt.MapObjects;
using MapsExt.Properties;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnboundLib;
using UnityEngine;

namespace MapsExt
{
	public class PropertyCompositeSerializer : IMapObjectSerializer
	{
		private readonly PropertyManager _propertyManager;
		private readonly Dictionary<Type, List<(MemberInfo, IPropertySerializer)>> _memberSerializerCache = new();

		public PropertyCompositeSerializer(PropertyManager propertyManager)
		{
			this._propertyManager = propertyManager;
		}

		public void Deserialize(MapObjectData data, GameObject target)
		{
			try
			{
				var mapObjectInstance = target.GetOrAddComponent<MapObjectInstance>();
				mapObjectInstance.MapObjectId = data.mapObjectId ?? Guid.NewGuid().ToString();
				mapObjectInstance.DataType = data.GetType();
				target.SetActive(data.active);

				this.CacheMemberSerializers(mapObjectInstance.DataType);

				foreach (var (memberInfo, serializer) in this._memberSerializerCache[mapObjectInstance.DataType])
				{
					var prop = (IProperty) memberInfo.GetFieldOrPropertyValue(data);
					serializer.Deserialize(prop, mapObjectInstance.gameObject);
				}
			}
			catch (Exception ex)
			{
				throw new MapObjectSerializationException($"Could not deserialize {data.GetType()} into {target.name}", ex);
			}
		}

		public MapObjectData Serialize(MapObjectInstance mapObjectInstance)
		{
			try
			{
				var data = (MapObjectData) AccessTools.CreateInstance(mapObjectInstance.DataType);

				data.mapObjectId = mapObjectInstance.MapObjectId;
				data.active = mapObjectInstance.gameObject.activeSelf;

				this.CacheMemberSerializers(mapObjectInstance.DataType);

				foreach (var (memberInfo, serializer) in this._memberSerializerCache[mapObjectInstance.DataType])
				{
					// var prop = (IProperty) memberInfo.GetFieldOrPropertyValue(data);
					var prop = serializer.Serialize(mapObjectInstance.gameObject);
					memberInfo.SetMemberValue(data, prop);
				}

				return data;
			}
			catch (Exception ex)
			{
				throw new MapObjectSerializationException($"Could not serialize map object: {mapObjectInstance.gameObject.name}", ex);
			}
		}

		private void CacheMemberSerializers(Type type)
		{
			if (this._memberSerializerCache.ContainsKey(type))
			{
				return;
			}

			var serializableMembers = this._propertyManager.GetSerializableMembers(type);

			foreach (var memberInfo in serializableMembers)
			{
				if (memberInfo is PropertyInfo propertyInfo && !propertyInfo.CanWrite)
				{
					throw new MapObjectSerializationException($"Property {propertyInfo.Name} on {type.Name} is not writable");
				}
			}

			var serializers = serializableMembers.Select(p => (p, this._propertyManager.GetSerializer(p.GetReturnType())));
			this._memberSerializerCache[type] = serializers.ToList();
		}
	}
}

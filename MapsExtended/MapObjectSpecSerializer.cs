using HarmonyLib;
using MapsExt.MapObjects;
using System;
using UnboundLib;
using UnityEngine;

namespace MapsExt
{
	[Obsolete("Deprecated")]
	public class MapObjectSpecSerializer : IMapObjectSerializer
	{
		static class BaseMapObjectSerializer
		{
			public static void Serialize(GameObject instance, MapObjectData target)
			{
				target.active = instance.activeSelf;
			}

			public static void Deserialize(MapObjectData data, GameObject target)
			{
				var c = target.GetOrAddComponent<MapObjectInstance>();
				c.dataType = data.GetType();
				target.SetActive(data.active);
			}
		}

		private readonly SerializerAction<MapObject> _serializer;
		private readonly DeserializerAction<MapObject> _deserializer;

		public MapObjectSpecSerializer(SerializerAction<MapObject> serializer, DeserializerAction<MapObject> deserializer)
		{
			this._serializer = serializer ?? throw new ArgumentException("Serializer cannot be null");
			this._deserializer = deserializer ?? throw new ArgumentException("Deserializer cannot be null");
		}

		public void Deserialize(MapObjectData data, GameObject target)
		{
			try
			{
				BaseMapObjectSerializer.Deserialize(data, target);
				this._deserializer((MapObject) data, target);
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
				var data = (MapObject) AccessTools.CreateInstance(mapObjectInstance.dataType);
				BaseMapObjectSerializer.Serialize(mapObjectInstance.gameObject, data);
				this._serializer(mapObjectInstance.gameObject, data);
				return data;
			}
			catch (Exception ex)
			{
				throw new MapObjectSerializationException($"Could not serialize {mapObjectInstance.gameObject.name}", ex);
			}
		}
	}
}

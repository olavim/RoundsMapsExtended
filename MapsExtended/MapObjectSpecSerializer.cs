using HarmonyLib;
using MapsExt.MapObjects;
using System;
using UnboundLib;
using UnityEngine;

#pragma warning disable CS0618

namespace MapsExt
{
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

		private readonly SerializerAction<MapObject> serializer;
		private readonly DeserializerAction<MapObject> deserializer;

		public MapObjectSpecSerializer(SerializerAction<MapObject> serializer, DeserializerAction<MapObject> deserializer)
		{
			this.serializer = serializer ?? throw new ArgumentException("Serializer cannot be null");
			this.deserializer = deserializer ?? throw new ArgumentException("Deserializer cannot be null");
		}

		public void Deserialize(MapObjectData data, GameObject target)
		{
			try
			{
				BaseMapObjectSerializer.Deserialize(data, target);
				this.deserializer((MapObject) data, target);
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
				this.serializer(mapObjectInstance.gameObject, data);
				return data;
			}
			catch (Exception ex)
			{
				throw new MapObjectSerializationException($"Could not serialize {mapObjectInstance.gameObject.name}", ex);
			}
		}
	}
}

#pragma warning restore CS0618

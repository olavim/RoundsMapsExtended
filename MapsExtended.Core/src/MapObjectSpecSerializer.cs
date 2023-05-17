using MapsExt.MapObjects;
using System;
using UnboundLib;
using UnityEngine;

namespace MapsExt
{
	[Obsolete("Deprecated")]
	public class MapObjectSpecSerializer : IMapObjectSerializer
	{
		protected DeserializerAction<MapObjectData> Writer { get; }
		protected SerializerAction<MapObjectData> Reader { get; }

		public MapObjectSpecSerializer(DeserializerAction<MapObjectData> deserializer, SerializerAction<MapObjectData> serializer)
		{
			this.Writer = deserializer ?? throw new ArgumentException("Deserializer cannot be null");
			this.Reader = serializer ?? throw new ArgumentException("Serializer cannot be null");
		}

		public void WriteMapObject(MapObjectData data, GameObject target)
		{
			try
			{
				var c = target.GetOrAddComponent<MapObjectInstance>();
				c.MapObjectId = data.MapObjectId ?? Guid.NewGuid().ToString();
				c.DataType = data.GetType();
				target.SetActive(data.Active);
				this.Writer((MapObjectData) data, target);
			}
			catch (Exception ex)
			{
				throw new MapObjectSerializationException($"Could not deserialize {data.GetType()} into {target.name}", ex);
			}
		}

		public MapObjectData ReadMapObject(MapObjectInstance mapObjectInstance)
		{
			try
			{
				var data = (MapObjectData) Activator.CreateInstance(mapObjectInstance.DataType);
				data.MapObjectId = mapObjectInstance.MapObjectId;
				data.Active = mapObjectInstance.gameObject.activeSelf;
				this.Reader(mapObjectInstance.gameObject, data);
				return data;
			}
			catch (Exception ex)
			{
				throw new MapObjectSerializationException($"Could not serialize {mapObjectInstance.gameObject.name}", ex);
			}
		}
	}
}

using MapsExt.MapObjects;
using System;
using UnboundLib;
using UnityEngine;

namespace MapsExt
{
	[Obsolete("Deprecated")]
	public class MapObjectSpecSerializer : IMapObjectSerializer
	{
		protected DeserializerAction<MapObject> Writer { get; }
		protected SerializerAction<MapObject> Reader { get; }

		public MapObjectSpecSerializer(DeserializerAction<MapObject> deserializer, SerializerAction<MapObject> serializer)
		{
			this.Writer = deserializer ?? throw new ArgumentException("Deserializer cannot be null");
			this.Reader = serializer ?? throw new ArgumentException("Serializer cannot be null");
		}

		public void WriteMapObject(MapObjectData data, GameObject target)
		{
			try
			{
				var c = target.GetOrAddComponent<MapObjectInstance>();
				c.DataType = data.GetType();
				target.SetActive(data.Active);
				this.Writer((MapObject) data, target);
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
				var data = (MapObject) Activator.CreateInstance(mapObjectInstance.DataType);
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

using MapsExt.MapObjects;
using System;
using UnboundLib;
using UnityEngine;

namespace MapsExt
{
	[Obsolete("Deprecated")]
	public class MapObjectSpecSerializer : IMapObjectSerializer
	{
		protected DeserializerAction<MapObject> Deserializer { get; }

		public MapObjectSpecSerializer(DeserializerAction<MapObject> deserializer)
		{
			this.Deserializer = deserializer ?? throw new ArgumentException("Deserializer cannot be null");
		}

		public void Deserialize(MapObjectData data, GameObject target)
		{
			try
			{
				var c = target.GetOrAddComponent<MapObjectInstance>();
				c.DataType = data.GetType();
				target.SetActive(data.active);

				this.Deserializer((MapObject) data, target);
			}
			catch (Exception ex)
			{
				throw new MapObjectSerializationException($"Could not deserialize {data.GetType()} into {target.name}", ex);
			}
		}
	}
}

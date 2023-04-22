using MapsExt.MapObjects;
using MapsExt.Properties;
using System;
using UnboundLib;
using UnityEngine;

namespace MapsExt
{
	public class PropertyCompositeSerializer : IMapObjectSerializer
	{
		private readonly PropertyManager _propertyManager;

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
				this._propertyManager.Write(data, target);
			}
			catch (Exception ex)
			{
				throw new MapObjectSerializationException($"Could not deserialize {data.GetType()} into {target.name}", ex);
			}
		}
	}
}

using MapsExt.MapObjects;
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

		public void WriteMapObject(MapObjectData data, GameObject target)
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
				throw new MapObjectSerializationException($"Could not write {data.GetType()} into {target.name}", ex);
			}
		}

		public MapObjectData ReadMapObject(MapObjectInstance mapObjectInstance)
		{
			try
			{
				var data = (MapObjectData) Activator.CreateInstance(mapObjectInstance.DataType);
				data.mapObjectId = mapObjectInstance.MapObjectId;
				data.active = mapObjectInstance.gameObject.activeSelf;

				foreach (var prop in this._propertyManager.ReadAll(mapObjectInstance.gameObject))
				{
					data.SetProperty(prop);
				}

				return data;
			}
			catch (Exception ex)
			{
				throw new MapObjectSerializationException($"Could not read {mapObjectInstance.gameObject.name}", ex);
			}
		}
	}
}

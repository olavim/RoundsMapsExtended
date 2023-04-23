using System;
using System.Collections.Generic;
using UnityEngine;
using MapsExt.MapObjects;

namespace MapsExt
{
	public abstract class MapObjectManager : MonoBehaviour
	{
		public static MapObjectManager Current { get; set; }

		private readonly Dictionary<Type, IMapObjectSerializer> _serializers = new();
		private readonly Dictionary<Type, IMapObject> _mapObjects = new();

		public virtual void RegisterMapObject(Type dataType, IMapObject mapObject, IMapObjectSerializer serializer)
		{
			if (mapObject.Prefab == null)
			{
				throw new ArgumentException("Prefab cannot be null");
			}

			if (this._mapObjects.ContainsKey(dataType))
			{
				throw new ArgumentException($"{dataType.Name} is already registered");
			}

			this._mapObjects[dataType] = mapObject;
			this._serializers[dataType] = serializer;
		}

		protected IMapObject GetMapObject(Type dataType)
		{
			return this._mapObjects[dataType];
		}

		protected IMapObjectSerializer GetSerializer(Type dataType)
		{
			return this._serializers.GetValueOrDefault(dataType) ?? throw new ArgumentException($"Map object type not registered: {dataType}");
		}

		public void WriteMapObject(MapObjectData data, GameObject target)
		{
			this.GetSerializer(data.GetType()).WriteMapObject(data, target);
		}

		public MapObjectData ReadMapObject(GameObject go)
		{
			return this.ReadMapObject(go.GetComponent<MapObjectInstance>());
		}

		public MapObjectData ReadMapObject(MapObjectInstance mapObjectInstance)
		{
			if (mapObjectInstance == null)
			{
				throw new ArgumentNullException(nameof(mapObjectInstance));
			}

			if (mapObjectInstance.DataType == null)
			{
				throw new ArgumentException($"Cannot read map object ({mapObjectInstance.gameObject.name}): null DataType");
			}

			var dataType = mapObjectInstance.DataType;
			var serializer = this.GetSerializer(dataType);
			return serializer.ReadMapObject(mapObjectInstance);
		}

		public void Instantiate<TData>(Transform parent, Action<GameObject> onInstantiate = null) where TData : MapObjectData
		{
			this.Instantiate(typeof(TData), parent, onInstantiate);
		}

		public void Instantiate(Type dataType, Transform parent, Action<GameObject> onInstantiate = null)
		{
			var data = (MapObjectData) Activator.CreateInstance(dataType);
			this.Instantiate(data, parent, onInstantiate);
		}

		public abstract void Instantiate(MapObjectData data, Transform parent, Action<GameObject> onInstantiate = null);
	}
}

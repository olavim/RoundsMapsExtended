using System;
using UnityEngine;
using MapsExt.MapObjects;

namespace MapsExt.Editor
{
	public class EditorMapObjectManager : MapObjectManager<IEditorMapObjectSerializer>
	{
		public MapObjectData Serialize(GameObject go)
		{
			var mapObjectInstance = go.GetComponent<MapObjectInstance>() ?? throw new ArgumentException("Cannot serialize GameObject: missing MapObjectInstance");
			return this.Serialize(mapObjectInstance);
		}

		public MapObjectData Serialize(MapObjectInstance mapObjectInstance)
		{
			if (mapObjectInstance == null)
			{
				throw new ArgumentException("Cannot serialize null MapObjectInstance");
			}

			if (mapObjectInstance.DataType == null)
			{
				throw new ArgumentException($"Cannot serialize MapObjectInstance ({mapObjectInstance.gameObject.name}): missing dataType");
			}

			var dataType = mapObjectInstance.DataType;
			var serializer = this.GetSerializer(dataType) ?? throw new ArgumentException($"Map object type not registered: {dataType}");
			return serializer.Serialize(mapObjectInstance);
		}

		protected override string GetInstanceName(Type dataType)
		{
			return dataType.Name;
		}

		public override void Instantiate(MapObjectData data, Transform parent, Action<GameObject> onInstantiate = null)
		{
			var mapObject = this.GetMapObject(data.GetType());
			var prefab = mapObject.Prefab;
			var instance = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, parent);

			if (instance.GetComponent<PhotonMapObject>())
			{
				GameObject.Destroy(instance.GetComponent<PhotonMapObject>());
			}

			instance.name = this.GetInstanceName(data.GetType());

			mapObject.OnInstantiate(instance);
			this.Deserialize(data, instance);

			onInstantiate?.Invoke(instance);
		}
	}
}

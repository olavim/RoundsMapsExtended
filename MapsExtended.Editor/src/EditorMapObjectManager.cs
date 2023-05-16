using System;
using UnityEngine;
using MapsExt.MapObjects;
using MapsExt.Editor.MapObjects;
using System.Linq;

namespace MapsExt.Editor
{
	public sealed class EditorMapObjectManager : MapObjectManager
	{
		public override void Instantiate(MapObjectData data, Transform parent, Action<GameObject> onInstantiate = null)
		{
			var mapObject = this.GetMapObject(data.GetType());
			var prefab = mapObject.Prefab;
			var instance = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, parent);

			if (instance.GetComponent<PhotonMapObject>())
			{
				GameObject.Destroy(instance.GetComponent<PhotonMapObject>());
			}

			instance.name = data.GetType().Name;

			mapObject.OnInstantiate(instance);
			this.WriteMapObject(data, instance);

			if (instance.GetComponentsInChildren<MapObjectPart>().Count() == 0)
			{
				instance.AddComponent<MapObjectPart>();
			}

			onInstantiate?.Invoke(instance);
		}
	}
}

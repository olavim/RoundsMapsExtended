using MapsExt.MapObjects;
using System;
using UnityEngine;

namespace MapsExt.Editor
{
	public static class MapObjectSpawner
	{
		public static void SpawnObject<T>(GameObject container, Action<GameObject> cb = null) where T : MapObjectData
		{
			SpawnObject(container, typeof(T), cb);
		}

		public static void SpawnObject(GameObject container, Type dataType, Action<GameObject> cb = null)
		{
			var mapObject = (MapObjectData) Activator.CreateInstance(dataType);
			SpawnObject(container, mapObject, cb);
		}

		public static void SpawnObject(GameObject container, MapObjectData data, Action<GameObject> cb = null)
		{
			try
			{
				MapsExtendedEditor.instance.MapObjectManager.Instantiate(data, container.transform, cb);
			}
			catch (Exception ex)
			{
				throw new Exception($"Could not spawn map object {data.GetType()}", ex);
			}
		}
	}
}

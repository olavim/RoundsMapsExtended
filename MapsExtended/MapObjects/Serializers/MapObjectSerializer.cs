using System;
using UnityEngine;
using UnboundLib;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;

namespace MapsExt.MapObjects
{
	[Serializable]
	public class MapObject
	{
		public string mapObjectId = Guid.NewGuid().ToString();
		public bool active = true;

		public MapObjectInstance FindInstance(GameObject container)
		{
			return container
				.GetComponentsInChildren<MapObjectInstance>(true)
				.FirstOrDefault(obj => obj.mapObjectId == this.mapObjectId);
		}

		public override string ToString()
		{
			return $"MapObject ({this.GetType()})\nid: {this.mapObjectId}";
		}
	}

	public delegate void SerializerAction<T>(GameObject instance, T target) where T : MapObject;
	public delegate void DeserializerAction<T>(T data, GameObject target) where T : MapObject;

	public static class BaseMapObjectSerializer
	{
		public static void Serialize(GameObject instance, MapObject target)
		{
			var mapObjectInstance = instance.GetComponent<MapObjectInstance>();
			target.mapObjectId = mapObjectInstance.mapObjectId;
			target.active = instance.activeSelf;
		}

		public static void Deserialize(MapObject data, GameObject target)
		{
			var mapObjectInstance = target.GetOrAddComponent<MapObjectInstance>();
			mapObjectInstance.mapObjectId = data.mapObjectId ?? Guid.NewGuid().ToString();
			mapObjectInstance.dataType = data.GetType();
			target.SetActive(data.active);
		}
	}

	public class MapObjectInstance : MonoBehaviour
	{
		public string mapObjectId;
		public Type dataType;

		public void Start()
		{
			this.FixShadow();
		}

		public void FixShadow()
		{
			var collider = this.gameObject.GetComponent<Collider2D>();
			var sf = this.gameObject.GetComponent<SFPolygon>();

			if (!collider || !sf)
			{
				return;
			}

			sf.opacity = 0.5f;

			if (collider is PolygonCollider2D || collider is BoxCollider2D)
			{
				sf.CopyFromCollider(collider);
			}

			if (collider is CircleCollider2D circleCollider)
			{
				int numVertices = 24;
				float anglePerVertex = 360f / numVertices;
				float radius = circleCollider.radius;

				var identity = new Vector3(0, radius, 0);
				var vertices = new List<Vector2>();

				for (int i = 0; i < numVertices; i++)
				{
					var rotation = Quaternion.Euler(0, 0, i * anglePerVertex);
					var point = rotation * identity;
					vertices.Add(new Vector2(point.x, point.y));
				}

				sf.SetPath(0, vertices.ToArray());
			}
		}
	}
}

using System;
using UnityEngine;
using UnboundLib;
using System.Collections.Generic;

namespace MapsExt.MapObjects
{
	public class MapObject
	{
		public bool active = true;
	}

	public interface IMapObjectSpecification
	{
		GameObject Prefab { get; }

		void Deserialize(MapObject data, GameObject target);

		void Serialize(GameObject instance, MapObject target);
	}

	public abstract class MapObjectSpecification<T> : IMapObjectSpecification
		where T : MapObject
	{
		public void Deserialize(MapObject data, GameObject target)
		{
			var c = target.GetOrAddComponent<MapObjectInstance>();
			c.dataType = data.GetType();
			target.SetActive(data.active);

			this.Deserialize((T) data, target);
		}

		public void Serialize(GameObject instance, MapObject target)
		{
			target.active = instance.activeSelf;
			this.Serialize(instance, (T) target);
		}

		public abstract GameObject Prefab { get; }

		protected abstract void Deserialize(T data, GameObject target);

		protected abstract void Serialize(GameObject instance, T target);
	}

	public class MapObjectInstance : MonoBehaviour
	{
		public Type dataType;

		public void Start()
		{
			this.FixShadow();
		}

		private void FixShadow()
		{
			var collider = this.gameObject.GetComponent<Collider2D>();

			if (collider == null)
			{
				return;
			}

			var sf = this.gameObject.GetOrAddComponent<SFPolygon>();
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

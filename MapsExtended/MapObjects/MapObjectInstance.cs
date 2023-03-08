using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapsExt.MapObjects
{
	public class MapObjectInstance : MonoBehaviour
	{
		public string mapObjectId;
		public Type dataType;

		protected virtual void Start()
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

using System;
using System.Collections.Generic;
using UnboundLib;
using UnityEngine;

namespace MapsExt.MapObjects
{
	public sealed class MapObjectInstance : MonoBehaviour
	{
		public string MapObjectId { get; set; }
		public Type DataType { get; set; }

		private void Start()
		{
			this.ReplaceCircleCollider();
			this.FixShadow();
			this.FixRendering();
		}

		private void ReplaceCircleCollider()
		{
			var circleCollider = this.gameObject.GetComponent<CircleCollider2D>();
			if (circleCollider)
			{
				var radius = new Vector2(circleCollider.radius, circleCollider.radius);
				GameObject.DestroyImmediate(circleCollider);
				var ellipseCollider = this.gameObject.GetOrAddComponent<EllipseCollider2D>();
				ellipseCollider.Radius = radius;
			}
		}

		private void FixShadow()
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
				const int numVertices = 24;
				const float anglePerVertex = 360f / numVertices;

				float radius = circleCollider.radius;

				var identity = new Vector3(0, radius, 0);
				var vertices = new List<Vector2>();

				for (int i = 0; i < numVertices; i++)
				{
					var rotation = Quaternion.Euler(0, 0, i * anglePerVertex);
					var point = rotation * identity;
					vertices.Add(new(point.x, point.y));
				}

				sf.SetPath(0, vertices.ToArray());
			}
		}

		private void FixRendering()
		{
			var renderer = this.GetComponent<SpriteRenderer>();
			if (renderer && renderer.color.a >= 0.5f)
			{
				renderer.transform.position = new Vector3(renderer.transform.position.x, renderer.transform.position.y, -3f);
				if (renderer.gameObject.tag != "NoMask")
				{
					renderer.color = new Color(0.21568628f, 0.21568628f, 0.21568628f);
					if (!renderer.GetComponent<SpriteMask>())
					{
						renderer.gameObject.AddComponent<SpriteMask>().sprite = renderer.sprite;
					}
				}
			}
		}
	}
}

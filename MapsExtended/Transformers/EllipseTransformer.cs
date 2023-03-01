using UnityEngine;
using System.Collections.Generic;
using UnboundLib;

namespace MapsExt.Transformers
{
	// Replaces a game object's CircleCollider2D with a PolygonCollider2D
	public class EllipseTransformer : MonoBehaviour
	{
		private void Start()
		{
			var circleCollider = this.gameObject.GetComponent<CircleCollider2D>();

			if (circleCollider)
			{
				var polygonCollider = this.gameObject.GetOrAddComponent<PolygonCollider2D>();

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

				polygonCollider.SetPath(0, vertices.ToArray());

				GameObject.Destroy(circleCollider);
			}
		}
	}
}

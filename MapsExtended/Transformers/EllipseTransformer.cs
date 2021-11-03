using UnityEngine;
using System.Collections.Generic;
using UnboundLib;

namespace MapsExt.Transformers
{
	public class EllipseTransformer : MonoBehaviour
	{
		private CircleCollider2D circleCollider;
		private PolygonCollider2D polygonCollider;

		public void Start()
		{
			this.circleCollider = this.gameObject.GetComponent<CircleCollider2D>();

			if (this.circleCollider)
			{
				this.polygonCollider = this.gameObject.GetOrAddComponent<PolygonCollider2D>();

				int numVertices = 24;
				float anglePerVertex = 360f / numVertices;
				float radius = this.circleCollider.radius;

				var identity = new Vector3(0, radius, 0);
				var vertices = new List<Vector2>();

				for (int i = 0; i < numVertices; i++)
				{
					var rotation = Quaternion.Euler(0, 0, i * anglePerVertex);
					var point = rotation * identity;
					vertices.Add(new Vector2(point.x, point.y));
				}

				this.polygonCollider.SetPath(0, vertices.ToArray());
			}
		}

		public void Update()
		{
			if (this.circleCollider?.enabled == false && this.transform.localScale.x == this.transform.localScale.y)
			{
				this.circleCollider.enabled = true;
				this.polygonCollider.enabled = false;
			}

			if (this.circleCollider?.enabled == true && this.transform.localScale.x != this.transform.localScale.y)
			{
				this.circleCollider.enabled = false;
				this.polygonCollider.enabled = true;
			}
		}
	}
}

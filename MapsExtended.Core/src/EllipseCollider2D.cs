using System.Collections.Generic;
using UnityEngine;

namespace MapsExt
{
	[RequireComponent(typeof(PolygonCollider2D))]
	public class EllipseCollider2D : MonoBehaviour
	{
		[SerializeField] private Vector2 _radius = Vector2.one;
		[SerializeField] private int _smoothness = 24;

		public Vector2 Radius
		{
			get => this._radius;
			set
			{
				this._radius = value;
				this.UpdatePath();
			}
		}

		public int Smoothness
		{
			get => this._smoothness;
			set
			{
				this._smoothness = value;
				this.UpdatePath();
			}
		}

		protected void Awake()
		{
			this.UpdatePath();
		}

		protected virtual void UpdatePath()
		{
			float anglePerVertex = 360f / this._smoothness;

			var polygonCollider = this.gameObject.GetComponent<PolygonCollider2D>();
			var vertices = new List<Vector2>();

			for (int i = 0; i < this._smoothness; i++)
			{
				var rotation = Quaternion.Euler(0, 0, i * anglePerVertex);
				var point = rotation * new Vector2(0, this._radius.y);
				point.x *= this._radius.x / this._radius.y;
				vertices.Add(point);
			}

			polygonCollider.SetPath(0, vertices.ToArray());
		}

		protected void OnValidate()
		{
			this.UpdatePath();
		}
	}
}

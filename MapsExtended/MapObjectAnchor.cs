using MapsExt.MapObjects;
using System.Linq;
using UnityEngine;

namespace MapsExt
{
	public class MapObjectAnchor : MonoBehaviour
	{
		class Offset
		{
			public float left;
			public float right;
			public float top;
			public float bottom;

			public Offset()
			{
				this.left = 0;
				this.right = 0;
				this.top = 0;
				this.bottom = 0;
			}

			public Offset(float left, float right, float top, float bottom)
			{
				this.left = left;
				this.right = right;
				this.top = top;
				this.bottom = bottom;
			}

			public bool IsCentered()
			{
				return this.left == this.right && this.top == this.bottom;
			}
		}

		public GameObject target { get; private set; }
		public bool IsAttached => this.target != this.gameObject;

		private Offset offset;

		private void Awake()
		{
			this.target = this.gameObject;
			this.offset = new Offset();
		}

		private void Update()
		{
			if (this.IsAttached)
			{
				this.UpdatePosition();
			}
		}

		public void UpdatePosition()
		{
			this.transform.position = this.GetAnchoredPosition();
		}

		public Vector3 GetAnchoredPosition()
		{
			if (this.target == null)
			{
				return this.transform.position;
			}

			var pos = this.target.transform.position;
			var targetCollider = this.target.GetComponent<Collider2D>();

			if (targetCollider == null || this.offset.IsCentered())
			{
				return pos;
			}

			var bounds = this.GetIdentityBounds(this.target);

			float x = this.offset.left > this.offset.right ? bounds.min.x + this.offset.right : bounds.max.x - this.offset.left;
			float y = this.offset.bottom > this.offset.top ? bounds.min.y + this.offset.top : bounds.max.y - this.offset.bottom;

			var dir = new Vector3(x - pos.x, y - pos.y, 0);
			dir = this.target.transform.rotation * dir;
			return pos - dir;
		}

		public void UpdateAttachment()
		{
			var pos = this.transform.position;
			var colliders = Physics2D.OverlapPointAll(pos);

			var collider =
				colliders.FirstOrDefault(c => c.gameObject.GetComponent<MapObjectInstance>() != null && c.gameObject == this.target) ??
				colliders.FirstOrDefault(c => c.gameObject.GetComponent<MapObjectInstance>() != null);

			if (collider == null)
			{
				this.Detach();
			}
			else
			{
				var rotation = collider.transform.rotation;
				var identityBounds = this.GetIdentityBounds(collider.gameObject);
				var identityPos = this.RotatePointAroundPivot(pos, collider.transform.position, Quaternion.Inverse(rotation));

				float offsetLeft = identityPos.x - identityBounds.min.x;
				float offsetRight = identityBounds.max.x - identityPos.x;
				float offsetBottom = identityPos.y - identityBounds.min.y;
				float offsetTop = identityBounds.max.y - identityPos.y;
				this.offset = new Offset(offsetLeft, offsetRight, offsetTop, offsetBottom);

				this.target = collider.gameObject;
			}
		}

		private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
		{
			var dir = point - pivot;
			dir = rotation * dir;
			point = dir + pivot;
			return point;
		}

		private Bounds GetIdentityBounds(GameObject obj)
		{
			var collider = obj.GetComponent<Collider2D>();

			if (collider == null)
			{
				return new Bounds();
			}

			// CircleCollider bounds is calculated wrong, so we do it manually
			if (collider is CircleCollider2D circleCollider)
			{
				float x = 2 * circleCollider.radius * circleCollider.transform.localScale.x;
				float y = 2 * circleCollider.radius * circleCollider.transform.localScale.y;
				return new Bounds(circleCollider.transform.position, new Vector3(x, y, 1));
			}

			var rotation = obj.transform.rotation;
			obj.transform.rotation = Quaternion.identity;
			var bounds = collider.bounds;
			obj.transform.transform.rotation = rotation;

			return bounds;
		}

		public void Detach()
		{
			this.target = this.gameObject;
			this.offset = new Offset();
		}
	}
}

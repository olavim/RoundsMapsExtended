using MapsExt.MapObjects;
using System.Linq;
using UnityEngine;

namespace MapsExt
{
	public class MapObjectAnchor : MonoBehaviour
	{
		public GameObject target { get; private set; }
		public bool IsAttached => this.target != this.gameObject;

		protected Vector3 targetLocalPosition;

		private void Awake()
		{
			this.target = this.gameObject;
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

			return this.target.transform.TransformPoint(this.targetLocalPosition);
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
			else if (collider.gameObject != this.target)
			{
				this.target = collider.gameObject;
				this.targetLocalPosition = this.target.transform.InverseTransformPoint(pos);
			}
		}

		public void Detach()
		{
			this.target = this.gameObject;
			this.targetLocalPosition = Vector3.zero;
		}
	}
}

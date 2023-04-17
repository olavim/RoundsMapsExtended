using MapsExt.MapObjects;
using System;
using UnityEngine;

namespace MapsExt
{
	public class MapObjectAnchor : MonoBehaviour
	{
		public GameObject Target { get; private set; }
		public bool IsAttached => this.Target != this.gameObject;

		private Vector2 _targetLocalPosition;

		protected virtual void Awake()
		{
			this.Target = this.gameObject;
		}

		protected virtual void Update()
		{
			if (this.IsAttached)
			{
				this.UpdatePosition();
			}
		}

		private void UpdatePosition()
		{
			this.transform.position = this.GetAnchoredPosition();
		}

		public Vector2 GetAnchoredPosition()
		{
			if (this.Target == null)
			{
				return this.transform.position;
			}

			return this.Target.transform.TransformPoint(this._targetLocalPosition);
		}

		public void UpdateAttachment()
		{
			var pos = this.transform.position;
			var colliders = Physics2D.OverlapPointAll(pos);

			var collider =
				Array.Find(colliders, c => c.gameObject.GetComponent<MapObjectInstance>() != null && c.gameObject == this.Target) ??
				Array.Find(colliders, c => c.gameObject.GetComponent<MapObjectInstance>() != null);

			if (collider == null)
			{
				this.Detach();
			}
			else if (collider.gameObject != this.Target)
			{
				this.Target = collider.gameObject;
				this._targetLocalPosition = this.Target.transform.InverseTransformPoint(pos);
			}
		}

		public void Detach()
		{
			this.Target = this.gameObject;
			this._targetLocalPosition = Vector2.zero;
		}
	}
}

using UnityEngine;

namespace MapsExt.Editor.UI
{
	public class UIAligner : MonoBehaviour
	{
		public GameObject ReferenceGameObject { get; set; }
		public float Padding { get; set; } = 0.2f;
		public Direction2D Position { get; set; }

		protected virtual void Start()
		{
			this.UpdatePosition();
		}

		protected virtual void Update()
		{
			this.UpdatePosition();
		}

		public void UpdatePosition()
		{
			if (!this.ReferenceGameObject)
			{
				return;
			}

			var refPos = this.ReferenceGameObject.transform.position;
			var refScale = this.ReferenceGameObject.transform.localScale;
			var refRotation = this.ReferenceGameObject.transform.rotation;

			float ratio = MainCam.instance.cam.orthographicSize / 20f;
			var borderPos = this.Position == Direction2D.Middle
				? (Vector2) refScale * 0.5f
				: this.Position * (refScale * 0.5f);
			var paddingPos = this.Position == Direction2D.Middle
				? new Vector2(this.Padding, this.Padding) * ratio
				: this.Position * (new Vector2(this.Padding, this.Padding) * ratio);

			this.transform.position = refPos + (refRotation * (borderPos + paddingPos));
			this.transform.rotation = refRotation;
		}
	}
}

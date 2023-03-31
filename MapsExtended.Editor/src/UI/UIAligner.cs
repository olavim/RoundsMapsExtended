using UnityEngine;

namespace MapsExt.Editor.UI
{
	public class UIAligner : MonoBehaviour
	{
		public GameObject ReferenceGameObject { get; set; }
		public float Padding { get; set; } = 0.2f;
		public int Position { get; set; }

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
			var dirMulti = AnchorPosition.directionMultipliers[this.Position];
			var borderPos = refScale * dirMulti * 0.5f;
			var paddingPos = new Vector3(this.Padding, this.Padding, 0) * dirMulti * ratio;

			this.transform.position = refPos + (refRotation * (borderPos + paddingPos));
			this.transform.rotation = refRotation;
		}
	}
}

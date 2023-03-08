using UnityEngine;

namespace MapsExt.Editor.UI
{
	class UIAligner : MonoBehaviour
	{
		public GameObject referenceGameObject;
		public float padding = 0.2f;
		public int position;

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
			if (!this.referenceGameObject)
			{
				return;
			}

			var refPos = this.referenceGameObject.transform.position;
			var refScale = this.referenceGameObject.transform.localScale;
			var refRotation = this.referenceGameObject.transform.rotation;

			float ratio = MainCam.instance.cam.orthographicSize / 20f;
			var dirMulti = AnchorPosition.directionMultipliers[this.position];
			var borderPos = refScale * dirMulti * 0.5f;
			var paddingPos = new Vector3(this.padding, this.padding, 0) * dirMulti * ratio;

			this.transform.position = refPos + (refRotation * (borderPos + paddingPos));
			this.transform.rotation = refRotation;
		}
	}
}

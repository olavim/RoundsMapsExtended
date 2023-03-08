using UnityEngine;

namespace MapsExt.Editor.UI
{
	class UIScaler : MonoBehaviour
	{
		public GameObject referenceGameObject;
		public float padding;
		public Vector2 constantSize = Vector2.zero;

		protected virtual void Awake()
		{
			this.referenceGameObject = null;
		}

		protected virtual void Start()
		{
			var rt = this.gameObject.GetComponent<RectTransform>();
			rt.sizeDelta = Vector2.zero;
		}

		protected virtual void Update()
		{
			if (!this.referenceGameObject)
			{
				return;
			}

			var scale = this.referenceGameObject.transform.localScale;
			var refSize = this.constantSize == Vector2.zero ? (Vector2) scale : this.constantSize;

			float zoomRatio = MainCam.instance.cam.orthographicSize / 20f;
			var zoomedScale = new Vector2(refSize.x / scale.x, refSize.y / scale.y) * zoomRatio;
			var zoomedPadding = new Vector2(this.padding / scale.x, this.padding / scale.y) * zoomRatio * 2f;
			this.transform.localScale = zoomedScale + zoomedPadding;

			this.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
		}
	}
}

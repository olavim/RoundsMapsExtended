using UnityEngine;

namespace MapsExt.Editor.UI
{
	public class UIScaler : MonoBehaviour
	{
		public GameObject ReferenceGameObject { get; set; }
		public float Padding { get; set; }
		public Vector2 ConstantSize { get; set; }

		protected virtual void Awake()
		{
			this.ReferenceGameObject = null;
		}

		protected virtual void Start()
		{
			var rt = this.gameObject.GetComponent<RectTransform>();
			rt.sizeDelta = Vector2.zero;
		}

		protected virtual void Update()
		{
			if (!this.ReferenceGameObject)
			{
				return;
			}

			var scale = this.ReferenceGameObject.transform.localScale;
			var refSize = this.ConstantSize == Vector2.zero ? (Vector2) scale : this.ConstantSize;

			float zoomRatio = MainCam.instance.cam.orthographicSize / 20f;
			var zoomedScale = new Vector2(refSize.x / scale.x, refSize.y / scale.y) * zoomRatio;
			var zoomedPadding = new Vector2(this.Padding / scale.x, this.Padding / scale.y) * zoomRatio * 2f;
			this.transform.localScale = zoomedScale + zoomedPadding;

			this.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
		}
	}
}

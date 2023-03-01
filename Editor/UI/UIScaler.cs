using UnityEngine;

namespace MapsExt.Editor.UI
{
	class UIScaler : MonoBehaviour
	{
		public GameObject referenceGameObject;
		public float padding;
		public Vector2 constantSize = Vector2.zero;

		private void Awake()
		{
			this.referenceGameObject = null;
			this.padding = 16f;
		}

		private void Start()
		{
			var rt = this.gameObject.GetComponent<RectTransform>() ?? this.gameObject.AddComponent<RectTransform>();
			rt.sizeDelta = Vector2.zero;
		}

		private void Update()
		{
			if (!this.referenceGameObject)
			{
				return;
			}

			var scale = this.referenceGameObject.transform.localScale;
			var rt = this.gameObject.GetComponent<RectTransform>();

			if (this.constantSize != Vector2.zero)
			{
				float ratio = MainCam.instance.cam.orthographicSize / 20f;
				this.transform.localScale = new Vector2(this.constantSize.x / scale.x, this.constantSize.y / scale.y) * ratio;
				rt.sizeDelta = new Vector2(1, 1);
				return;
			}

			var pos = this.referenceGameObject.transform.position;
			var bounds = UIUtils.WorldToScreenRect(new Rect(pos.x - (scale.x / 2f), pos.y - (scale.y / 2f), scale.x, scale.y));
			bounds.x -= this.padding;
			bounds.y -= this.padding;
			bounds.width += 2 * this.padding;
			bounds.height += 2 * this.padding;

			rt.sizeDelta = bounds.size;
			rt.anchoredPosition = bounds.center;
			rt.rotation = this.referenceGameObject.transform.rotation;
		}
	}
}

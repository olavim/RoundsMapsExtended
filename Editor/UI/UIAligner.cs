using UnityEngine;

namespace MapsExt.Editor.UI
{
	class UIAligner : MonoBehaviour
	{
		public GameObject referenceGameObject;
		public float padding;
		public int position;

		private Rect prevReferenceBounds;

		public void Awake()
		{
			this.padding = 16f;
		}

		public void Start()
		{
			if (!this.gameObject.GetComponent<RectTransform>())
			{
				this.gameObject.AddComponent<RectTransform>();
			}

			this.UpdatePosition();
		}

		public void Update()
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

			var refBounds = UIUtils.WorldToScreenRect(new Rect(refPos.x - (refScale.x / 2f), refPos.y - (refScale.y / 2f), refScale.x, refScale.y));
			refBounds.x -= this.padding;
			refBounds.y -= this.padding;
			refBounds.width += 2 * this.padding;
			refBounds.height += 2 * this.padding;

			if (refBounds != this.prevReferenceBounds)
			{
				var directionMulti = AnchorPosition.directionMultipliers[this.position] * 0.5f;

				var rt = this.gameObject.GetComponent<RectTransform>();
				rt.anchoredPosition = new Vector2(
					refBounds.center.x + (refBounds.width * directionMulti.x),
					refBounds.center.y + (refBounds.height * directionMulti.y)
				);
				rt.rotation = Quaternion.identity;
			}

			var rotationDelta = this.referenceGameObject.transform.rotation.eulerAngles.z - this.transform.rotation.eulerAngles.z;

			if (rotationDelta != 0)
			{
				this.transform.RotateAround(refBounds.center, Vector3.forward, rotationDelta);
			}

			this.prevReferenceBounds = refBounds;
		}
	}
}

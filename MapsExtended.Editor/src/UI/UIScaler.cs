using UnityEngine;

namespace MapsExt.Editor.UI
{
	[RequireComponent(typeof(RectTransform))]
	public class UIScaler : MonoBehaviour
	{
		public GameObject ReferenceGameObject { get; set; }
		public float Padding { get; set; }
		public Vector2 ConstantScale { get; set; } = Vector2.zero;

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

			var refScale = (Vector2) this.ReferenceGameObject.transform.localScale;
			float ratio = MainCam.instance.cam.orthographicSize / 20f;

			this.transform.localScale = this.ConstantScale == Vector2.zero
				? Vector3.one
				: new Vector2(this.ConstantScale.x / refScale.x, this.ConstantScale.y / refScale.y) * ratio;

			this.transform.localScale += new Vector3(this.Padding / refScale.x, this.Padding / refScale.y) * 2f * ratio;

			this.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
		}
	}
}

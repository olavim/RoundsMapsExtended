using MapsExt.Editor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.ActionHandlers
{
	public class SelectionHandler : ActionHandler
	{
		private bool _isSelected;

		public GameObject Content { get; private set; }

		public Bounds GetBounds()
		{
			return this.GetComponent<Collider2D>().bounds;
		}

		public override void OnSelect()
		{
			this.Content = new GameObject("SelectionHandler Content");
			this.Content.transform.SetParent(this.transform);
			this.Content.transform.localScale = Vector3.one;
			this.Content.transform.localPosition = Vector3.zero;
			this.Content.transform.localRotation = Quaternion.identity;
			this.Content.layer = MapsExtendedEditor.LAYER_MAPOBJECT_UI;

			var canvas = this.Content.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.WorldSpace;
			canvas.worldCamera = MainCam.instance.cam;

			var scaler = this.Content.AddComponent<UIScaler>();
			scaler.referenceGameObject = this.gameObject;
			scaler.padding = 0.6f;

			var image = this.Content.AddComponent<Image>();
			image.color = new Color32(255, 255, 255, 5);
			image.raycastTarget = false;

			this._isSelected = true;
		}

		public override void OnDeselect()
		{
			GameObjectUtils.DestroyImmediateSafe(this.Content);
			this._isSelected = false;
		}

		public override void OnPointerDown()
		{
			if (this._isSelected)
			{
				this.Content.GetComponent<Image>().color = new Color32(255, 255, 255, 7);
			}
		}

		public override void OnPointerUp()
		{
			if (this._isSelected)
			{
				this.Content.GetComponent<Image>().color = new Color32(255, 255, 255, 5);
			}
		}
	}
}

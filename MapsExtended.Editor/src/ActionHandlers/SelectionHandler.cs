using MapsExt.Editor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.ActionHandlers
{
	public class SelectionHandler : MapObjectActionHandler
	{
		private bool isSelected;

		public GameObject content;

		public Bounds GetBounds()
		{
			return this.GetComponent<Collider2D>().bounds;
		}

		public override void OnSelect()
		{
			this.content = new GameObject("SelectionHandler Content");
			this.content.transform.SetParent(this.transform);
			this.content.transform.localScale = Vector3.one;
			this.content.transform.localPosition = Vector3.zero;
			this.content.transform.localRotation = Quaternion.identity;
			this.content.layer = MapsExtendedEditor.LAYER_MAPOBJECT_UI;

			var canvas = this.content.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.WorldSpace;
			canvas.worldCamera = MainCam.instance.cam;

			var scaler = this.content.AddComponent<UIScaler>();
			scaler.referenceGameObject = this.gameObject;
			scaler.padding = 0.6f;

			var image = this.content.AddComponent<Image>();
			image.color = new Color32(255, 255, 255, 5);
			image.raycastTarget = false;

			this.isSelected = true;
		}

		public override void OnDeselect()
		{
			GameObjectUtils.DestroyImmediateSafe(this.content);
			this.isSelected = false;
		}

		public override void OnPointerDown()
		{
			if (this.isSelected)
			{
				this.content.GetComponent<Image>().color = new Color32(255, 255, 255, 7);
			}
		}

		public override void OnPointerUp()
		{
			if (this.isSelected)
			{
				this.content.GetComponent<Image>().color = new Color32(255, 255, 255, 5);
			}
		}
	}
}

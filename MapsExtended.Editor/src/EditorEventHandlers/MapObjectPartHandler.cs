using MapsExt.Editor.MapObjects;
using MapsExt.Editor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.Events
{
	public class MapObjectPartHandler : EditorEventHandler
	{
		private bool _isSelected;

		public GameObject Content { get; private set; }

		protected override void Awake()
		{
			base.Awake();

			if (this.GetComponent<MapObjectPart>() == null)
			{
				throw new System.Exception($"No {nameof(MapObjectPart)} found on {this.gameObject.name}");
			}
		}

		protected override void HandleEvent(IEditorEvent evt)
		{
			switch (evt)
			{
				case SelectEvent:
					this.OnSelect();
					break;
				case DeselectEvent:
					this.OnDeselect();
					break;
				case PointerDownEvent:
					this.OnPointerDown();
					break;
				case PointerUpEvent:
					this.OnPointerUp();
					break;
			}
		}

		protected virtual void OnSelect()
		{
			this.Content = new GameObject("SelectionHandler Content");
			this.Content.transform.SetParent(this.transform);
			this.Content.transform.localScale = Vector3.one;
			this.Content.transform.localPosition = Vector3.zero;
			this.Content.transform.localRotation = Quaternion.identity;
			this.Content.layer = MapsExtendedEditor.MapObjectUILayer;

			var canvas = this.Content.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.WorldSpace;
			canvas.worldCamera = MainCam.instance.cam;

			var scaler = this.Content.AddComponent<UIScaler>();
			scaler.ReferenceGameObject = this.gameObject;
			scaler.Padding = this.Editor.ActiveMapObjectPart == this.gameObject ? 0.6f : 0.2f;

			var image = this.Content.AddComponent<Image>();
			image.color = new Color32(255, 255, 255, 5);
			image.raycastTarget = false;

			this._isSelected = true;
		}

		protected virtual void OnDeselect()
		{
			if (this._isSelected)
			{
				GameObjectUtils.DestroyImmediateSafe(this.Content);
				this._isSelected = false;
			}
		}

		protected virtual void OnPointerDown()
		{
			if (this._isSelected)
			{
				this.Content.GetComponent<Image>().color = new Color32(255, 255, 255, 7);
			}
		}

		protected virtual void OnPointerUp()
		{
			if (this._isSelected)
			{
				this.Content.GetComponent<Image>().color = new Color32(255, 255, 255, 5);
			}
		}
	}
}

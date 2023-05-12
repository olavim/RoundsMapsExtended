using MapsExt.Editor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.Events
{
	public class SelectionHandler : EditorEventHandler
	{
		private bool _isSelected;

		public GameObject Content { get; private set; }

		protected virtual void Start()
		{
			var colliders = this.GetComponentsInChildren<Collider2D>();
			if (colliders.Length == 0)
			{
				throw new System.Exception($"No colliders found on {this.gameObject.name}");
			}
		}

		public Bounds GetBounds()
		{
			if (this.GetComponent<Collider2D>())
			{
				return this.GetComponent<Collider2D>().bounds;
			}

			var colliders = this.GetComponentsInChildren<Collider2D>();
			var bounds = colliders[0].bounds;
			for (var i = 1; i < colliders.Length; i++)
			{
				bounds.Encapsulate(colliders[i].bounds);
			}

			return bounds;
		}

		protected override bool ShouldHandleEvent(IEditorEvent evt)
		{
			return this.Editor.SelectedObjects.Contains(this.gameObject);
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
			scaler.Padding = 0.6f;

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

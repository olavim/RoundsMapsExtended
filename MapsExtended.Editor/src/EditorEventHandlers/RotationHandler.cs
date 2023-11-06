using MapsExt.Editor.Utils;
using MapsExt.Properties;
using MapsExt.Utils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace MapsExt.Editor.Events
{
	public class RotationHandler : EditorEventHandler
	{
		public GameObject Content { get; private set; }

		private bool _isRotating;
		private RotationProperty _prevRotation;

		protected override void Awake()
		{
			base.Awake();

			this.Content = new GameObject("Rotate Interaction Content");
			this.Content.transform.SetParent(this.transform);
			this.Content.transform.localScale = Vector3.one;
			this.Content.layer = MapsExtendedEditor.MapObjectUILayer;

			this.Content.AddComponent<GraphicRaycaster>();
			var canvas = this.Content.GetComponent<Canvas>();
			canvas.renderMode = RenderMode.WorldSpace;
			canvas.worldCamera = MainCam.instance.cam;
		}

		protected virtual void Update()
		{
			if (this._isRotating)
			{
				this.RotateMapObjects();
			}
		}

		public virtual void SetValue(RotationProperty rotation)
		{
			this.GetComponent<RotationPropertyInstance>().Rotation = rotation;
			this.transform.rotation = (Quaternion) rotation;
		}

		public virtual RotationProperty GetValue()
		{
			return this.GetComponent<RotationPropertyInstance>().Rotation;
		}

		private void OnRotateStart()
		{
			this._isRotating = true;
			this._prevRotation = this.GetValue();
		}

		private void OnRotateEnd()
		{
			this._isRotating = false;

			if (this.GetValue() != this._prevRotation)
			{
				this.Editor.TakeSnaphot();
			}
		}

		private void RotateMapObjects()
		{
			var mousePos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(EditorInput.MousePosition.x, EditorInput.MousePosition.y));
			var objectPos = this.transform.position;
			mousePos.x -= objectPos.x;
			mousePos.y -= objectPos.y;

			float angle = (Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg) - 90;
			angle = EditorUtils.Snap(angle, this.Editor.SnapToGrid ? 15f : 2f);
			if (angle < 0)
			{
				angle += 360;
			}
			this.SetValue(angle);
		}

		private void AddRotationHandle()
		{
			var go = new GameObject("Rotation Handle");

			var image = go.AddComponent<ProceduralImage>();
			image.FalloffDistance = 0.005f;

			var aligner = go.AddComponent<UI.UIAligner>();
			aligner.ReferenceGameObject = this.gameObject;
			aligner.Position = Direction2D.North;
			aligner.Padding = 1.6f;

			var scaler = go.AddComponent<UI.UIScaler>();
			scaler.ReferenceGameObject = this.gameObject;
			scaler.ConstantScale = new Vector2(0.5f, 0.5f);

			var modifier = go.AddComponent<UniformModifier>();
			modifier.Radius = 0.5f;

			var button = go.AddComponent<Button>();
			button.colors = new ColorBlock()
			{
				colorMultiplier = 1,
				fadeDuration = 0.1f,
				normalColor = new Color(1, 1, 1),
				highlightedColor = new Color(0.8f, 0.8f, 0.8f),
				pressedColor = new Color(0.6f, 0.6f, 0.6f)
			};

			var events = go.AddComponent<PointerDownHandler>();

			events.PointerDown += _ =>
			{
				if (!this._isRotating)
				{
					this.OnRotateStart();
				}
			};

			events.PointerUp += _ =>
			{
				if (this._isRotating)
				{
					this.OnRotateEnd();
				}
			};

			go.transform.SetParent(this.Content.transform);
			go.transform.localScale = Vector3.one;
		}

		protected override bool ShouldHandleEvent(IEditorEvent evt)
		{
			return this.Editor.ActiveMapObjectPart == this.gameObject;
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
				case PointerUpEvent:
					this.OnPointerUp();
					break;
			}
		}

		protected virtual void OnPointerUp()
		{
			if (this._isRotating)
			{
				this.OnRotateEnd();
			}
		}

		protected virtual void OnSelect()
		{
			this.AddRotationHandle();
		}

		protected virtual void OnDeselect()
		{
			GameObjectUtils.DestroyChildrenImmediateSafe(this.Content);
		}
	}
}

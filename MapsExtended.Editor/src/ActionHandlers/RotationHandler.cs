using MapsExt.Properties;
using UnboundLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace MapsExt.Editor.ActionHandlers
{
	public class RotationHandler : ActionHandler<RotationProperty>
	{
		public GameObject Content { get; private set; }

		private bool _isRotating;
		private float _prevAngle;

		protected virtual void Awake()
		{
			this.gameObject.GetOrAddComponent<SelectionHandler>();

			this.Content = new GameObject("Rotate Interaction Content");
			this.Content.transform.SetParent(this.transform);
			this.Content.transform.localScale = Vector3.one;
			this.Content.layer = MapsExtendedEditor.LAYER_MAPOBJECT_UI;

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

		public override void OnPointerUp()
		{
			if (this._isRotating)
			{
				this.OnRotateEnd();
			}
		}

		public override void SetValue(RotationProperty rotation)
		{
			this.transform.rotation = rotation;
		}

		public override RotationProperty GetValue()
		{
			return new RotationProperty(this.transform.rotation);
		}

		public override void OnSelect()
		{
			this.AddRotationHandle();
		}

		public override void OnDeselect()
		{
			GameObjectUtils.DestroyChildrenImmediateSafe(this.Content);
		}

		private void OnRotateStart()
		{
			this._isRotating = true;
			this._prevAngle = this.transform.rotation.eulerAngles.z;
		}

		private void OnRotateEnd()
		{
			this._isRotating = false;
			this.Editor.RefreshHandlers();

			if (this.transform.rotation.eulerAngles.z != this._prevAngle)
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
			var toRotation = Quaternion.AngleAxis(angle, Vector3.forward);

			this.SetValue(toRotation);
		}

		private void AddRotationHandle()
		{
			var go = new GameObject("Rotation Handle");

			var image = go.AddComponent<ProceduralImage>();
			image.FalloffDistance = 0.005f;

			var aligner = go.AddComponent<UI.UIAligner>();
			aligner.ReferenceGameObject = this.gameObject;
			aligner.Position = AnchorPosition.TopMiddle;
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
	}
}

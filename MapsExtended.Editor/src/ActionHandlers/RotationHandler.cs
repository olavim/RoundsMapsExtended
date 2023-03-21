using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace MapsExt.Editor.ActionHandlers
{
	public class RotationHandler : MapObjectActionHandler
	{
		public GameObject content;

		private bool isRotating;
		private float prevAngle;

		protected virtual void Awake()
		{
			this.content = new GameObject("Rotate Interaction Content");
			this.content.transform.SetParent(this.transform);
			this.content.transform.localScale = Vector3.one;
			this.content.layer = MapsExtendedEditor.LAYER_MAPOBJECT_UI;

			this.content.AddComponent<GraphicRaycaster>();
			var canvas = this.content.GetComponent<Canvas>();
			canvas.renderMode = RenderMode.WorldSpace;
			canvas.worldCamera = MainCam.instance.cam;
		}

		protected virtual void Update()
		{
			if (this.isRotating)
			{
				this.RotateMapObjects();
			}
		}

		public override void OnPointerUp()
		{
			if (this.isRotating)
			{
				this.OnRotateEnd();
			}
		}

		public virtual void SetRotation(Quaternion rotation)
		{
			this.transform.rotation = Quaternion.Euler(rotation.eulerAngles.Round(4));
			this.OnChange();
		}

		public virtual Quaternion GetRotation()
		{
			return this.transform.rotation;
		}

		public override void OnSelect()
		{
			this.AddRotationHandle();
		}

		public override void OnDeselect()
		{
			GameObjectUtils.DestroyChildrenImmediateSafe(this.content);
		}

		private void OnRotateStart()
		{
			this.isRotating = true;
			this.prevAngle = this.transform.rotation.eulerAngles.z;
		}

		private void OnRotateEnd()
		{
			this.isRotating = false;
			this.Editor.UpdateRopeAttachments();

			if (this.transform.rotation.eulerAngles.z != this.prevAngle)
			{
				this.Editor.TakeSnaphot();
			}
		}

		private void RotateMapObjects()
		{
			var mousePos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(EditorInput.mousePosition.x, EditorInput.mousePosition.y));
			var objectPos = this.transform.position;
			mousePos.x -= objectPos.x;
			mousePos.y -= objectPos.y;

			float angle = (Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg) - 90;
			angle = EditorUtils.Snap(angle, this.Editor.snapToGrid ? 15f : 2f);
			var toRotation = Quaternion.AngleAxis(angle, Vector3.forward);

			this.SetRotation(toRotation);
		}

		private void AddRotationHandle()
		{
			var go = new GameObject("Rotation Handle");

			var image = go.AddComponent<ProceduralImage>();
			image.FalloffDistance = 0.005f;

			var aligner = go.AddComponent<UI.UIAligner>();
			aligner.referenceGameObject = this.gameObject;
			aligner.position = AnchorPosition.TopMiddle;
			aligner.padding = 1.6f;

			var scaler = go.AddComponent<UI.UIScaler>();
			scaler.referenceGameObject = this.gameObject;
			scaler.constantSize = new Vector2(0.5f, 0.5f);

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

			events.pointerDown += _ =>
			{
				if (!this.isRotating)
				{
					this.OnRotateStart();
				}
			};

			events.pointerUp += _ =>
			{
				if (this.isRotating)
				{
					this.OnRotateEnd();
				}
			};

			go.transform.SetParent(this.content.transform);
			go.transform.localScale = Vector3.one;
		}
	}
}

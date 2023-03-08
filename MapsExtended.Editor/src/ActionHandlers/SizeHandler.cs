using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.ActionHandlers
{
	public class SizeHandler : MapObjectActionHandler
	{
		public GameObject content;

		private bool isResizing;
		private int resizeDirection;
		private Vector3 prevMouse;
		private Vector3Int prevCell;

		protected virtual void Awake()
		{
			this.content = new GameObject("Resize Interaction Content");
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
			if (this.isResizing)
			{
				this.ResizeMapObjects();
			}
		}

		public override void OnPointerUp()
		{
			if (this.isResizing)
			{
				this.OnResizeEnd();
			}
		}

		public virtual void Resize(Vector3 delta, int resizeDirection = 0)
		{
			this.SetSize(this.transform.localScale + delta, resizeDirection);
		}

		public virtual void SetSize(Vector3 size, int resizeDirection = 0)
		{
			var delta = size - this.transform.localScale;
			float gridSize = this.Editor.GridSize;
			bool snapToGrid = this.Editor.snapToGrid;

			var directionMulti = AnchorPosition.directionMultipliers[resizeDirection];
			var scaleMulti = AnchorPosition.sizeMultipliers[resizeDirection];
			var scaleDelta = Vector3.Scale(delta, scaleMulti);

			var currentScale = this.transform.localScale;
			var currentRotation = this.transform.rotation;

			if (snapToGrid && scaleDelta.x != 0 && currentScale.x + scaleDelta.x < gridSize)
			{
				scaleDelta.x = gridSize - currentScale.x;
			}

			if (snapToGrid && scaleDelta.y != 0 && currentScale.y + scaleDelta.y < gridSize)
			{
				scaleDelta.y = gridSize - currentScale.y;
			}

			if (scaleDelta.x != 0 && currentScale.x + scaleDelta.x < 0.1f)
			{
				scaleDelta.x = 0.1f - currentScale.x;
			}

			if (scaleDelta.y != 0 && currentScale.y + scaleDelta.y < 0.1f)
			{
				scaleDelta.y = 0.1f - currentScale.y;
			}

			var newScale = currentScale + scaleDelta;

			if (newScale == currentScale)
			{
				return;
			}

			var positionDelta = currentRotation * Vector3.Scale(scaleDelta, directionMulti);
			this.transform.localScale = newScale;

			var posHandler = this.gameObject.GetComponent<PositionHandler>();
			posHandler?.Move(positionDelta * 0.5f);
		}

		public override void OnSelect()
		{
			this.AddResizeHandle(AnchorPosition.TopLeft);
			this.AddResizeHandle(AnchorPosition.TopRight);
			this.AddResizeHandle(AnchorPosition.BottomLeft);
			this.AddResizeHandle(AnchorPosition.BottomRight);
			this.AddResizeHandle(AnchorPosition.MiddleLeft);
			this.AddResizeHandle(AnchorPosition.MiddleRight);
			this.AddResizeHandle(AnchorPosition.BottomMiddle);
			this.AddResizeHandle(AnchorPosition.TopMiddle);
		}

		public override void OnDeselect()
		{
			foreach (Transform child in this.content.transform)
			{
				GameObject.Destroy(child.gameObject);
			}
		}

		private void OnResizeStart(int resizeDirection)
		{
			var mousePos = EditorInput.mousePosition;
			var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));

			this.Editor.grid.transform.rotation = this.transform.rotation;

			this.isResizing = true;
			this.resizeDirection = resizeDirection;
			this.prevMouse = mouseWorldPos;
			this.prevCell = this.Editor.grid.WorldToCell(mouseWorldPos);
		}

		private void OnResizeEnd()
		{
			this.isResizing = false;
			this.Editor.UpdateRopeAttachments();
			this.Editor.TakeSnaphot();
		}

		private void ResizeMapObjects()
		{
			var mousePos = EditorInput.mousePosition;
			var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));
			var mouseCell = this.Editor.grid.WorldToCell(mouseWorldPos);
			var mouseDelta = mouseWorldPos - this.prevMouse;
			Vector3 cellDelta = mouseCell - this.prevCell;

			var sizeDelta = this.Editor.snapToGrid
				? cellDelta * this.Editor.GridSize
				: Quaternion.Inverse(this.Editor.grid.transform.rotation) * mouseDelta;

			if (sizeDelta != Vector3.zero)
			{
				this.Resize(sizeDelta, this.resizeDirection);
				this.OnChange();

				this.prevMouse += mouseDelta;
				this.prevCell = mouseCell;
			}
		}

		protected void AddResizeHandle(int direction)
		{
			var go = new GameObject("Resize Handle " + direction);
			go.AddComponent<Image>();

			var aligner = go.AddComponent<UI.UIAligner>();
			aligner.referenceGameObject = this.gameObject;
			aligner.position = direction;
			aligner.padding = 0.6f;

			var scaler = go.AddComponent<UI.UIScaler>();
			scaler.referenceGameObject = this.gameObject;
			scaler.constantSize = new Vector2(0.4f, 0.4f);

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
				if (!this.isResizing)
				{
					this.OnResizeStart(direction);
				}
			};

			events.pointerUp += _ =>
			{
				if (this.isResizing)
				{
					this.OnResizeEnd();
				}
			};

			go.transform.SetParent(this.content.transform);
			go.transform.localScale = Vector3.one;
		}
	}
}

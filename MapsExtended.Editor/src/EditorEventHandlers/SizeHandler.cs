using MapsExt.Properties;
using MapsExt.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.Events
{
	public class SizeHandler : EditorEventHandler
	{
		private bool _isResizing;
		private Direction2D _resizeDirection;
		private Vector2 _prevMouse;
		private Vector2 _prevScale;
		private Vector2Int _prevCell;

		public GameObject Content { get; private set; }

		protected override void Awake()
		{
			base.Awake();

			this.Content = new GameObject("Resize Interaction Content");
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
			if (this._isResizing)
			{
				this.ResizeMapObjects();
			}
		}

		public void Resize(ScaleProperty delta, Direction2D resizeDirection = null)
		{
			var newScale = this.GetValue().Value + delta.Value;
			this.SetValue(newScale, resizeDirection ?? Direction2D.Middle);
		}

		public void SetValue(ScaleProperty size)
		{
			this.SetValue(size, Direction2D.Middle);
		}

		public virtual ScaleProperty GetValue()
		{
			return new ScaleProperty(this.transform.localScale);
		}

		public virtual void SetValue(ScaleProperty size, Direction2D resizeDirection)
		{
			var delta = size.Value - (Vector2) this.transform.localScale;
			float gridSize = this.Editor.GridSize;
			bool snapToGrid = this.Editor.SnapToGrid;

			var scaleDelta = resizeDirection == Direction2D.Middle
				? delta
				: resizeDirection * delta;
			var currentScale = this.GetValue().Value;
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

			this.transform.localScale = newScale;

			var posHandler = this.GetComponent<PositionHandler>();
			if (posHandler != null)
			{
				var positionDelta = (PositionProperty) (currentRotation * (resizeDirection * scaleDelta));
				posHandler.Move(positionDelta * 0.5f);
			}
		}

		private void OnResizeStart(Direction2D resizeDirection)
		{
			var mousePos = EditorInput.MousePosition;
			var mouseWorldPos = (Vector2) MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));

			this.Editor.Grid.transform.rotation = this.transform.rotation;

			this._isResizing = true;
			this._resizeDirection = resizeDirection;
			this._prevMouse = mouseWorldPos;
			this._prevCell = (Vector2Int) this.Editor.Grid.WorldToCell(mouseWorldPos);
			this._prevScale = this.transform.localScale;
		}

		private void OnResizeEnd()
		{
			this._isResizing = false;

			if (this._prevScale != this.GetValue().Value)
			{
				this.Editor.TakeSnaphot();
			}
		}

		private void ResizeMapObjects()
		{
			var mousePos = EditorInput.MousePosition;
			var mouseWorldPos = (Vector2) MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));
			var mouseCell = (Vector2Int) this.Editor.Grid.WorldToCell(mouseWorldPos);
			var mouseDelta = mouseWorldPos - this._prevMouse;
			Vector2 cellDelta = mouseCell - this._prevCell;

			var sizeDelta = this.Editor.SnapToGrid
				? cellDelta * this.Editor.GridSize
				: (Vector2) (Quaternion.Inverse(this.Editor.Grid.transform.rotation) * mouseDelta);

			if (sizeDelta != Vector2.zero)
			{
				this.Resize(sizeDelta, this._resizeDirection);
				this._prevMouse += mouseDelta;
				this._prevCell = mouseCell;
			}
		}

		protected void AddResizeHandle(Direction2D direction)
		{
			var go = new GameObject("Resize Handle " + direction);
			go.AddComponent<Image>();

			var aligner = go.AddComponent<UI.UIAligner>();
			aligner.ReferenceGameObject = this.gameObject;
			aligner.Position = direction;
			aligner.Padding = 0.6f;

			var scaler = go.AddComponent<UI.UIScaler>();
			scaler.ReferenceGameObject = this.gameObject;
			scaler.ConstantScale = new Vector2(0.4f, 0.4f);

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
				if (!this._isResizing)
				{
					this.OnResizeStart(direction);
				}
			};

			events.PointerUp += _ =>
			{
				if (this._isResizing)
				{
					this.OnResizeEnd();
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
			if (this._isResizing)
			{
				this.OnResizeEnd();
			}
		}

		protected virtual void OnSelect()
		{
			this.AddResizeHandle(Direction2D.North);
			this.AddResizeHandle(Direction2D.South);
			this.AddResizeHandle(Direction2D.East);
			this.AddResizeHandle(Direction2D.West);
			this.AddResizeHandle(Direction2D.NorthEast);
			this.AddResizeHandle(Direction2D.NorthWest);
			this.AddResizeHandle(Direction2D.SouthEast);
			this.AddResizeHandle(Direction2D.SouthWest);
		}

		protected virtual void OnDeselect()
		{
			GameObjectUtils.DestroyChildrenImmediateSafe(this.Content);
		}
	}
}

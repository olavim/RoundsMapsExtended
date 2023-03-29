using MapsExt.MapObjects.Properties;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public class PositionHandler : ActionHandler<PositionProperty>
	{
		public static Vector2 KeyCodeToNudge(KeyCode key)
		{
			Vector2 delta;

			switch (key)
			{
				case KeyCode.RightArrow:
					delta = new Vector2(1, 0);
					break;
				case KeyCode.LeftArrow:
					delta = new Vector2(-1, 0);
					break;
				case KeyCode.UpArrow:
					delta = new Vector2(0, 1);
					break;
				case KeyCode.DownArrow:
					delta = new Vector2(0, -1);
					break;
				default:
					delta = Vector2.zero;
					break;
			}

			if (EditorInput.GetKey(KeyCode.LeftShift))
			{
				delta *= 2f;
			}

			if (EditorInput.GetKey(KeyCode.LeftControl))
			{
				delta /= 2f;
			}

			return delta;
		}

		private bool _isDragging;
		private Vector2 _prevMouse;
		private Vector2 _prevPosition;
		private Vector2Int _prevCell;
		private Vector2 _offset;

		public virtual void Move(PositionProperty delta)
		{
			this.SetValue((PositionProperty) ((Vector2) this.transform.position + delta));
		}

		public override void SetValue(PositionProperty position)
		{
			this.transform.position = position;
			this.OnChange();
		}

		public override PositionProperty GetValue()
		{
			return new PositionProperty(this.transform.position);
		}

		protected virtual void Update()
		{
			if (this._isDragging)
			{
				this.DragMapObjects();
			}
		}

		public override void OnPointerDown()
		{
			var mousePos = EditorInput.MousePosition;
			var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));

			this._prevMouse = mouseWorldPos;
			this._prevPosition = this.GetValue();
			this._isDragging = true;

			var referenceRotation = this.transform.rotation;
			var referenceAngles = referenceRotation.eulerAngles;
			referenceRotation.eulerAngles = new Vector3(referenceAngles.x, referenceAngles.y, referenceAngles.z % 90);

			this.Editor.grid.transform.rotation = referenceRotation;

			var scaleOffset = Vector2.zero;
			var objectCell = this.Editor.grid.WorldToCell(this.GetValue());
			var snappedPosition = (Vector2) this.Editor.grid.CellToWorld(objectCell);

			if (snappedPosition != this.GetValue().Value)
			{
				var diff = this.GetValue().Value - snappedPosition;
				var identityDiff = Quaternion.Inverse(referenceRotation) * diff;
				var identityDelta = new Vector2(this.Editor.GridSize / 2f, this.Editor.GridSize / 2f);

				const float eps = 0.000005f;

				if ((Mathf.Abs(identityDiff.x) < eps || Mathf.Abs(identityDiff.x - identityDelta.x) < eps) &&
					(Mathf.Abs(identityDiff.y) < eps || Mathf.Abs(identityDiff.y - identityDelta.y) < eps))
				{
					scaleOffset = diff;
				}
			}

			this._offset = scaleOffset;
			this._prevCell = (Vector2Int) this.Editor.grid.WorldToCell(mouseWorldPos);
		}

		public override void OnPointerUp()
		{
			if (this._isDragging)
			{
				bool moved = this._prevPosition != this.GetValue().Value;
				this.SetValue(this.GetValue().Value.Round(4));
				this._isDragging = false;

				if (moved)
				{
					this.Editor.TakeSnaphot();
				}
			}
		}

		public override void OnKeyDown(KeyCode key)
		{
			Vector2 nudge = PositionHandler.KeyCodeToNudge(key);

			if (this.Editor.GridSize > 0)
			{
				nudge *= this.Editor.GridSize;
			}

			if (nudge.magnitude > 0)
			{
				this.Move(nudge);
				this.Editor.TakeSnaphot();
			}
		}

		private void DragMapObjects()
		{
			var mousePos = EditorInput.MousePosition;
			var mouseWorldPos = (Vector2) MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));
			var mouseCell = (Vector2Int) this.Editor.grid.WorldToCell(mouseWorldPos);
			var mouseDelta = mouseWorldPos - this._prevMouse;
			var cellDelta = mouseCell - this._prevCell;

			var delta = mouseDelta;

			if (this.Editor.SnapToGrid)
			{
				var objectCell = (Vector2Int) this.Editor.grid.WorldToCell(this.GetValue());
				var snappedPosition = (Vector2) this.Editor.grid.CellToWorld((Vector3Int) (objectCell + cellDelta));

				delta = snappedPosition + this._offset - this.GetValue();
			}

			if (delta != Vector2.zero)
			{
				this.Move(delta);
			}

			this._prevMouse += mouseDelta;
			this._prevCell = mouseCell;
		}
	}
}

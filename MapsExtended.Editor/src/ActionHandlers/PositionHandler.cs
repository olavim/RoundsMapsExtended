using MapsExt.Properties;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public class PositionHandler : ActionHandler
	{
		private static Vector2 KeyCodeToNudge(KeyCode key)
		{
			var delta = key switch
			{
				KeyCode.RightArrow => new Vector2(1, 0),
				KeyCode.LeftArrow => new Vector2(-1, 0),
				KeyCode.UpArrow => new Vector2(0, 1),
				KeyCode.DownArrow => new Vector2(0, -1),
				_ => Vector2.zero,
			};

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

		protected virtual void Awake()
		{
			this.gameObject.GetOrAddComponent<SelectionHandler>();
		}

		public void Move(PositionProperty delta)
		{
			this.SetValue(this.GetValue() + delta);
		}

		public virtual void SetValue(PositionProperty position)
		{
			this.transform.position = position;
		}

		public virtual PositionProperty GetValue()
		{
			return this.transform.position;
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

			this.Editor.Grid.transform.rotation = referenceRotation;

			var scaleOffset = Vector2.zero;
			var objectCell = this.Editor.Grid.WorldToCell(this.GetValue());
			var snappedPosition = (Vector2) this.Editor.Grid.CellToWorld(objectCell);

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
			this._prevCell = (Vector2Int) this.Editor.Grid.WorldToCell(mouseWorldPos);
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

		public override void OnPaste()
		{
			this.Move(new(1, -1));
		}

		private void DragMapObjects()
		{
			var mousePos = EditorInput.MousePosition;
			var mouseWorldPos = (Vector2) MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));
			var mouseCell = (Vector2Int) this.Editor.Grid.WorldToCell(mouseWorldPos);
			var mouseDelta = mouseWorldPos - this._prevMouse;
			var cellDelta = mouseCell - this._prevCell;

			var delta = mouseDelta;

			if (this.Editor.SnapToGrid)
			{
				var objectCell = (Vector2Int) this.Editor.Grid.WorldToCell(this.GetValue());
				var snappedPosition = (Vector2) this.Editor.Grid.CellToWorld((Vector3Int) (objectCell + cellDelta));

				delta = snappedPosition + this._offset - (Vector2) this.GetValue();
			}

			if (delta != Vector2.zero)
			{
				this.SetValue(((Vector2) this.GetValue() + delta).Round(4));
			}

			this._prevMouse += mouseDelta;
			this._prevCell = mouseCell;
		}
	}
}

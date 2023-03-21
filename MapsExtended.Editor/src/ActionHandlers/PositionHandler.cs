using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public class PositionHandler : MapObjectActionHandler
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

		private bool isDragging;
		private Vector3 prevMouse;
		private Vector3 prevPosition;
		private Vector3Int prevCell;
		private Vector3 offset;

		public virtual void Move(Vector3 delta)
		{
			this.SetPosition(this.transform.position + delta);
		}

		public virtual void SetPosition(Vector3 position)
		{
			this.transform.position = position;
			this.OnChange();
		}

		public virtual Vector3 GetPosition()
		{
			return this.transform.position;
		}

		protected virtual void Update()
		{
			if (this.isDragging)
			{
				this.DragMapObjects();
			}
		}

		public override void OnPointerDown()
		{
			var mousePos = EditorInput.mousePosition;
			var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));

			this.prevMouse = mouseWorldPos;
			this.prevPosition = this.transform.position;
			this.isDragging = true;

			var referenceRotation = this.transform.rotation;
			var referenceAngles = referenceRotation.eulerAngles;
			referenceRotation.eulerAngles = new Vector3(referenceAngles.x, referenceAngles.y, referenceAngles.z % 90);

			this.Editor.grid.transform.rotation = referenceRotation;

			var scaleOffset = Vector3.zero;
			var objectCell = this.Editor.grid.WorldToCell(this.transform.position);
			var snappedPosition = this.Editor.grid.CellToWorld(objectCell);

			if (snappedPosition != this.transform.position)
			{
				var diff = this.transform.position - snappedPosition;
				var identityDiff = Quaternion.Inverse(referenceRotation) * diff;
				var identityDelta = new Vector3(this.Editor.GridSize / 2f, this.Editor.GridSize / 2f, 0);

				const float eps = 0.000005f;

				if ((Mathf.Abs(identityDiff.x) < eps || Mathf.Abs(identityDiff.x - identityDelta.x) < eps) &&
					(Mathf.Abs(identityDiff.y) < eps || Mathf.Abs(identityDiff.y - identityDelta.y) < eps))
				{
					scaleOffset = diff;
				}
			}

			this.offset = scaleOffset;
			this.prevCell = this.Editor.grid.WorldToCell(mouseWorldPos);
		}

		public override void OnPointerUp()
		{
			if (this.isDragging)
			{
				bool moved = this.prevPosition != this.transform.position;
				this.SetPosition(this.transform.position.Round(4));
				this.isDragging = false;

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
			var mousePos = EditorInput.mousePosition;
			var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));
			var mouseCell = this.Editor.grid.WorldToCell(mouseWorldPos);
			var mouseDelta = mouseWorldPos - this.prevMouse;
			var cellDelta = mouseCell - this.prevCell;

			var delta = mouseDelta;

			if (this.Editor.snapToGrid)
			{
				var objectCell = this.Editor.grid.WorldToCell(this.transform.position);
				var snappedPosition = this.Editor.grid.CellToWorld(objectCell + cellDelta);

				delta = snappedPosition + this.offset - this.transform.position;
			}

			if (delta != Vector3.zero)
			{
				this.Move(delta);
			}

			this.prevMouse += mouseDelta;
			this.prevCell = mouseCell;
		}
	}
}

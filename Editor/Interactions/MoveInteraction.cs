using MapsExt.Editor.ActionHandlers;
using System.Linq;
using UnityEngine;

namespace MapsExt.Editor.Interactions
{
	public class MoveInteraction : MonoBehaviour, IEditorInteraction
	{
		private MapEditor editor;

		private bool isDraggingMapObjects;
		private Vector3 prevMouse;
		private Vector3Int prevCell;
		private Vector3 offset;

		private void Start()
		{
			this.editor = this.GetComponentInParent<MapEditor>();
		}

		private void Update()
		{
			if (this.isDraggingMapObjects)
			{
				this.DragMapObjects();
			}
		}

		public void OnPointerDown()
		{
			var mousePos = EditorInput.mousePosition;
			var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));

			this.prevMouse = mouseWorldPos;
			this.isDraggingMapObjects = true;

			if (this.editor.selectedObjects.Count == 1)
			{
				var selectedObj = this.editor.selectedObjects[0];

				var referenceRotation = selectedObj.transform.rotation;
				var referenceAngles = referenceRotation.eulerAngles;
				referenceRotation.eulerAngles = new Vector3(referenceAngles.x, referenceAngles.y, referenceAngles.z % 90);

				this.editor.grid.transform.rotation = referenceRotation;

				var scaleOffset = Vector3.zero;
				var objectCell = this.editor.grid.WorldToCell(selectedObj.transform.position);
				var snappedPosition = this.editor.grid.CellToWorld(objectCell);

				if (snappedPosition != selectedObj.transform.position)
				{
					var diff = selectedObj.transform.position - snappedPosition;
					var identityDiff = Quaternion.Inverse(referenceRotation) * diff;
					var identityDelta = new Vector3(this.editor.GridSize / 2f, this.editor.GridSize / 2f, 0);

					const float eps = 0.000005f;

					if ((Mathf.Abs(identityDiff.x) < eps || Mathf.Abs(identityDiff.x - identityDelta.x) < eps) &&
						(Mathf.Abs(identityDiff.y) < eps || Mathf.Abs(identityDiff.y - identityDelta.y) < eps))
					{
						scaleOffset = diff;
					}
				}

				this.offset = scaleOffset;
			}
			else
			{
				this.editor.grid.transform.rotation = Quaternion.identity;

				foreach (var handler in this.editor.selectedObjects)
				{
					var objectCell = this.editor.grid.WorldToCell(handler.transform.position);
					var snappedPosition = this.editor.grid.CellToWorld(objectCell);
					this.offset = handler.transform.position - snappedPosition;
				}
			}

			this.prevCell = this.editor.grid.WorldToCell(mouseWorldPos);
		}

		public void OnPointerUp()
		{
			this.isDraggingMapObjects = false;
			this.editor.UpdateRopeAttachments();
			this.editor.TakeSnaphot();
		}

		private void DragMapObjects()
		{
			var mousePos = EditorInput.mousePosition;
			var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));
			var mouseCell = this.editor.grid.WorldToCell(mouseWorldPos);
			var mouseDelta = mouseWorldPos - this.prevMouse;
			var cellDelta = mouseCell - this.prevCell;

			var delta = mouseDelta;

			if (this.editor.snapToGrid)
			{
				var selectedObj = this.editor.selectedObjects[0];
				var objectCell = this.editor.grid.WorldToCell(selectedObj.transform.position);
				var snappedPosition = this.editor.grid.CellToWorld(objectCell + cellDelta);

				delta = snappedPosition + this.offset - selectedObj.transform.position;
			}

			if (delta != Vector3.zero)
			{
				foreach (var handler in this.editor.selectedObjects.SelectMany(obj => obj.GetComponents<PositionHandler>()))
				{
					handler.Move(delta);
					handler.OnChange();
				}
			}

			this.prevMouse += mouseDelta;
			this.prevCell = mouseCell;
		}
	}
}
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public class PositionHandler : MapObjectActionHandler
	{
		public virtual void Move(Vector3 delta)
		{
			this.transform.position += delta;
		}

		public virtual void SetPosition(Vector3 position)
		{
			this.transform.position = position;
		}
	}

	public class SizeHandler : MapObjectActionHandler
	{
		public virtual void Resize(Vector3 delta, int resizeDirection = 0)
		{
			this.SetSize(this.transform.localScale + delta, resizeDirection);
		}

		public virtual void SetSize(Vector3 size, int resizeDirection = 0)
		{
			var delta = size - this.transform.localScale;
			float gridSize = this.gameObject.GetComponentInParent<Editor.MapEditor>().GridSize;
			bool snapToGrid = this.gameObject.GetComponentInParent<Editor.MapEditor>().snapToGrid;

			var directionMulti = Editor.AnchorPosition.directionMultipliers[resizeDirection];
			var scaleMulti = Editor.AnchorPosition.sizeMultipliers[resizeDirection];
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
			this.transform.position += positionDelta * 0.5f;
		}
	}

	public class RotationHandler : MapObjectActionHandler
	{
		public virtual void SetRotation(Quaternion rotation)
		{
			this.transform.rotation = rotation;
		}
	}
}

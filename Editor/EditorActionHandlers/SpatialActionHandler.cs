using UnityEngine;

namespace MapsExt.Editor
{
	public class SpatialActionHandler : EditorActionHandler
	{
		public override bool CanRotate()
		{
			return true;
		}
		
		public override bool CanResize(int resizeDirection)
		{
			return true;
		}

		public override bool Resize(Vector3 sizeDelta, int resizeDirection)
		{
			float gridSize = this.gameObject.GetComponentInParent<Editor.MapEditor>().GridSize;
			bool snapToGrid = this.gameObject.GetComponentInParent<Editor.MapEditor>().snapToGrid;

			var scaleMulti = Editor.TogglePosition.directionMultipliers[resizeDirection];
			var scaleDelta = Vector3.Scale(sizeDelta, scaleMulti);

			if (snapToGrid && scaleDelta.x != 0 && this.transform.localScale.x + scaleDelta.x < gridSize)
			{
				scaleDelta.x = gridSize - this.transform.localScale.x;
			}

			if (snapToGrid && scaleDelta.y != 0 && this.transform.localScale.y + scaleDelta.y < gridSize)
			{
				scaleDelta.y = gridSize - this.transform.localScale.y;
			}

			if (scaleDelta.x != 0 && this.transform.localScale.x + scaleDelta.x < 0.1f)
			{
				scaleDelta.x = 0.1f - this.transform.localScale.x;
			}

			if (scaleDelta.y != 0 && this.transform.localScale.y + scaleDelta.y < 0.1f)
			{
				scaleDelta.y = 0.1f - this.transform.localScale.y;
			}

			var positionDelta = this.transform.rotation * Vector3.Scale(scaleDelta, scaleMulti);
			var newScale = this.transform.localScale + scaleDelta;

			if (newScale == this.transform.localScale)
			{
				return false;
			}

			this.transform.localScale = newScale;
			this.transform.position += positionDelta * 0.5f;
			return true;
		}
	}
}

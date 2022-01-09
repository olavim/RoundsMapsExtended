using MapsExt.Editor.Commands;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public class MoveActionHandler : ActionHandler<MoveCommand>
	{
		public override void Handle(MoveCommand cmd)
		{
			this.transform.position += cmd.delta;
		}
	}

	public class ResizeActionHandler : ActionHandler<ResizeCommand>
	{
		public override void Handle(ResizeCommand cmd)
		{
			float gridSize = this.gameObject.GetComponentInParent<Editor.MapEditor>().GridSize;
			bool snapToGrid = this.gameObject.GetComponentInParent<Editor.MapEditor>().snapToGrid;

			var directionMulti = Editor.AnchorPosition.directionMultipliers[cmd.resizeDirection];
			var scaleMulti = Editor.AnchorPosition.sizeMultipliers[cmd.resizeDirection];
			var scaleDelta = Vector3.Scale(cmd.delta, scaleMulti);

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

	public class RotateActionHandler : ActionHandler<RotateCommand>
	{
		public override void Handle(RotateCommand cmd)
		{
			this.transform.rotation = cmd.toRotation;
		}
	}
}

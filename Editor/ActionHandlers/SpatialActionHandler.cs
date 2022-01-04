using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public class SpatialActionHandler : EditorActionHandler
	{
		public override bool CanResize => true;
		public override bool CanRotate => true;

		public override void Move(Vector3 positionDelta)
		{
			this.Move(positionDelta, 0);
		}

		public override void Resize(Vector3 sizeDelta, int resizeDirection)
		{
			this.Resize(sizeDelta, resizeDirection, 0);
		}

		public override void SetRotation(Quaternion rotation)
		{
			this.SetRotation(rotation, 0);
		}

		public void Move(Vector3 positionDelta, int keyframe)
		{
			var animation = this.GetComponent<MapObjectAnimation>();

			if (animation)
			{
				animation.keyframes[keyframe].position += positionDelta;
			}

			if (!animation || keyframe == 0)
			{
				this.transform.position += positionDelta;
			}
		}

		public void Resize(Vector3 sizeDelta, int resizeDirection, int keyframe)
		{
			float gridSize = this.gameObject.GetComponentInParent<Editor.MapEditor>().GridSize;
			bool snapToGrid = this.gameObject.GetComponentInParent<Editor.MapEditor>().snapToGrid;

			var directionMulti = Editor.AnchorPosition.directionMultipliers[resizeDirection];
			var scaleMulti = Editor.AnchorPosition.sizeMultipliers[resizeDirection];
			var scaleDelta = Vector3.Scale(sizeDelta, scaleMulti);

			var animation = this.GetComponent<MapObjectAnimation>();
			var currentScale = animation ? animation.keyframes[keyframe].scale : this.transform.localScale;
			var currentRotation = animation ? animation.keyframes[keyframe].rotation : this.transform.rotation;

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

			if (animation)
			{
				animation.keyframes[keyframe].scale = newScale;
				animation.keyframes[keyframe].position += positionDelta * 0.5f;
			}

			if (!animation || keyframe == 0)
			{
				this.transform.localScale = newScale;
				this.transform.position += positionDelta * 0.5f;
			}
		}

		public void SetRotation(Quaternion rotation, int keyframe)
		{
			var animation = this.GetComponent<MapObjectAnimation>();

			if (animation)
			{
				animation.keyframes[keyframe].rotation = rotation;
			}

			if (!animation || keyframe == 0)
			{
				this.transform.rotation = rotation;
			}
		}
	}
}

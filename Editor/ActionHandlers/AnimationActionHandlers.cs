using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public class AnimationMoveHandler : PositionHandler
	{
		public MapObjectAnimation animation;
		public int frameIndex;

		public override void SetPosition(Vector3 position)
		{
			base.SetPosition(position);
			this.animation.keyframes[this.frameIndex].GetComponentValue<PositionComponentValue>().Value = this.transform.position;
		}
	}

	public class AnimationResizeHandler : SizeHandler
	{
		public MapObjectAnimation animation;
		public int frameIndex;

		public override void SetSize(Vector3 scale, int resizeDirection)
		{
			base.SetSize(scale, resizeDirection);
			this.animation.keyframes[this.frameIndex].GetComponentValue<ScaleComponentValue>().Value = this.transform.localScale;
		}
	}

	public class AnimationRotateHandler : RotationHandler
	{
		public MapObjectAnimation animation;
		public int frameIndex;

		public override void SetRotation(Quaternion rotation)
		{
			base.SetRotation(rotation);
			this.animation.keyframes[this.frameIndex].GetComponentValue<RotationComponentValue>().Value = this.transform.rotation;
		}
	}
}

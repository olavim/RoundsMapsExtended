using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public interface IAnimationActionHandler
	{
		MapObjectAnimation animation { get; set; }
		int frameIndex { get; set; }
	}

	public class AnimationPositionHandler : PositionHandler, IAnimationActionHandler
	{
		public MapObjectAnimation animation { get; set; }
		public int frameIndex { get; set; }

		public override void Move(Vector3 delta)
		{
			base.Move(delta);
			this.animation.keyframes[this.frameIndex].GetComponentValue<PositionComponentValue>().Value = this.transform.position;
		}

		public override void SetPosition(Vector3 position)
		{
			base.SetPosition(position);
			this.animation.keyframes[this.frameIndex].GetComponentValue<PositionComponentValue>().Value = this.transform.position;
		}
	}

	public class AnimationSizeHandler : SizeHandler, IAnimationActionHandler
	{
		public MapObjectAnimation animation { get; set; }
		public int frameIndex { get; set; }

		public override void SetSize(Vector3 scale, int resizeDirection)
		{
			base.SetSize(scale, resizeDirection);
			this.animation.keyframes[this.frameIndex].GetComponentValue<ScaleComponentValue>().Value = this.transform.localScale;
		}
	}

	public class AnimationRotationHandler : RotationHandler, IAnimationActionHandler
	{
		public MapObjectAnimation animation { get; set; }
		public int frameIndex { get; set; }

		public override void SetRotation(Quaternion rotation)
		{
			base.SetRotation(rotation);
			this.animation.keyframes[this.frameIndex].GetComponentValue<RotationComponentValue>().Value = this.transform.rotation;
		}
	}
}

using MapsExt.Editor.Commands;

namespace MapsExt.Editor.ActionHandlers
{
	public class AnimationMoveActionHandler : MoveActionHandler
	{
		public MapObjectAnimation animation;
		public int frameIndex;

		public override void Handle(MoveCommand cmd)
		{
			base.Handle(cmd);
			this.animation.keyframes[this.frameIndex].position = this.transform.position;
		}
	}

	public class AnimationResizeActionHandler : ResizeActionHandler
	{
		public MapObjectAnimation animation;
		public int frameIndex;

		public override void Handle(ResizeCommand cmd)
		{
			base.Handle(cmd);
			this.animation.keyframes[this.frameIndex].scale = this.transform.localScale;
		}
	}

	public class AnimationRotateActionHandler : RotateActionHandler
	{
		public MapObjectAnimation animation;
		public int frameIndex;

		public override void Handle(RotateCommand cmd)
		{
			base.Handle(cmd);
			this.animation.keyframes[this.frameIndex].rotation = this.transform.rotation;
		}
	}
}

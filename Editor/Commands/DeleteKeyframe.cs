using MapsExt.MapObjects;
using UnityEngine;

namespace MapsExt.Editor.Commands
{
	public class DeleteKeyframeCommand : ICommand
	{
		public readonly MapObject data;
		public readonly AnimationKeyframe frame;
		public readonly int frameIndex;

		public DeleteKeyframeCommand(GameObject instance, int frameIndex)
		{
			this.data = MapsExtendedEditor.instance.mapObjectManager.Serialize(instance);
			this.frame = new AnimationKeyframe(instance.GetComponent<MapObjectAnimation>().keyframes[frameIndex]);
			this.frameIndex = frameIndex;
		}
	}

	public class DeleteKeyframeCommandHandler : CommandHandler<DeleteKeyframeCommand>
	{
		private MapEditor editor;

		public DeleteKeyframeCommandHandler(MapEditor editor)
		{
			this.editor = editor;
		}

		override public void Execute(DeleteKeyframeCommand cmd)
		{
			var instance = cmd.data.FindInstance(this.editor.content).gameObject;
			var animation = instance.GetComponent<MapObjectAnimation>();
			animation.keyframes.RemoveAt(cmd.frameIndex);

			if (this.editor.animationHandler.KeyframeIndex >= animation.keyframes.Count)
			{
				this.editor.animationHandler.SetKeyframe(animation.keyframes.Count - 1);
			}
		}

		override public void Undo(DeleteKeyframeCommand cmd)
		{
			var instance = cmd.data.FindInstance(this.editor.content).gameObject;
			var animation = instance.GetComponent<MapObjectAnimation>();
			animation.keyframes.Insert(cmd.frameIndex, cmd.frame);
		}

		override public DeleteKeyframeCommand Merge(DeleteKeyframeCommand cmd1, DeleteKeyframeCommand cmd2)
		{
			return cmd2;
		}
	}
}

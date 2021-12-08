using MapsExt.MapObjects;
using UnityEngine;

namespace MapsExt.Editor.Commands
{
	public class AddKeyframeCommand : ICommand
	{
		public readonly MapObject data;
		public readonly AnimationKeyframe frame;
		public readonly int frameIndex;

		public AddKeyframeCommand(GameObject instance, AnimationKeyframe frame, int frameIndex)
		{
			this.data = MapsExtendedEditor.instance.mapObjectManager.Serialize(instance);
			this.frame = frame;
			this.frameIndex = frameIndex;
		}

		public AddKeyframeCommand(GameObject instance)
		{
			this.data = MapsExtendedEditor.instance.mapObjectManager.Serialize(instance);
			this.frame = new AnimationKeyframe((SpatialMapObject) this.data);
			this.frameIndex = instance.GetComponent<MapObjectAnimation>().keyframes.Count;
		}
	}

	public class AddKeyframeCommandHandler : CommandHandler<AddKeyframeCommand>
	{
		private MapEditor editor;

		public AddKeyframeCommandHandler(MapEditor editor)
		{
			this.editor = editor;
		}

		override public void Execute(AddKeyframeCommand cmd)
		{
			var instance = cmd.data.FindInstance(this.editor.content).gameObject;
			var animation = instance.GetComponent<MapObjectAnimation>();
			animation.keyframes.Insert(cmd.frameIndex, cmd.frame);

			this.editor.animationHandler.SetKeyframe(cmd.frameIndex);
		}

		override public void Undo(AddKeyframeCommand cmd)
		{
			var instance = cmd.data.FindInstance(this.editor.content).gameObject;
			var animation = instance.GetComponent<MapObjectAnimation>();
			animation.keyframes.RemoveAt(cmd.frameIndex);

			if (this.editor.animationHandler.KeyframeIndex >= animation.keyframes.Count)
			{
				this.editor.animationHandler.SetKeyframe(animation.keyframes.Count - 1);
			}
		}

		override public AddKeyframeCommand Merge(AddKeyframeCommand cmd1, AddKeyframeCommand cmd2)
		{
			return cmd2;
		}
	}
}

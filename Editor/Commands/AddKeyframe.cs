using MapsExt.MapObjects;
using UnityEngine;
using UnboundLib;

namespace MapsExt.Editor.Commands
{
	public class AddKeyframeCommand : ICommand
	{
		public readonly SpatialMapObject data;
		public readonly AnimationKeyframe frame;
		public readonly int frameIndex;

		public AddKeyframeCommand(GameObject instance, AnimationKeyframe frame, int frameIndex)
		{
			this.data = (SpatialMapObject) MapsExtendedEditor.instance.mapObjectManager.Serialize(instance);
			this.frame = frame;
			this.frameIndex = frameIndex;
		}

		public AddKeyframeCommand(GameObject instance)
		{
			this.data = (SpatialMapObject) MapsExtendedEditor.instance.mapObjectManager.Serialize(instance);
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

		public override void Execute(AddKeyframeCommand cmd)
		{
			var instance = cmd.data.FindInstance(this.editor.content).gameObject;
			var animation = instance.GetOrAddComponent<MapObjectAnimation>();

			if (animation.keyframes.Count == 0)
			{
				animation.playOnAwake = false;
				animation.Initialize(cmd.data);
			}

			animation.keyframes.Insert(cmd.frameIndex, cmd.frame);

			this.editor.animationHandler.SetKeyframe(cmd.frameIndex);
		}

		public override void Undo(AddKeyframeCommand cmd)
		{
			var instance = cmd.data.FindInstance(this.editor.content).gameObject;
			var animation = instance.GetComponent<MapObjectAnimation>();
			animation.keyframes.RemoveAt(cmd.frameIndex);

			if (this.editor.animationHandler.KeyframeIndex >= animation.keyframes.Count)
			{
				this.editor.animationHandler.SetKeyframe(animation.keyframes.Count - 1);
			}
		}

		public override AddKeyframeCommand Merge(AddKeyframeCommand cmd1, AddKeyframeCommand cmd2)
		{
			return cmd2;
		}

		public override bool IsRedundant(AddKeyframeCommand cmd)
		{
			return cmd.frame == null;
		}
	}
}

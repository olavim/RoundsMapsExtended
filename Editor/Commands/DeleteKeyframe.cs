using MapsExt.MapObjects;
using System.Collections;
using UnityEngine;

namespace MapsExt.Editor.Commands
{
	public class DeleteKeyframeCommand : ICommand
	{
		public readonly SpatialMapObject data;
		public readonly AnimationKeyframe frame;
		public readonly int frameIndex;

		public DeleteKeyframeCommand(GameObject instance, int frameIndex)
		{
			this.data = (SpatialMapObject) MapsExtendedEditor.instance.mapObjectManager.Serialize(instance);
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

		public override IEnumerator Execute(DeleteKeyframeCommand cmd)
		{
			var instance = cmd.data.FindInstance(this.editor.content).gameObject;
			var animation = instance.GetComponent<MapObjectAnimation>();
			animation.keyframes.RemoveAt(cmd.frameIndex);

			if (this.editor.animationHandler.KeyframeIndex >= animation.keyframes.Count)
			{
				this.editor.animationHandler.SetKeyframe(animation.keyframes.Count - 1);
			}

			yield break;
		}

		public override IEnumerator Undo(DeleteKeyframeCommand cmd)
		{
			var instance = cmd.data.FindInstance(this.editor.content).gameObject;
			var animation = instance.GetComponent<MapObjectAnimation>();
			animation.keyframes.Insert(cmd.frameIndex, cmd.frame);
			yield break;
		}

		public override DeleteKeyframeCommand Merge(DeleteKeyframeCommand cmd1, DeleteKeyframeCommand cmd2)
		{
			return cmd2;
		}

		public override bool IsRedundant(DeleteKeyframeCommand cmd)
		{
			return cmd.data == null;
		}
	}
}

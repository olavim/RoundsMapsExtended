using MapsExt.MapObjects;
using UnityEngine;

namespace MapsExt.Editor.Commands
{
	public class ChangeKeyframeDurationCommand : ICommand
	{
		public readonly MapObject data;
		public readonly float delta;
		public readonly int frameIndex;

		public ChangeKeyframeDurationCommand(GameObject instance, float delta, int frameIndex)
		{
			this.data = MapsExtendedEditor.instance.mapObjectManager.Serialize(instance);
			this.delta = delta;
			this.frameIndex = frameIndex;
		}

		public ChangeKeyframeDurationCommand(MapObject data, float delta, int frameIndex)
		{
			this.data = data;
			this.delta = delta;
			this.frameIndex = frameIndex;
		}
	}

	public class ChangeKeyframeDurationCommandHandler : CommandHandler<ChangeKeyframeDurationCommand>
	{
		private MapEditor editor;

		public ChangeKeyframeDurationCommandHandler(MapEditor editor)
		{
			this.editor = editor;
		}

		override public void Execute(ChangeKeyframeDurationCommand cmd)
		{
			var instance = cmd.data.FindInstance(this.editor.content).gameObject;
			var animation = instance.GetComponent<MapObjectAnimation>();
			animation.keyframes[cmd.frameIndex].duration += cmd.delta;
			animation.keyframes[cmd.frameIndex].UpdateCurve();
		}

		override public void Undo(ChangeKeyframeDurationCommand cmd)
		{
			var instance = cmd.data.FindInstance(this.editor.content).gameObject;
			var animation = instance.GetComponent<MapObjectAnimation>();
			animation.keyframes[cmd.frameIndex].duration = ((SpatialMapObject) cmd.data).animationKeyframes[cmd.frameIndex - 1].duration;
			animation.keyframes[cmd.frameIndex].UpdateCurve();
		}

		override public ChangeKeyframeDurationCommand Merge(ChangeKeyframeDurationCommand cmd1, ChangeKeyframeDurationCommand cmd2)
		{
			return new ChangeKeyframeDurationCommand(cmd1.data, cmd1.delta + cmd2.delta, cmd1.frameIndex);
		}
	}
}

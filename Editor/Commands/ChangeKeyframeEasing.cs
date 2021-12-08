using MapsExt.MapObjects;
using UnityEngine;

namespace MapsExt.Editor.Commands
{
	public class ChangeKeyframeEasingCommand : ICommand
	{
		public readonly MapObject data;
		public readonly AnimationKeyframe.CurveType curveType;
		public readonly int frameIndex;

		public ChangeKeyframeEasingCommand(GameObject instance, AnimationKeyframe.CurveType curveType, int frameIndex)
		{
			this.data = MapsExtendedEditor.instance.mapObjectManager.Serialize(instance);
			this.curveType = curveType;
			this.frameIndex = frameIndex;
		}
	}

	public class ChangeKeyframeEasingCommandHandler : CommandHandler<ChangeKeyframeEasingCommand>
	{
		private MapEditor editor;

		public ChangeKeyframeEasingCommandHandler(MapEditor editor)
		{
			this.editor = editor;
		}

		override public void Execute(ChangeKeyframeEasingCommand cmd)
		{
			var instance = cmd.data.FindInstance(this.editor.content).gameObject;
			var animation = instance.GetComponent<MapObjectAnimation>();
			animation.keyframes[cmd.frameIndex].curveType = cmd.curveType;
			animation.keyframes[cmd.frameIndex].UpdateCurve();
		}

		override public void Undo(ChangeKeyframeEasingCommand cmd)
		{
			var instance = cmd.data.FindInstance(this.editor.content).gameObject;
			var animation = instance.GetComponent<MapObjectAnimation>();
			animation.keyframes[cmd.frameIndex].curveType = ((SpatialMapObject) cmd.data).animationKeyframes[cmd.frameIndex - 1].curveType;
			animation.keyframes[cmd.frameIndex].UpdateCurve();
		}

		override public ChangeKeyframeEasingCommand Merge(ChangeKeyframeEasingCommand cmd1, ChangeKeyframeEasingCommand cmd2)
		{
			return cmd2;
		}
	}
}

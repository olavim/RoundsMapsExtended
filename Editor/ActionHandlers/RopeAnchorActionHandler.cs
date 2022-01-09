using MapsExt.Editor.Commands;

namespace MapsExt.Editor.ActionHandlers
{
	public class RopeAnchorMoveHandler : ActionHandler<MoveCommand>
	{
		public override void Handle(MoveCommand cmd)
		{
			var anchor = this.GetComponent<MapObjectAnchor>();
			this.transform.position += cmd.delta;
			anchor.UpdateAttachment();
		}
	}
}

using MapsExt.MapObjects.Properties;

namespace MapsExt.Editor.ActionHandlers
{
	public class RopeAnchorPositionHandler : PositionHandler
	{
		public override void Move(PositionProperty delta)
		{
			var anchor = this.GetComponent<MapObjectAnchor>();
			anchor.Detach();
			base.Move(delta);
			anchor.UpdateAttachment();
		}

		public override void SetValue(PositionProperty property)
		{
			var anchor = this.GetComponent<MapObjectAnchor>();
			anchor.Detach();
			base.SetValue(property);
			anchor.UpdateAttachment();
		}
	}
}

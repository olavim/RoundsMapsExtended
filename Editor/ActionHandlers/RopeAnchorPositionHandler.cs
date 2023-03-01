using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public class RopeAnchorPositionHandler : PositionHandler
	{
		public override void Move(Vector3 delta)
		{
			var anchor = this.GetComponent<MapObjectAnchor>();
			anchor.Detach();
			base.Move(delta);
			anchor.UpdateAttachment();
		}

		public override void SetPosition(Vector3 position)
		{
			var anchor = this.GetComponent<MapObjectAnchor>();
			anchor.Detach();
			base.SetPosition(position);
			anchor.UpdateAttachment();
		}
	}
}

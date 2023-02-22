using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public class RopeAnchorMoveHandler : PositionHandler
	{
		public override void Move(Vector3 delta)
		{
			var anchor = this.GetComponent<MapObjectAnchor>();
			anchor.Detach();
			this.transform.position += delta;
			anchor.UpdateAttachment();
		}

		public override void SetPosition(Vector3 position)
		{
			var anchor = this.GetComponent<MapObjectAnchor>();
			anchor.Detach();
			this.transform.position = position;
			anchor.UpdateAttachment();
		}
	}
}

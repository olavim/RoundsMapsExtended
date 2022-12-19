using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public class RopeAnchorMoveHandler : PositionHandler
	{
		public override void SetPosition(Vector3 position)
		{
			var anchor = this.GetComponent<MapObjectAnchor>();
			this.transform.position = position;
			anchor.UpdateAttachment();
		}
	}
}

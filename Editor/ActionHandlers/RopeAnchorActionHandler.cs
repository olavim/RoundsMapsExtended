using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public class RopeAnchorActionHandler : EditorActionHandler
	{
		public override void Move(Vector3 positionDelta)
		{
			var anchor = this.GetComponent<MapObjectAnchor>();
			this.transform.position += positionDelta;
			anchor.UpdateAttachment();
		}
	}
}

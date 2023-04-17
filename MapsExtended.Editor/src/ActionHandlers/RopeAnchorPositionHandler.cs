using MapsExt.Properties;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	[RequireComponent(typeof(SelectionHandler))]
	public class RopeAnchorPositionHandler : PositionHandler
	{
		public override void SetValue(PositionProperty property)
		{
			var anchor = this.GetComponent<MapObjectAnchor>();
			anchor.Detach();
			base.SetValue(property);
			anchor.UpdateAttachment();
		}

		public override void OnRefresh()
		{
			this.GetComponent<MapObjectAnchor>().UpdateAttachment();
		}
	}
}

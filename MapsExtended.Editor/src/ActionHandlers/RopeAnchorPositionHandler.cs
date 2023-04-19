using MapsExt.Properties;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	[RequireComponent(typeof(SelectionHandler))]
	public class RopeAnchorPositionHandler : PositionHandler
	{
		public override PositionProperty GetValue()
		{
			return this.GetComponent<MapObjectAnchor>().GetAnchoredPosition();
		}

		public override void OnRefresh()
		{
			this.GetComponent<MapObjectAnchor>().UpdateAttachment();
		}

		public override void OnSelect(bool inGroup)
		{
			this.GetComponent<MapObjectAnchor>().Detach();
		}

		public override void OnDeselect()
		{
			this.GetComponent<MapObjectAnchor>().UpdateAttachment();
		}
	}
}

using MapsExt.Properties;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	[RequireComponent(typeof(SelectionHandler))]
	public class RopeAnchorPositionHandler : PositionHandler
	{
		private bool _isSelected;

		public override PositionProperty GetValue()
		{
			return this.GetComponent<MapObjectAnchor>().GetAnchoredPosition();
		}

		public override void OnRefresh()
		{
			if (!this._isSelected)
			{
				this.GetComponent<MapObjectAnchor>().UpdateAttachment();
			}
		}

		public override void OnSelect(bool inGroup)
		{
			this._isSelected = true;
			this.GetComponent<MapObjectAnchor>().Detach();
		}

		public override void OnDeselect()
		{
			this._isSelected = false;
			this.GetComponent<MapObjectAnchor>().UpdateAttachment();
		}
	}
}

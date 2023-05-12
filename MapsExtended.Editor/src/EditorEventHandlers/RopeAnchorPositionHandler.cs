using MapsExt.Properties;
using UnityEngine;

namespace MapsExt.Editor.Events
{
	[RequireComponent(typeof(SelectionHandler))]
	public class RopeAnchorPositionHandler : PositionHandler
	{
		private bool _isSelected;

		public override PositionProperty GetValue()
		{
			return this.GetComponent<MapObjectAnchor>().GetAnchoredPosition();
		}

		protected override bool ShouldHandleEvent(IEditorEvent evt) => true;

		protected override void HandleEvent(IEditorEvent evt)
		{
			if (base.ShouldHandleEvent(evt))
			{
				base.HandleEvent(evt);

				switch (evt)
				{
					case SelectEvent:
						this.OnSelect();
						break;
					case DeselectEvent:
						this.OnDeselect();
						break;
				}
			}

			if (!this._isSelected && evt is DeselectEvent)
			{
				this.GetComponent<MapObjectAnchor>().UpdateAttachment();
			}
		}

		protected virtual void HandleGlobalEvent(IEditorEvent evt)
		{
			if (evt is SelectEvent)
			{
				this.OnSelect();
			}
			else if (evt is DeselectEvent)
			{
				this.OnDeselect();
			}
		}

		protected virtual void OnSelect()
		{
			this._isSelected = true;
			this.GetComponent<MapObjectAnchor>().Detach();
		}

		protected virtual void OnDeselect()
		{
			this._isSelected = false;
			this.GetComponent<MapObjectAnchor>().UpdateAttachment();
		}
	}
}

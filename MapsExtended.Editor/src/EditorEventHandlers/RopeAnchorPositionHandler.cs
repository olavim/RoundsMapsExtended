using MapsExt.Properties;
using System.Collections.Generic;
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

		protected override void HandleEditorEvent(IEditorEvent evt, ISet<EditorEventHandler> subjects)
		{
			base.HandleEditorEvent(evt, subjects);

			if (!this._isSelected && evt is DeselectEvent)
			{
				this.GetComponent<MapObjectAnchor>().UpdateAttachment();
			}
		}

		protected override void HandleAcceptedEditorEvent(IEditorEvent evt, ISet<EditorEventHandler> subjects)
		{
			UnityEngine.Debug.Log($"RopeAnchorPositionHandler: {this.ShouldHandleEvent(evt, subjects)}");
			base.HandleAcceptedEditorEvent(evt, subjects);

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

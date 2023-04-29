using System.Collections.Generic;
using UnityEngine;

namespace MapsExt.Editor.Events
{
	[GroupEventHandler(typeof(SelectionHandler))]
	public class GroupSelectionHandler : SelectionHandler
	{
		protected override void Awake()
		{
			base.Awake();
			this.gameObject.AddComponent<BoxCollider2D>();
		}

		protected override bool ShouldHandleEvent(IEditorEvent evt, ISet<EditorEventHandler> subjects) => true;

		protected override void HandleAcceptedEditorEvent(IEditorEvent evt, ISet<EditorEventHandler> subjects)
		{
			base.HandleAcceptedEditorEvent(evt, subjects);

			if (evt is SelectEvent)
			{
				this.OnSelect();
			}
		}
	}
}

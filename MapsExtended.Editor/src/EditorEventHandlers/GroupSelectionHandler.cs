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

		protected override bool ShouldHandleEvent(IEditorEvent evt) => true;

		protected override void HandleEvent(IEditorEvent evt)
		{
			base.HandleEvent(evt);

			if (evt is SelectEvent)
			{
				this.OnSelect();
			}
		}
	}
}

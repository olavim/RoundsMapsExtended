using System.Collections.Generic;
using System.Linq;

namespace MapsExt.Editor.Events
{
	[GroupEventHandler(typeof(SelectionHandler))]
	public class GroupSizeHandler : EditorEventHandler
	{
		protected override bool ShouldHandleEvent(IEditorEvent evt, ISet<EditorEventHandler> subjects) => true;

		protected override void HandleAcceptedEditorEvent(IEditorEvent evt, ISet<EditorEventHandler> subjects)
		{
			if (evt is SelectEvent)
			{
				var boundsArr = this.Editor.SelectedObjects.Select(obj => obj.GetComponent<SelectionHandler>().GetBounds()).ToArray();
				var bounds = boundsArr[0];
				for (var i = 1; i < boundsArr.Length; i++)
				{
					bounds.Encapsulate(boundsArr[i]);
				}

				this.transform.localScale = bounds.size;
			}
		}
	}
}

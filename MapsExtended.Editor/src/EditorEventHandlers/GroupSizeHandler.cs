using MapsExt.Editor.MapObjects;
using System.Linq;

namespace MapsExt.Editor.Events
{
	[GroupEventHandler(typeof(MapObjectPartHandler))]
	public class GroupSizeHandler : EditorEventHandler
	{
		protected override bool ShouldHandleEvent(IEditorEvent evt)
		{
			return this.Editor.ActiveMapObjectPart == this.gameObject;
		}

		protected override void HandleEvent(IEditorEvent evt)
		{
			if (evt is SelectEvent)
			{
				var boundsArr = this.Editor.SelectedMapObjectParts.Select(obj => obj.GetComponent<MapObjectPart>().Collider.bounds).ToArray();
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

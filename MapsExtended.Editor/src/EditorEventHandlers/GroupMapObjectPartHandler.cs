namespace MapsExt.Editor.Events
{
	[GroupEventHandler(typeof(MapObjectPartHandler))]
	public class GroupMapObjectPartHandler : MapObjectPartHandler
	{
		protected override bool ShouldHandleEvent(IEditorEvent evt)
		{
			return this.Editor.ActiveMapObjectPart == this.gameObject;
		}
	}
}

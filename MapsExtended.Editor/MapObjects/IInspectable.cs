using MapsExt.Editor.UI;

namespace MapsExt.MapObjects
{
	public interface IInspectable
	{
		void OnInspectorLayout(MapObjectInspector inspector, InspectorLayoutBuilder builder);
	}
}

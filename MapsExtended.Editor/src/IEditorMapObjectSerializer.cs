using MapsExt.MapObjects;

namespace MapsExt.Editor
{
	public interface IEditorMapObjectSerializer : IMapObjectSerializer
	{
		MapObjectData Serialize(MapObjectInstance mapObjectInstance);
	}
}

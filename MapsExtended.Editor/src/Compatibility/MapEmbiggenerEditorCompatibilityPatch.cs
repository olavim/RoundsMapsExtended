using MapsExt.Compatibility;

namespace MapsExt.Editor.Compatibility
{
	[CompatibilityPatch]
	internal sealed class MapEmbiggenerEditorCompatibilityPatch : ICompatibilityPatch
	{
		public void Apply()
		{
			MapsExtended.GetCompatibilityPatch<MapEmbiggenerCompatibilityPatch>().AddDisableCase((scene) => scene.name == "MapEditor");
		}
	}
}

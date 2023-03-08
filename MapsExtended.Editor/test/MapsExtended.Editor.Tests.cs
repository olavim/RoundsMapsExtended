using BepInEx;

namespace MapsExt.Editor.Tests
{
	[BepInPlugin(ModId, ModName, ModVersion)]
	public sealed class MapsExtendedEditorTests : BaseUnityPlugin
	{
		public const string ModId = MapsExtendedEditor.ModId + ".tests";
		public const string ModName = MapsExtendedEditor.ModName + ".Tests";
		public const string ModVersion = MapsExtendedEditor.ModVersion;
	}
}

using BepInEx;
using MapsExt.Editor;

namespace MapsExt.Editor.Tests
{
	[BepInPlugin(ModId, ModName, ModVersion)]
	public class MapsExtendedEditorTests : BaseUnityPlugin
	{
		public const string ModId = MapsExtendedEditor.ModId + ".tests";
		public const string ModName = MapsExtendedEditor.ModName + ".Tests";
		public const string ModVersion = "0.0.1";
	}
}

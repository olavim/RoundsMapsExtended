using BepInEx;

namespace MapsExt.Tests
{
	[BepInPlugin(ModId, ModName, ModVersion)]
	public sealed class MapsExtendedTests : BaseUnityPlugin
	{
		public const string ModId = MapsExtended.ModId + ".tests";
		public const string ModName = MapsExtended.ModName + ".Tests";
		public const string ModVersion = MapsExtended.ModVersion;
	}
}

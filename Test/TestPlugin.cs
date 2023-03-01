using BepInEx;
using BepInEx.Logging;
using MapsExt.Editor;

namespace MapsExt.Test
{
	[BepInDependency("com.willis.rounds.unbound", "2.7.3")]
	[BepInDependency(MapsExtended.ModId, MapsExtended.ModVersion)]
	[BepInDependency(MapsExtendedEditor.ModId, MapsExtendedEditor.ModVersion)]
	[BepInPlugin(ModId, ModName, ModVersion)]
	public class MapsExtendedTest : BaseUnityPlugin
	{
		public const string ModId = "io.olavim.rounds.mapsextended.test";
		public const string ModName = "MapsExtended.Test";
		public const string ModVersion = "0.0.1";

		public static MapsExtendedTest instance;

		internal new static ManualLogSource Logger { get; private set; }

		private void Awake()
		{
			MapsExtendedTest.Logger = base.Logger;
			MapsExtendedTest.instance = this;
		}

		private void Start()
		{
			var runner = new TestRunner();
			this.StartCoroutine(runner.DiscoverAndRun());
		}
	}
}

using System.Runtime.CompilerServices;

namespace MapsExt
{
	public static class MapManagerExtensions
	{
		private class MapManagerExtraData
		{
			public CustomMap CurrentCustomMap { get; set; }
		}

		private static readonly ConditionalWeakTable<MapManager, MapManagerExtraData> s_extraData = new();

		public static void SetCurrentCustomMap(this MapManager mgr, CustomMap customMap)
		{
			var data = s_extraData.GetOrCreateValue(mgr);
			data.CurrentCustomMap = customMap;
		}

		public static CustomMap GetCurrentCustomMap(this MapManager mgr)
		{
			var data = s_extraData.GetOrCreateValue(mgr);
			return data.CurrentCustomMap;
		}
	}
}

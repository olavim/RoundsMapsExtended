using System.Linq;
using UnityEngine.SceneManagement;

namespace MapsExt
{
	public static class MapManagerExtensions
	{
		public static bool TryGetCurrentCustomMap(this MapManager mgr, out CustomMap customMap)
		{
			string sceneName = SceneManager.GetActiveScene().name;
			if (sceneName.StartsWith("MapsExtended:"))
			{
				string id = sceneName.Split(':')[1];
				customMap = MapsExtended.LoadedMaps.First(m => m.Id == id);
				return true;
			}

			customMap = null;
			return false;
		}
	}
}

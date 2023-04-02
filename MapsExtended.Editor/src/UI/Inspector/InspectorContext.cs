using UnityEngine;

namespace MapsExt.Editor.UI
{
	public sealed class InspectorContext
	{
		public GameObject InspectorTarget { get; init; }
		public MapEditor Editor { get; init; }
	}
}

using UnityEngine;

namespace MapsExt.Editor.UI
{
	public sealed class DividerElement : InspectorElement
	{
		protected override GameObject GetInstance() => GameObject.Instantiate(Assets.InspectorDividerPrefab);
	}
}

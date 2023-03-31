using UnityEngine;

namespace MapsExt.Editor.UI
{
	public class DividerElement : InspectorElement
	{
		protected override GameObject GetInstance() => GameObject.Instantiate(Assets.InspectorDividerPrefab);
	}
}

using UnityEngine;

namespace MapsExt.Editor.UI
{
	public interface IInspectorElement
	{
		GameObject Instantiate(InspectorContext context);
		void OnUpdate();
	}
}

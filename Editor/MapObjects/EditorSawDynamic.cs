using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
	[MapsExtendedEditorMapObject(typeof(SawDynamic), "Saw", "Dynamic")]
	public class EditorSawDynamic : SawDynamicSpecification
	{
		protected override void OnDeserialize(SawDynamic data, GameObject target)
		{
			base.OnDeserialize(data, target);
			target.GetOrAddComponent<BoxActionHandler>();
		}
	}
}

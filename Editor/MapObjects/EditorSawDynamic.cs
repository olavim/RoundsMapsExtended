using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
	[MapsExtendedEditorMapObject(typeof(SawDynamic), "Saw", "Dynamic")]
	public class EditorSawDynamic : SawDynamicSpecification, IEditorMapObjectSpecification
	{
		protected override void Deserialize(SawDynamic data, GameObject target)
		{
			base.Deserialize(data, target);
			target.GetOrAddComponent<BoxActionHandler>();
		}
	}
}

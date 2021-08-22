using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
	[MapsExtendedEditorMapObject(typeof(MapsExt.MapObjects.Saw), "Saw", "Static")]
	public class EditorSaw : SawSpecification
	{
		protected override void OnDeserialize(MapsExt.MapObjects.Saw data, GameObject target)
		{
			base.OnDeserialize(data, target);
			target.GetOrAddComponent<BoxActionHandler>();
		}
	}
}

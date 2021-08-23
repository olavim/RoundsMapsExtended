using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
	[MapsExtendedEditorMapObject(typeof(BoxDestructible), "Box (Destructible)", "Dynamic")]
	public class EditorBoxDestructibleSpecification : BoxDestructibleSpecification, IEditorMapObjectSpecification
	{
		protected override void Deserialize(BoxDestructible data, GameObject target)
		{
			base.Deserialize(data, target);
			target.GetOrAddComponent<BoxActionHandler>();
		}
	}
}

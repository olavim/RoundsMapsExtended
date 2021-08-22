using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
	[MapsExtendedEditorMapObject(typeof(BoxDestructible), "Box (Destructible)", "Dynamic")]
	public class EditorBoxDestructibleSpecification : BoxDestructibleSpecification
	{
		protected override void OnDeserialize(BoxDestructible data, GameObject target)
		{
			base.OnDeserialize(data, target);
			target.GetOrAddComponent<BoxActionHandler>();
		}
	}
}

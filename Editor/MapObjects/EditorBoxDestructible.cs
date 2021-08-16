using MapsExt.MapObjects;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
    [MapsExtendedEditorMapObject(typeof(BoxDestructible))]
    public class EditorBoxDestructibleSpecification : BoxDestructibleSpecification
    {
        protected override void Deserialize(BoxDestructible data, GameObject target)
        {
            base.Deserialize(data, target);
            target.AddComponent<BoxActionHandler>();
        }
    }
}

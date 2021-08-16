using MapsExtended.MapObjects;
using UnityEngine;

namespace MapsExtended.Editor
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

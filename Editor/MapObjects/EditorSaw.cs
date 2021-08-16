using MapsExtended.MapObjects;
using UnityEngine;

namespace MapsExtended.Editor
{
    [MapsExtendedEditorMapObject(typeof(MapObjects.Saw))]
    public class EditorSaw : SawSpecification
    {
        protected override void Deserialize(MapObjects.Saw data, GameObject target)
        {
            base.Deserialize(data, target);
            target.AddComponent<BoxActionHandler>();
        }
    }
}

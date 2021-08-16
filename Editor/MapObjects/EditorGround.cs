using MapsExtended.MapObjects;
using UnityEngine;

namespace MapsExtended.Editor
{
    [MapsExtendedEditorMapObject(typeof(Ground))]
    public class EditorGroundSpecification : GroundSpecification
    {
        protected override void Deserialize(Ground data, GameObject target)
        {
            base.Deserialize(data, target);
            target.AddComponent<BoxActionHandler>();
        }
    }
}

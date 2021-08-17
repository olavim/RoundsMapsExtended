using MapsExt.MapObjects;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
    [MapsExtendedEditorMapObject(typeof(Ground), "Ground", "Static")]
    public class EditorGroundSpecification : GroundSpecification
    {
        protected override void OnDeserialize(Ground data, GameObject target)
        {
            base.OnDeserialize(data, target);
            target.AddComponent<BoxActionHandler>();
        }
    }
}

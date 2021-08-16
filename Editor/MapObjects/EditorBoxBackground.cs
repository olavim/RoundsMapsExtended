using MapsExt.MapObjects;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
    [MapsExtendedEditorMapObject(typeof(BoxBackground))]
    public class EditorBoxBackgroundSpecification : BoxBackgroundSpecification
    {
        protected override void Deserialize(BoxBackground data, GameObject target)
        {
            base.Deserialize(data, target);
            target.AddComponent<BoxActionHandler>();
        }
    }
}

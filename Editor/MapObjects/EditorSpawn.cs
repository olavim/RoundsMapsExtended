using MapsExt.MapObjects;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
    [MapsExtendedEditorMapObject(typeof(Spawn), "Spawn")]
    public class EditorSpawnSpecification : SpawnSpecification
    {
        protected override void OnDeserialize(Spawn data, GameObject target)
        {
            base.OnDeserialize(data, target);
            target.gameObject.AddComponent<Visualizers.SpawnVisualizer>();
            target.gameObject.AddComponent<SpawnActionHandler>();
            target.transform.SetAsLastSibling();
        }
    }
}

using UnityEngine;

namespace MapsExt.MapObjects
{
    public class BoxDestructible : PhysicalMapObject { }

    [MapsExtendedMapObject(typeof(BoxDestructible))]
    public class BoxDestructibleSpecification : PhysicalMapObjectSpecification<BoxDestructible>
    {
        public override GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/Box_Destructible");
    }
}

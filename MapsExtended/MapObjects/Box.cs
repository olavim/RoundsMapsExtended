using UnityEngine;

namespace MapsExtended.MapObjects
{
    public class Box : PhysicalMapObject { }

    [MapsExtendedMapObject(typeof(Box))]
    public class BoxSpecification : PhysicalMapObjectSpecification<Box>
    {
        public override GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/Box");
    }
}

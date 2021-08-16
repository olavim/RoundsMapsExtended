using System;
using UnityEngine;

namespace MapsExt.MapObjects
{
    public class MapObject
    {
        public bool active = true;
    }

    public abstract class MapObjectSpecification
    {
        public abstract GameObject Prefab { get; }

        internal abstract void DeserializeInternal(MapObject data, GameObject target);

        internal abstract MapObject SerializeInternal(GameObject instance);
    }

    public abstract class MapObjectSpecification<T> : MapObjectSpecification
        where T : MapObject
    {
        internal override void DeserializeInternal(MapObject data, GameObject target)
        {
            this.Deserialize((T) data, target);

            var c = target.AddComponent<MapObjectInstance>();
            c.dataType = data.GetType();
            target.SetActive(data.active);
        }

        internal override MapObject SerializeInternal(GameObject instance)
        {
            var data = this.Serialize(instance);
            data.active = instance.activeSelf;
            return data;
        }

        protected abstract void Deserialize(T data, GameObject target);

        protected abstract T Serialize(GameObject instance);
    }

    public class MapObjectInstance : MonoBehaviour
    {
        public Type dataType;
    }
}

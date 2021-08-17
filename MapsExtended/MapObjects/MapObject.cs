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

        public abstract void Deserialize(MapObject data, GameObject target);

        public abstract MapObject Serialize(GameObject instance);
    }

    public abstract class MapObjectSpecification<T> : MapObjectSpecification
        where T : MapObject
    {
        public override void Deserialize(MapObject data, GameObject target)
        {
            this.OnDeserialize((T) data, target);

            var c = target.AddComponent<MapObjectInstance>();
            c.dataType = data.GetType();
            target.SetActive(data.active);
        }

        public override MapObject Serialize(GameObject instance)
        {
            var data = this.OnSerialize(instance);
            data.active = instance.activeSelf;
            return data;
        }

        protected abstract void OnDeserialize(T data, GameObject target);

        protected abstract T OnSerialize(GameObject instance);
    }

    public class MapObjectInstance : MonoBehaviour
    {
        public Type dataType;
    }
}

using System;
using UnityEngine;

namespace MapsExtended
{
    [Serializable]
    public class MapObjectData
    {
        public string mapObjectName;
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;
        public bool active;

        public MapObjectData(MapObject obj)
        {
            this.mapObjectName = obj.mapObjectName;
            this.position = obj.transform.position;
            this.scale = obj.transform.localScale;
            this.rotation = obj.transform.rotation;
            this.active = obj.gameObject.activeSelf;
        }
    }

    public class MapObject : MonoBehaviour {
        public string mapObjectName;
    }
}

using System;
using UnityEngine;

namespace MapsExtended
{
    [Serializable]
    public class RopeData
    {
        public Vector3 startPosition;
        public Vector3 endPosition;
        public bool active;

        public RopeData(Vector3 startPosition, Vector3 endPosition, bool active)
        {
            this.startPosition = startPosition;
            this.endPosition = endPosition;
            this.active = active;
        }
    }
}

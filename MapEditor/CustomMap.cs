using System;
using System.Collections.Generic;

namespace MapEditor
{
    [Serializable]
    public class CustomMap
    {
        public List<MapObjectData> mapObjects;
        public List<SpawnPointData> spawns;
    }
}

using System;
using UnityEngine;

namespace MapsExtended
{
    [Serializable]
    public class SpawnPointData
    {
        public int id;
        public int teamID;
        public Vector3 position;
        public bool active;

        public SpawnPointData(SpawnPoint spawn)
        {
            this.id = spawn.ID;
            this.teamID = spawn.TEAMID;
            this.position = spawn.transform.position;
            this.active = spawn.gameObject.activeSelf;
        }

        public SpawnPointData(int id, int teamID, Vector3 position, bool active)
        {
            this.id = id;
            this.teamID = teamID;
            this.position = position;
            this.active = active;
        }
    }
}

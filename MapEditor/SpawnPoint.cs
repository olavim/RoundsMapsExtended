using System;
using UnityEngine;

namespace MapEditor
{
    [Serializable]
    public class SpawnPointData
    {
        public int id;
        public int teamID;
        public Vector3 position;

        public SpawnPointData(SpawnPoint spawn)
        {
            this.id = spawn.ID;
            this.teamID = spawn.TEAMID;
            this.position = spawn.transform.position;
        }

        public SpawnPointData(int id, int teamID, Vector3 position)
        {
            this.id = id;
            this.teamID = teamID;
            this.position = position;
        }
    }
}

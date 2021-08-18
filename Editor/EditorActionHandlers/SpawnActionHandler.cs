using UnityEngine;

namespace MapsExt.Editor
{
    public class SpawnActionHandler : EditorActionHandler
    {
        public override bool CanRotate()
        {
            return false;
        }

        public override bool CanResize(int resizeDirection)
        {
            return false;
        }

        public override bool Resize(Vector3 sizeDelta, int resizeDirection)
        {
            return false;
        }
    }
}

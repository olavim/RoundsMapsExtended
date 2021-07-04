using UnityEngine;

namespace MapsExtended.Editor
{
    public class SpawnActionHandler : MonoBehaviour, IEditorActionHandler
    {
        public bool CanRotate()
        {
            return false;
        }

        public bool CanResize(int resizeDirection)
        {
            return false;
        }

        public bool Resize(Vector3 mouseDelta, int resizeDirection)
        {
            return false;
        }
    }
}

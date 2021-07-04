using UnityEngine;

namespace MapsExtended.Editor
{
    public interface IEditorActionHandler
    {
        bool CanRotate();

        bool CanResize(int resizeDirection);

        bool Resize(Vector3 mouseDelta, int resizeDirection);
    }
}

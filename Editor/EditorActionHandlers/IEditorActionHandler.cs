using UnityEngine;

namespace MapsExt.Editor
{
    public interface IEditorActionHandler
    {
        bool CanRotate();

        bool CanResize(int resizeDirection);

        bool Resize(Vector3 sizeDelta, int resizeDirection);
    }
}

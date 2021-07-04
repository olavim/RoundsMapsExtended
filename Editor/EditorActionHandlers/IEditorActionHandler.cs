using UnityEngine;

namespace MapsExtended.Editor
{
    public interface IEditorActionHandler
    {
        bool Resize(Vector3 mouseDelta, int resizeDirection);
    }
}

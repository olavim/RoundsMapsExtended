using UnityEngine;

namespace MapEditor.Transformers
{
    public interface IMapObjectTransformer
    {
        bool Resize(Vector3 mouseDelta, int resizeDirection);
    }
}

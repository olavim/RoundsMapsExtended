﻿using UnityEngine;

namespace MapsExt.Editor
{
    public class RopeActionHandler : MonoBehaviour, IEditorActionHandler
    {
        public bool CanRotate()
        {
            return false;
        }

        public bool CanResize(int resizeDirection)
        {
            return false;
        }

        public bool Resize(Vector3 sizeDelta, int resizeDirection)
        {
            return false;
        }
    }
}

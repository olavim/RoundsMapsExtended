using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MapsExtended
{
    class EditorPointerEvents : MonoBehaviour, IPointerDownHandler
    {
        public Action<GameObject> pointerDown;

        public void OnPointerDown(PointerEventData eventData)
        {
            this.pointerDown?.Invoke(this.gameObject);
        }
    }
}

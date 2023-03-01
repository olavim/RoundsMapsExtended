using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MapsExt
{
	class PointerDownHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		public Action<GameObject> pointerDown;
		public Action<GameObject> pointerUp;

		public void OnPointerDown(PointerEventData eventData)
		{
			this.pointerDown?.Invoke(this.gameObject);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			this.pointerUp?.Invoke(this.gameObject);
		}
	}
}

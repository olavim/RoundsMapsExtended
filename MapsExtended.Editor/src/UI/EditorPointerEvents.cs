using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MapsExt
{
	public class PointerDownHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		public Action<GameObject> PointerDown { get; set; }
		public Action<GameObject> PointerUp { get; set; }

		public virtual void OnPointerDown(PointerEventData eventData)
		{
			this.PointerDown?.Invoke(this.gameObject);
		}

		public virtual void OnPointerUp(PointerEventData eventData)
		{
			this.PointerUp?.Invoke(this.gameObject);
		}
	}
}

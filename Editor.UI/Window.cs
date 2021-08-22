using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MapsExt.UI
{
	public class Window : MonoBehaviour
	{
		public Text title;
		public Button closeButton;
		public GameObject content;

		public void Start()
		{
			this.closeButton.onClick.AddListener(() =>
			{
				this.gameObject.SetActive(false);
			});
		}
	}

	public class DragHandle : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler
	{
		public GameObject target;
		private Vector3 offset;
		public void OnPointerDown(PointerEventData data)
		{
			data.useDragThreshold = false;
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			this.offset = target.transform.position - Input.mousePosition;
		}

		public void OnDrag(PointerEventData data)
		{
			this.target.transform.position = Input.mousePosition + this.offset;
		}
	}
}

using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.UI
{
	public class Window : MonoBehaviour
	{
		public Text title;
		public Button closeButton;
		public GameObject content;

		protected virtual void Start()
		{
			this.closeButton.onClick.AddListener(() => this.gameObject.SetActive(false));
		}
	}
}

using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.UI
{
	public class Window : MonoBehaviour
	{
		[SerializeField] private Text _title;
		[SerializeField] private Button _closeButton;
		[SerializeField] private GameObject _content;

		public Text Title { get => this._title; set => this._title = value; }
		public Button CloseButton { get => this._closeButton; set => this._closeButton = value; }
		public GameObject Content { get => this._content; set => this._content = value; }

		protected virtual void Start()
		{
			this.CloseButton.onClick.AddListener(() => this.gameObject.SetActive(false));
		}
	}
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MapsExt.Editor.UI
{
	public class SaveDialog : MonoBehaviour
	{
		[SerializeField] private Button _saveButton;
		[SerializeField] private Button _closeButton;
		[SerializeField] private TextMeshProUGUI _title;
		[SerializeField] private TMP_InputField _textField;

		public Button SaveButton { get => this._saveButton; set => this._saveButton = value; }
		public Button CloseButton { get => this._closeButton; set => this._closeButton = value; }
		public TextMeshProUGUI Title { get => this._title; set => this._title = value; }
		public TMP_InputField TextField { get => this._textField; set => this._textField = value; }
	}
}

using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MapsExt.Utils;

namespace MapsExt.Editor.UI
{
	public sealed class FileBrowser : MonoBehaviour
	{
		[SerializeField] private Button _openButton;
		[SerializeField] private Button _closeButton;
		[SerializeField] private TMP_Dropdown _pathSelect;
		[SerializeField] private GameObject _fileContainer;
		[SerializeField] private TextMeshProUGUI _title;
		private string _basePath;

		public Button OpenButton { get => this._openButton; set => this._openButton = value; }
		public Button CloseButton { get => this._closeButton; set => this._closeButton = value; }
		public TMP_Dropdown PathSelect { get => this._pathSelect; set => this._pathSelect = value; }
		public GameObject FileContainer { get => this._fileContainer; set => this._fileContainer = value; }
		public TextMeshProUGUI Title { get => this._title; set => this._title = value; }

		public string SelectedPath { get; private set; }

		public void SetOptions(params string[] opts)
		{
			this.PathSelect.options = opts.Select(o => new TMP_Dropdown.OptionData(o)).ToList();
		}

		public void SetPath(string path)
		{
			this._basePath = path;
			this.Title.text = path;
			this.UpdateFiles();
		}

		public void UpdateFiles()
		{
			var paths = Directory.GetFiles(this._basePath, "*.map", SearchOption.AllDirectories);

			GameObjectUtils.DestroyChildrenImmediateSafe(this.FileContainer);

			foreach (string path in paths)
			{
				var go = new GameObject("Path");
				go.transform.SetParent(this.FileContainer.transform);

				var imageGo = new GameObject("Image");
				imageGo.transform.SetParent(go.transform);

				var image = imageGo.AddComponent<Image>();
				image.color = new Color32(255, 255, 255, 255);
				image.rectTransform.anchorMin = new Vector2(0, 0);
				image.rectTransform.anchorMax = new Vector2(1, 1);
				image.rectTransform.sizeDelta = new Vector2(0, 0);

				var textGo = new GameObject("Text");
				textGo.transform.SetParent(go.transform);

				var text = textGo.AddComponent<TextMeshProUGUI>();
				text.fontSize = 16;
				text.fontStyle = FontStyles.Bold;
				text.color = new Color32(200, 200, 200, 255);
				text.alignment = TextAlignmentOptions.MidlineLeft;
				text.text = path.Replace(this._basePath + Path.DirectorySeparatorChar, "");
				text.rectTransform.anchorMin = new Vector2(0, 0);
				text.rectTransform.anchorMax = new Vector2(1, 1);
				text.rectTransform.pivot = new Vector2(0, 0.5f);
				text.rectTransform.anchoredPosition = new Vector2(20, 0);
				text.rectTransform.sizeDelta = new Vector2(0, 0);

				var toggle = go.AddComponent<Toggle>();
				toggle.targetGraphic = image;
				toggle.toggleTransition = Toggle.ToggleTransition.Fade;

				toggle.colors = new ColorBlock()
				{
					colorMultiplier = 1,
					normalColor = new Color32(0, 0, 0, 0),
					highlightedColor = new Color32(255, 255, 255, 2),
					pressedColor = new Color32(255, 255, 255, 4)
				};

				toggle.onValueChanged.AddListener(isOn =>
				{
					if (isOn || this.SelectedPath == path)
					{
						this.SelectedPath = isOn ? path : null;
					}
				});

				var layout = go.AddComponent<LayoutElement>();
				layout.preferredHeight = 28;
			}
		}
	}
}

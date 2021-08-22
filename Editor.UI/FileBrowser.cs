using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MapsExt.UI
{
	public class FileBrowser : MonoBehaviour
	{
		public Button openButton;
		public Button closeButton;
		public TMP_Dropdown pathSelect;
		public GameObject fileContainer;
		public TextMeshProUGUI title;

		public string selectedPath;
		private string basePath;

		public void SetOptions(params string[] opts)
		{
			this.pathSelect.options = opts.Select(o => new TMP_Dropdown.OptionData(o)).ToList();
		}

		public void SetPath(string path)
		{
			this.basePath = path;
			this.title.text = path;
			this.UpdateFiles();
		}

		public void UpdateFiles()
		{
			var paths = Directory.GetFiles(this.basePath, "*.map", SearchOption.AllDirectories);

			foreach (Transform child in this.fileContainer.transform)
			{
				GameObject.Destroy(child.gameObject);
			}

			foreach (string path in paths)
			{
				var go = new GameObject("Path");
				go.transform.SetParent(this.fileContainer.transform);

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
				text.text = path.Replace(this.basePath + Path.DirectorySeparatorChar, "");
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
					if (isOn || this.selectedPath == path)
					{
						this.selectedPath = isOn ? path : null;
					}
				});

				var layout = go.AddComponent<LayoutElement>();
				layout.preferredHeight = 28;
			}
		}
	}
}

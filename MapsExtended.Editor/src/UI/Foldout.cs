using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.UI
{
	public sealed class Foldout : MonoBehaviour
	{
		[SerializeField] private Button _foldoutToggle;
		[SerializeField] private Text _label;
		[SerializeField] private GameObject _content;
		[SerializeField] private GameObject _expandedFeature;
		[SerializeField] private GameObject _collapsedFeature;

		public Button FoldoutToggle { get => this._foldoutToggle; set => this._foldoutToggle = value; }
		public Text Label { get => this._label; set => this._label = value; }
		public GameObject Content { get => this._content; set => this._content = value; }
		public GameObject ExpandedFeature { get => this._expandedFeature; set => this._expandedFeature = value; }
		public GameObject CollapsedFeature { get => this._collapsedFeature; set => this._collapsedFeature = value; }

		private void Start()
		{
			this.FoldoutToggle.onClick.AddListener(() =>
			{
				if (this.Content)
				{
					this.SetOpen(!this.Content.activeSelf);
				}
			});
		}

		public void SetOpen(bool open)
		{
			if (this.Content)
			{
				this.Content.SetActive(open);
			}

			if (this.ExpandedFeature)
			{
				this.ExpandedFeature.SetActive(open);
			}

			if (this.CollapsedFeature)
			{
				this.CollapsedFeature.SetActive(!open);
			}
		}
	}
}

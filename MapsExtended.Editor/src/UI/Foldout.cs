using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.UI
{
	public class Foldout : MonoBehaviour
	{
		public Button foldoutToggle;
		public Text label;
		public GameObject content;
		public GameObject expandedFeature;
		public GameObject collapsedFeature;

		protected virtual void Start()
		{
			this.foldoutToggle.onClick.AddListener(() =>
			{
				if (this.content)
				{
					this.SetOpen(!this.content.activeSelf);
				}
			});
		}

		public void SetOpen(bool open)
		{
			if (this.content)
			{
				this.content.SetActive(open);
			}

			if (this.expandedFeature)
			{
				this.expandedFeature.SetActive(open);
			}

			if (this.collapsedFeature)
			{
				this.collapsedFeature.SetActive(!open);
			}
		}
	}
}

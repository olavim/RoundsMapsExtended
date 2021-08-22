using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.UI
{
	public class Foldout : MonoBehaviour
	{
		public Button foldoutToggle;
		public Text label;
		public GameObject content;
		public GameObject expandedFeature;
		public GameObject collapsedFeature;

		public void Start()
		{
			this.foldoutToggle.onClick.AddListener(() =>
			{
				bool expand = !this.content?.activeSelf ?? true;
				this.content?.SetActive(expand);
				this.expandedFeature?.SetActive(expand);
				this.collapsedFeature?.SetActive(!expand);
			});
		}
	}
}

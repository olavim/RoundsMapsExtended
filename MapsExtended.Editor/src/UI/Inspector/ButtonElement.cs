using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.UI
{
	public abstract class ButtonElement : InspectorElement
	{
		public Button Button { get; private set; }

		protected string ButtonText
		{
			get => this.Button.GetComponentInChildren<Text>().text;
			set => this.Button.GetComponentInChildren<Text>().text = value;
		}

		protected override GameObject GetInstance()
		{
			var instance = GameObject.Instantiate(Assets.InspectorButtonPrefab);
			this.Button = instance.GetComponentInChildren<Button>();
			this.Button.onClick.AddListener(this.OnClick);
			return instance;
		}

		protected abstract void OnClick();
	}
}

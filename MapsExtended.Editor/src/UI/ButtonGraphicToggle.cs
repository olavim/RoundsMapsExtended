using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.UI
{
	public class ButtonGraphicToggle : MonoBehaviour
	{
		[SerializeField] private Graphic _graphic1;
		[SerializeField] private Graphic _graphic2;
		[SerializeField] private Button _button;

		private Graphic _currentGraphic;

		public Graphic Graphic1 { get => this._graphic1; set => this._graphic1 = value; }
		public Graphic Graphic2 { get => this._graphic2; set => this._graphic2 = value; }
		public Button Button { get => this._button; set => this._button = value; }

		protected virtual void OnEnable()
		{
			this._currentGraphic = this.Button.targetGraphic;
			this.Button.onClick.AddListener(this.ToggleGraphic);
		}

		protected virtual void OnDisable()
		{
			this.Button.onClick.RemoveListener(this.ToggleGraphic);
		}

		protected virtual void Update()
		{
			var color = this._currentGraphic.color;
			color.a = this.Button.interactable ? 1 : 0.5f;
			this._currentGraphic.color = color;
		}

		private void ToggleGraphic()
		{
			this._currentGraphic.gameObject.SetActive(false);

			this.Button.targetGraphic = this._currentGraphic == this.Graphic1 ? this.Graphic2 : this.Graphic1;
			this._currentGraphic = this.Button.targetGraphic;

			this._currentGraphic.gameObject.SetActive(true);
		}
	}
}

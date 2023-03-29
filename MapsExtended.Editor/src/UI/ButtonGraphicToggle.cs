using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.UI
{
	public class ButtonGraphicToggle : MonoBehaviour
	{
		public Graphic graphic1;
		public Graphic graphic2;
		public Button button;

		private Graphic _currentGraphic;

		protected virtual void OnEnable()
		{
			this._currentGraphic = this.button.targetGraphic;
			this.button.onClick.AddListener(this.ToggleGraphic);
		}

		protected virtual void OnDisable()
		{
			this.button.onClick.RemoveListener(this.ToggleGraphic);
		}

		protected virtual void Update()
		{
			var color = this._currentGraphic.color;
			color.a = this.button.interactable ? 1 : 0.5f;
			this._currentGraphic.color = color;
		}

		private void ToggleGraphic()
		{
			this._currentGraphic.gameObject.SetActive(false);

			this.button.targetGraphic = this._currentGraphic == this.graphic1 ? this.graphic2 : this.graphic1;
			this._currentGraphic = this.button.targetGraphic;

			this._currentGraphic.gameObject.SetActive(true);
		}
	}
}

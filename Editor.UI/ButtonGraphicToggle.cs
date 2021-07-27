using UnityEngine;
using UnityEngine.UI;

namespace MapsExtended.UI
{
    public class ButtonGraphicToggle : MonoBehaviour
    {
        public Graphic graphic1;
        public Graphic graphic2;
        public Button button;

        private Graphic currentGraphic;

        public void OnEnable()
        {
            this.currentGraphic = this.button.targetGraphic;
            this.button.onClick.AddListener(this.ToggleGraphic);
        }

        public void OnDisable()
        {
            this.button.onClick.RemoveListener(this.ToggleGraphic);
        }

        public void Update()
        {
            var color = this.currentGraphic.color;
            color.a = this.button.interactable ? 1 : 0.5f;
            this.currentGraphic.color = color;
        }

        private void ToggleGraphic()
        {
            this.currentGraphic.gameObject.SetActive(false);

            this.button.targetGraphic = this.currentGraphic == this.graphic1 ? this.graphic2 : this.graphic1;
            this.currentGraphic = this.button.targetGraphic;

            this.currentGraphic.gameObject.SetActive(true);
        }
    }
}

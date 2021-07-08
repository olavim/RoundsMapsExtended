using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MapsExtended.Visualizers
{
    public class SpawnVisualizer : MonoBehaviour, IMapObjectVisualizer
    {
        private Image labelBg;
        private TextMeshProUGUI label;
        private Image positionIndicator;

        public void SetEnabled(bool enabled)
        {
            this.enabled = enabled;
        }

        public void OnEnable()
        {
            float canvasScale = 4f;

            var collider = this.gameObject.AddComponent<BoxCollider2D>();
            collider.size = Vector3.one * 0.5f;

            var canvasGo = new GameObject("Canvas");
            canvasGo.transform.SetParent(this.transform);
            canvasGo.transform.localPosition = Vector3.zero;

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGo.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
            canvasGo.transform.localScale = new Vector3(1f / canvasScale, 1f / canvasScale, 1f);

            var imageGo = new GameObject("Image");
            imageGo.transform.SetParent(canvasGo.transform);
            imageGo.transform.localScale = Vector3.one;

            this.labelBg = imageGo.AddComponent<Image>();
            this.labelBg.rectTransform.sizeDelta = new Vector2(3f, 1f) * canvasScale;
            this.labelBg.color = new Color32(200, 200, 200, 150);
            this.labelBg.rectTransform.anchoredPosition = this.transform.position + new Vector3(0, 1.5f * canvasScale, 0);
            imageGo.transform.localPosition = new Vector3(0, 1.5f * canvasScale, 0);

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(canvasGo.transform);
            textGo.transform.localScale = Vector3.one;

            this.label = textGo.AddComponent<TextMeshProUGUI>();
            this.label.fontSize = 0.6f * canvasScale;
            this.label.fontStyle = FontStyles.Bold;
            this.label.color = new Color32(50, 50, 50, 255);
            this.label.alignment = TextAlignmentOptions.Center;
            this.label.rectTransform.anchoredPosition = this.transform.position + new Vector3(0, 1.5f * canvasScale, 0);
            textGo.transform.localPosition = new Vector3(0, 1.5f * canvasScale, 0);

            var pointCanvasGo = new GameObject("Position Indicator");
            pointCanvasGo.transform.SetParent(canvasGo.transform);
            pointCanvasGo.transform.localScale = Vector3.one;

            this.positionIndicator = pointCanvasGo.AddComponent<Image>();
            this.positionIndicator.rectTransform.sizeDelta = collider.size * canvasScale;
            this.positionIndicator.rectTransform.anchoredPosition = this.transform.position;
            pointCanvasGo.transform.localPosition = Vector3.zero;
        }

        public void OnDisable()
        {
            GameObject.Destroy(this.gameObject.GetComponent<BoxCollider2D>());
            GameObject.Destroy(this.transform.Find("Canvas").gameObject);
        }

        public void LateUpdate()
        {
            var spawnObj = this.gameObject.GetComponent<SpawnPoint>();
            this.label.text = $"Spawn {spawnObj.ID}";
        }
    }
}

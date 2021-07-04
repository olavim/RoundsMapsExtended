using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MapsExtended.Visualizers
{
    public class SpawnVisualizer : MonoBehaviour
    {
        private Image labelBg;
        private TextMeshProUGUI label;
        private Image positionIndicator;

        public void Awake()
        {
            float canvasScale = 4f;

            var spawnObj = this.gameObject.GetComponent<SpawnPoint>();
            var collider = this.gameObject.AddComponent<BoxCollider2D>();
            collider.size = Vector3.one * 0.5f;

            var canvasGo = new GameObject("Canvas");
            canvasGo.transform.SetParent(this.transform);

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
            this.labelBg.rectTransform.anchoredPosition = this.transform.position + new Vector3(0, 2f * canvasScale, 0);

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(canvasGo.transform);
            textGo.transform.localScale = Vector3.one;

            this.label = textGo.AddComponent<TextMeshProUGUI>();
            this.label.text = $"Spawn {spawnObj.ID}";
            this.label.fontSize = 0.6f * canvasScale;
            this.label.fontStyle = FontStyles.Bold;
            this.label.color = new Color32(50, 50, 50, 255);
            this.label.alignment = TextAlignmentOptions.Center;
            this.label.rectTransform.anchoredPosition = this.transform.position + new Vector3(0, 2f * canvasScale, 0);

            var pointCanvasGo = new GameObject("Position Indicator");
            pointCanvasGo.transform.SetParent(canvasGo.transform);
            pointCanvasGo.transform.localScale = Vector3.one;

            this.positionIndicator = pointCanvasGo.AddComponent<Image>();
            this.positionIndicator.rectTransform.sizeDelta = collider.size * canvasScale;
            this.positionIndicator.rectTransform.anchoredPosition = this.transform.position;
        }
    }
}

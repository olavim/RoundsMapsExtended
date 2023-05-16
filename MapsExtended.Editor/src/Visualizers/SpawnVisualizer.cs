using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using TMPro;
using MapsExt.Editor;

namespace MapsExt.Visualizers
{
	public class SpawnVisualizer : MonoBehaviour, IMapObjectVisualizer
	{
		private Image _labelBg;
		private TextMeshProUGUI _label;
		private Image _positionIndicator;
		private Canvas _canvas;

		public void SetEnabled(bool enabled)
		{
			this.enabled = enabled;
		}

		protected virtual void OnEnable()
		{
			var collider = this.gameObject.AddComponent<BoxCollider2D>();
			collider.size = new Vector3(3, 3);

			var colliderGo = new GameObject("Label Collider");
			colliderGo.transform.SetParent(this.transform);
			colliderGo.transform.localPosition = new Vector3(0, 2);

			var labelCollider = colliderGo.AddComponent<BoxCollider2D>();
			labelCollider.size = new Vector2(3, 1);

			var canvasGo = new GameObject("Canvas");
			canvasGo.transform.SetParent(this.transform);
			canvasGo.transform.localPosition = Vector3.zero;

			this._canvas = canvasGo.AddComponent<Canvas>();
			this._canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			this._canvas.sortingOrder = 1;

			var imageGo = new GameObject("Image");
			imageGo.transform.SetParent(canvasGo.transform);
			imageGo.transform.localScale = Vector3.one;

			this._labelBg = imageGo.AddComponent<ProceduralImage>();
			this._labelBg.rectTransform.sizeDelta = new Vector2(80f, 30f);
			this._labelBg.color = new Color(0, 0.57f, 0.45f, 0.2f);

			var modifier = imageGo.AddComponent<UniformModifier>();
			modifier.Radius = 8;

			var textGo = new GameObject("Text");
			textGo.transform.SetParent(canvasGo.transform);

			this._label = textGo.AddComponent<TextMeshProUGUI>();
			this._label.fontSize = 12;
			this._label.fontStyle = FontStyles.Bold;
			this._label.color = new Color(1, 1, 1, 0.8f);
			this._label.alignment = TextAlignmentOptions.Center;

			var pointCanvasGo = new GameObject("Position Indicator");
			pointCanvasGo.transform.SetParent(canvasGo.transform);

			this._positionIndicator = pointCanvasGo.AddComponent<Image>();
			this._positionIndicator.rectTransform.sizeDelta = Vector2.one * 10f;
		}

		protected virtual void OnDisable()
		{
			GameObject.Destroy(this.gameObject.GetComponent<BoxCollider2D>());
			GameObject.Destroy(this.transform.Find("Canvas").gameObject);
			GameObject.Destroy(this.transform.Find("Label Collider").gameObject);
		}

		private void LateUpdate()
		{
			var spawnObj = this.gameObject.GetComponent<SpawnPoint>();
			this._label.text = $"Spawn {spawnObj.ID}";

			var screenPos = MainCam.instance.cam.WorldToScreenPoint(this.transform.position) - MainCam.instance.cam.WorldToScreenPoint(MainCam.instance.cam.transform.position);
			this._positionIndicator.rectTransform.anchoredPosition = screenPos;
			this._label.rectTransform.anchoredPosition = screenPos + new Vector3(0, 50f, 0);
			this._labelBg.rectTransform.anchoredPosition = screenPos + new Vector3(0, 50f, 0);
		}
	}
}

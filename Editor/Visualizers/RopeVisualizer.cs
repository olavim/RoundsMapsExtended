using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace MapsExtended.Visualizers
{
    public class RopeVisualizer : MonoBehaviour, IMapObjectVisualizer
    {
        public GameObject start;
        public GameObject end;

        public Vector3 startOffset = Vector3.zero;
        public Vector3 endOffset = Vector3.zero;

        private LineRenderer renderer;
        private Graphic startGraphic;
        private Graphic endGraphic;
        private bool initialized = false;

        public void SetEnabled(bool enabled)
        {
            this.enabled = enabled;
        }

        public void Initialize()
        {
            this.initialized = true;
            this.OnEnable();
        }

        public void OnEnable()
        {
            if (!this.initialized)
            {
                return;
            }

            this.renderer = this.gameObject.AddComponent<LineRenderer>();
            this.renderer.material = new Material(Shader.Find("Sprites/Default"));
            this.renderer.startColor = new Color(0.039f, 0.039f, 0.039f, 1f);
            this.renderer.endColor = this.renderer.startColor;
            this.renderer.startWidth = 0.2f;
            this.renderer.endWidth = 0.2f;

            var startCollider = this.transform.GetChild(0).gameObject.AddComponent<BoxCollider2D>();
            var endCollider = this.transform.GetChild(1).gameObject.AddComponent<BoxCollider2D>();
            startCollider.size = Vector2.one * 1;
            endCollider.size = Vector2.one * 1;

            var canvasGo = new GameObject("Canvas");
            canvasGo.transform.SetParent(this.transform);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var startVizGo = new GameObject("Rope Start");
            var endVizGo = new GameObject("Rope End");

            startVizGo.transform.SetParent(canvasGo.transform);
            endVizGo.transform.SetParent(canvasGo.transform);

            this.startGraphic = startVizGo.AddComponent<ProceduralImage>();
            this.endGraphic = endVizGo.AddComponent<ProceduralImage>();

            this.startGraphic.rectTransform.sizeDelta = Vector2.one * 10;
            this.endGraphic.rectTransform.sizeDelta = Vector2.one * 10;

            this.startGraphic.color = new Color(1, 1, 1, 0.5f);
            this.endGraphic.color = new Color(1, 1, 1, 0.5f);

            var startModifier = startVizGo.AddComponent<UniformModifier>();
            var endModifier = endVizGo.AddComponent<UniformModifier>();

            startModifier.Radius = 5;
            endModifier.Radius = 5;
        }

        public void OnDisable()
        {
            GameObject.Destroy(this.gameObject.GetComponent<LineRenderer>());
            this.renderer = null;
            this.startGraphic = null;
            this.endGraphic = null;

            GameObject.Destroy(this.transform.Find("Canvas").gameObject);
            GameObject.Destroy(this.transform.GetChild(0).gameObject.GetComponent<BoxCollider2D>());
            GameObject.Destroy(this.transform.GetChild(1).gameObject.GetComponent<BoxCollider2D>());
        }

        public void Update()
        {
            if (!this.renderer || !this.startGraphic || !this.endGraphic)
            {
                return;
            }

            this.renderer.SetPositions(new Vector3[] { this.start.transform.position + this.startOffset, this.end.transform.position + this.endOffset });

            this.startGraphic.transform.position = MainCam.instance.cam.WorldToScreenPoint(this.start.transform.position + this.startOffset);
            this.endGraphic.transform.position = MainCam.instance.cam.WorldToScreenPoint(this.end.transform.position + this.endOffset);

            if (this.start != this.transform.GetChild(0).gameObject)
            {
                this.transform.GetChild(0).position = this.start.transform.position + this.startOffset;
                this.startGraphic.color = new Color(0, 0.5f, 1);
            }
            else
            {
                this.startGraphic.color = new Color(1, 1, 1);
            }

            if (this.end != this.transform.GetChild(1).gameObject)
            {
                this.transform.GetChild(1).position = this.end.transform.position + this.endOffset;
                this.endGraphic.color = new Color(0, 0.5f, 1);
            }
            else
            {
                this.endGraphic.color = new Color(1, 1, 1);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace MapsExtended.Visualizers
{
    public class RopeVisualizer : MonoBehaviour, IMapObjectVisualizer
    {
        private Rope rope;
        private LineRenderer renderer;
        private Graphic startGraphic;
        private Graphic endGraphic;

        public void SetEnabled(bool enabled)
        {
            this.enabled = enabled;
        }

        public void Start()
        {
            this.rope = this.gameObject.GetComponent<Rope>();
        }

        public void OnEnable()
        {
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

            var pos1 = this.rope.GetAnchor(0).GetPosition();
            var pos2 = this.rope.GetAnchor(1).GetPosition();

            this.renderer.SetPositions(new Vector3[] { pos1, pos2 });
            this.startGraphic.transform.position = MainCam.instance.cam.WorldToScreenPoint(pos1);
            this.endGraphic.transform.position = MainCam.instance.cam.WorldToScreenPoint(pos2);

            this.startGraphic.color = this.rope.GetAnchor(0).IsAttached() ? new Color(0, 0.5f, 1) : new Color(1, 1, 1);
            this.endGraphic.color = this.rope.GetAnchor(1).IsAttached() ? new Color(0, 0.5f, 1) : new Color(1, 1, 1);
        }
    }
}

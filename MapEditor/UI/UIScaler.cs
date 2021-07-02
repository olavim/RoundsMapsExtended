using UnityEngine;

namespace MapEditor.UI
{
    class UIScaler : MonoBehaviour
    {
        public GameObject referenceGameObject;
        public float padding;

        public void Awake()
        {
            this.referenceGameObject = null;
            this.padding = 16f;
        }

        public void Start()
        {
            var rt = this.gameObject.GetComponent<RectTransform>() ?? this.gameObject.AddComponent<RectTransform>();
            rt.sizeDelta = Vector2.zero;
        }

        public void Update()
        {
            if (!this.referenceGameObject)
            {
                return;
            }

            var bounds = UIUtils.WorldToScreenRect(EditorUtils.GetMapObjectBounds(this.referenceGameObject));
            bounds.x -= this.padding;
            bounds.y -= this.padding;
            bounds.width += 2 * this.padding;
            bounds.height += 2 * this.padding;

            var rt = this.gameObject.GetComponent<RectTransform>();
            rt.sizeDelta = bounds.size;
            rt.anchoredPosition = bounds.center;
        }
    }
}

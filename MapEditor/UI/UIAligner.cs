using System.Collections.Generic;
using UnityEngine;

namespace MapEditor.UI
{
    class UIAligner : MonoBehaviour
    {
        public GameObject referenceGameObject;
        public float padding;
        public int position;

        public void Awake()
        {
            this.referenceGameObject = null;
            this.padding = 16f;
        }

        public void Start()
        {
            if (!this.gameObject.GetComponent<RectTransform>())
            {
                this.gameObject.AddComponent<RectTransform>();
            }
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

            var sizeMulti = TogglePosition.directionMultipliers[this.position] * 0.5f;

            var rt = this.gameObject.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(
                bounds.center.x + (bounds.width * sizeMulti.x),
                bounds.center.y + (bounds.height * sizeMulti.y)
            );
        }
    }
}

using UnityEngine;

namespace MapsExtended.Editor.UI
{
    class UIAligner : MonoBehaviour
    {
        public GameObject referenceGameObject;
        public float padding;
        public int position;

        private Vector3 prevReferencePosition;
        private Vector3 prevReferenceScale;

        public void Awake()
        {
            this.padding = 16f;
        }

        public void Start()
        {
            if (!this.gameObject.GetComponent<RectTransform>())
            {
                this.gameObject.AddComponent<RectTransform>();
            }

            this.UpdatePosition();
        }

        public void Update()
        {
            this.UpdatePosition();
        }

        public void UpdatePosition()
        {
            if (!this.referenceGameObject)
            {
                return;
            }

            var refPos = this.referenceGameObject.transform.position;
            var refScale = this.referenceGameObject.transform.localScale;

            var bounds = UIUtils.WorldToScreenRect(new Rect(refPos.x - (refScale.x / 2f), refPos.y - (refScale.y / 2f), refScale.x, refScale.y));
            bounds.x -= this.padding;
            bounds.y -= this.padding;
            bounds.width += 2 * this.padding;
            bounds.height += 2 * this.padding;

            if (refPos != this.prevReferencePosition || refScale != this.prevReferenceScale)
            {
                var directionMulti = TogglePosition.directionMultipliers[this.position] * 0.5f;

                var rt = this.gameObject.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(
                    bounds.center.x + (bounds.width * directionMulti.x),
                    bounds.center.y + (bounds.height * directionMulti.y)
                );
                rt.rotation = Quaternion.identity;
            }

            var rotationDelta = this.referenceGameObject.transform.rotation.eulerAngles.z - this.transform.rotation.eulerAngles.z;

            if (rotationDelta != 0)
            {
                this.transform.RotateAround(bounds.center, Vector3.forward, rotationDelta);
            }

            this.prevReferencePosition = refPos;
            this.prevReferenceScale = refScale;
        }
    }
}

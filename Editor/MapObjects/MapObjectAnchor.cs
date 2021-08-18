using System.Linq;
using UnityEngine;

namespace MapsExt.MapObjects
{
    public class MapObjectAnchor : MonoBehaviour
    {
        class Offset
        {
            public float left;
            public float right;
            public float top;
            public float bottom;

            public Offset()
            {
                this.left = 0;
                this.right = 0;
                this.top = 0;
                this.bottom = 0;
            }

            public Offset(float left, float right, float top, float bottom)
            {
                this.left = left;
                this.right = right;
                this.top = top;
                this.bottom = bottom;
            }

            public bool IsCentered()
            {
                return this.left == this.right && this.top == this.bottom;
            }
        }

        private GameObject target;
        private Offset offset;

        private void Awake()
        {
            this.target = this.gameObject;
            this.offset = new Offset();
        }

        private void Update()
        {
            if (this.IsAttached())
            {
                this.transform.position = this.GetPosition();
            }
        }

        public Vector3 GetPosition()
        {
            var pos = this.target.transform.position;
            var targetCollider = this.target.GetComponent<Collider2D>();

            if (targetCollider == null || this.offset.IsCentered())
            {
                return pos;
            }

            var bounds = this.GetIdentityBounds(this.target);

            float x = this.offset.left > this.offset.right ? bounds.min.x + this.offset.right : bounds.max.x - this.offset.left;
            float y = this.offset.bottom > this.offset.top ? bounds.min.y + this.offset.top : bounds.max.y - this.offset.bottom;

            var dir = new Vector3(x - pos.x, y - pos.y, 0);
            dir = this.target.transform.rotation * dir;
            return pos - dir;
        }

        public bool IsAttached()
        {
            return this.target != this.gameObject;
        }

        public void UpdateAttachment()
        {
            var pos = this.GetPosition();
            var colliders = Physics2D.OverlapPointAll(pos);

            var collider =
                colliders.FirstOrDefault(c => c.gameObject.GetComponent<PhysicalMapObjectInstance>() != null && c.gameObject == this.target) ??
                colliders.FirstOrDefault(c => c.gameObject.GetComponent<PhysicalMapObjectInstance>() != null);

            if (collider == null)
            {
                this.Detach();
            }
            else if (this.target != collider.gameObject)
            {
                var rotation = collider.transform.rotation;
                var identityBounds = this.GetIdentityBounds(collider.gameObject);
                var identityPos = this.RotatePointAroundPivot(pos, collider.transform.position, Quaternion.Inverse(rotation));

                float offsetLeft = identityPos.x - identityBounds.min.x;
                float offsetRight = identityBounds.max.x - identityPos.x;
                float offsetBottom = identityPos.y - identityBounds.min.y;
                float offsetTop = identityBounds.max.y - identityPos.y;
                this.offset = new Offset(offsetLeft, offsetRight, offsetTop, offsetBottom);

                this.target = collider.gameObject;
            }
        }

        private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
        {
            var dir = point - pivot;
            dir = rotation * dir;
            point = dir + pivot;
            return point;
        }

        private Bounds GetIdentityBounds(GameObject obj)
        {
            var collider = obj.GetComponent<Collider2D>();

            if (collider == null)
            {
                return new Bounds();
            }

            var rotation = obj.transform.rotation;
            obj.transform.rotation = Quaternion.identity;
            var bounds = collider.bounds;
            obj.transform.transform.rotation = rotation;

            return bounds;
        }

        public void Detach()
        {
            this.target = this.gameObject;
            this.offset = new Offset();
        }
    }
}

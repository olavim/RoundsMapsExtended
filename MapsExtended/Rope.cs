using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapsExtended
{
    [Serializable]
    public class RopeData
    {
        public Vector3 startPosition;
        public Vector3 endPosition;
        public bool active;

        public RopeData(Vector3 startPosition, Vector3 endPosition, bool active)
        {
            this.startPosition = startPosition;
            this.endPosition = endPosition;
            this.active = active;
        }
    }

    public class RopeAnchor : MonoBehaviour
    {
        public GameObject target;
        public Vector3 offset;

        private void Start()
        {
            this.target = this.gameObject;
            this.offset = Vector3.zero;
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
            return this.target.transform.position + this.offset;
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
                colliders.FirstOrDefault(c => c.gameObject.GetComponent<MapObject>() != null && c.gameObject == this.target) ??
                colliders.FirstOrDefault(c => c.gameObject.GetComponent<MapObject>() != null);

            if (collider == null)
            {
                this.Detach();
            }
            else if (this.target != collider.gameObject)
            {
                this.offset = this.target.transform.position - collider.transform.position;
                this.target = collider.gameObject;
            }
        }

        public void Detach()
        {
            this.target = this.gameObject;
            this.offset = Vector3.zero;
        }
    }

    public class Rope : MonoBehaviour
    {
        private List<RopeAnchor> anchors;

        private void Start()
        {
            this.anchors = this.gameObject.GetComponentsInChildren<RopeAnchor>().ToList();
            this.UpdateAttachments();
        }

        public RopeAnchor GetAnchor(int index)
        {
            return this.anchors[index];
        }

        public void UpdateAttachments()
        {
            foreach (var anchor in this.anchors)
            {
                anchor.UpdateAttachment();
            }
        }
    }
}

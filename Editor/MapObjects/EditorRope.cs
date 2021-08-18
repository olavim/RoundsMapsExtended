using MapsExt.MapObjects;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
    [MapsExtendedEditorMapObject(typeof(Rope), "Rope")]
    public class EditorRope : MapObjectSpecification<Rope>
    {
        public override GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Editor Rope");

        protected override void OnDeserialize(Rope data, GameObject target)
        {
            target.transform.GetChild(0).gameObject.GetOrAddComponent<EditorRopeAnchor>();
            target.transform.GetChild(0).gameObject.GetOrAddComponent<RopeActionHandler>();

            target.transform.GetChild(1).gameObject.GetOrAddComponent<EditorRopeAnchor>();
            target.transform.GetChild(1).gameObject.GetOrAddComponent<RopeActionHandler>();

            var instance = target.GetOrAddComponent<EditorRopeInstance>();
            target.GetOrAddComponent<Visualizers.RopeVisualizer>();

            instance.Detach();
            target.transform.GetChild(0).position = data.startPosition;
            target.transform.GetChild(1).position = data.endPosition;
            instance.UpdateAttachments();
        }

        protected override Rope OnSerialize(GameObject instance)
        {
            var ropeInstance = instance.GetComponent<EditorRopeInstance>();
            return new Rope
            {
                startPosition = ropeInstance.GetAnchor(0).GetPosition(),
                endPosition = ropeInstance.GetAnchor(1).GetPosition()
            };
        }
    }

    public class EditorRopeInstance : MonoBehaviour
    {
        private List<EditorRopeAnchor> anchors;

        private void Awake()
        {
            this.anchors = this.gameObject.GetComponentsInChildren<EditorRopeAnchor>().ToList();
        }

        public EditorRopeAnchor GetAnchor(int index)
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

        public void Detach()
        {
            foreach (var anchor in this.anchors)
            {
                anchor.Detach();
            }
        }
    }

    public class EditorRopeAnchor : MonoBehaviour
    {
        private GameObject target;
        private Vector3 offset;

        private void Awake()
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
            var dir = this.offset;
            dir = this.target.transform.rotation * dir;
            return this.target.transform.position - dir;
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
                this.offset = collider.transform.position - this.target.transform.position;
                this.offset = Quaternion.Inverse(collider.transform.rotation) * this.offset;
                this.target = collider.gameObject;
            }
        }

        public void Detach()
        {
            this.target = this.gameObject;
            this.offset = Vector3.zero;
        }
    }
}

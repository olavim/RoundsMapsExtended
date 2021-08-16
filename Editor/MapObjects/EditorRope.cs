using MapsExtended.MapObjects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapsExtended.Editor
{
    [MapsExtendedEditorMapObject(typeof(Rope))]
    public class EditorRope : MapObjectSpecification<Rope>
    {
        public override GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Editor Rope");

        protected override void Deserialize(Rope data, GameObject target)
        {
            target.transform.GetChild(0).position = data.startPosition;
            target.transform.GetChild(1).position = data.endPosition;

            target.transform.GetChild(0).gameObject.AddComponent<EditorRopeAnchor>();
            target.transform.GetChild(0).gameObject.AddComponent<RopeActionHandler>();

            target.transform.GetChild(1).gameObject.AddComponent<EditorRopeAnchor>();
            target.transform.GetChild(1).gameObject.AddComponent<RopeActionHandler>();

            target.AddComponent<EditorRopeInstance>();
            target.AddComponent<Visualizers.RopeVisualizer>();
        }

        protected override Rope Serialize(GameObject instance)
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

        private void Start()
        {
            this.anchors = this.gameObject.GetComponentsInChildren<EditorRopeAnchor>().ToList();
            this.UpdateAttachments();
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
    }

    public class EditorRopeAnchor : MonoBehaviour
    {
        public GameObject target;
        public Vector3 offset;

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
                colliders.FirstOrDefault(c => c.gameObject.GetComponent<PhysicalMapObjectInstance>() != null && c.gameObject == this.target) ??
                colliders.FirstOrDefault(c => c.gameObject.GetComponent<PhysicalMapObjectInstance>() != null);

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
}

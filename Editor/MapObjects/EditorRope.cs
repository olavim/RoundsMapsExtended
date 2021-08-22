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
			target.transform.GetChild(0).gameObject.GetOrAddComponent<MapObjectAnchor>();
			target.transform.GetChild(0).gameObject.GetOrAddComponent<RopeActionHandler>();

			target.transform.GetChild(1).gameObject.GetOrAddComponent<MapObjectAnchor>();
			target.transform.GetChild(1).gameObject.GetOrAddComponent<RopeActionHandler>();

			var startCollider = target.transform.GetChild(0).gameObject.GetOrAddComponent<BoxCollider2D>();
			var endCollider = target.transform.GetChild(1).gameObject.GetOrAddComponent<BoxCollider2D>();
			startCollider.size = Vector2.one * 1;
			endCollider.size = Vector2.one * 1;

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
		private List<MapObjectAnchor> anchors;

		private void Awake()
		{
			this.anchors = this.gameObject.GetComponentsInChildren<MapObjectAnchor>().ToList();
		}

		public MapObjectAnchor GetAnchor(int index)
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
}

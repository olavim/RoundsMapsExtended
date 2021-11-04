using UnityEngine;
using UnboundLib;
using MapsExt.MapObjects;
using System;
using System.Collections;

namespace MapsExt.Editor
{
	public class MapEditorAnimationHandler : MonoBehaviour
	{
		public MapEditor editor;
		private MapObjectAnimation animation;
		private int keyframe;

		private GameObject keyframeMapObject;

		public void Awake()
		{
			this.editor = this.gameObject.GetComponent<MapEditor>();
		}

		public void Update()
		{
			if (this.editor.isSimulating)
			{
				return;
			}
		}

		private void AddAnimation(GameObject go)
		{
			var anim = go.AddComponent<MapObjectAnimation>();

			Action baseChanged = () =>
			{
				var frame = anim.keyframes[0];
				frame.position = go.transform.position;
				frame.scale = go.transform.localScale;
				frame.rotation = go.transform.rotation;
			};

			var baseActionHandler = go.GetComponent<EditorActionHandler>();
			baseActionHandler.onMove += baseChanged;
			baseActionHandler.onResize += baseChanged;
			baseActionHandler.onRotate += baseChanged;

			this.SetAnimation(anim);
		}

		private void SetAnimation(MapObjectAnimation newAnimation)
		{
			if (newAnimation == null)
			{
				this.animation?.gameObject.SetActive(true);
				this.animation = null;
				this.SetKeyframe(-1);
				return;
			}

			this.animation = newAnimation;
			this.animation.gameObject.SetActive(false);

			if (this.animation.keyframes.Count == 0)
			{
				var mapObject = (SpatialMapObject) MapsExtendedEditor.instance.mapObjectManager.Serialize(this.animation.gameObject);
				this.animation.Initialize(mapObject);
			}

			this.SetKeyframe(0);
		}

		private void SetKeyframe(int frameIndex)
		{
			if (this.keyframeMapObject)
			{
				var prevActionHandler = this.keyframeMapObject.GetComponent<EditorActionHandler>();
				prevActionHandler.onMove -= this.HandleKeyframeChanged;
				prevActionHandler.onResize -= this.HandleKeyframeChanged;
				prevActionHandler.onRotate -= this.HandleKeyframeChanged;
				GameObject.Destroy(this.keyframeMapObject);
				this.keyframeMapObject = null;
			}

			if (frameIndex >= 0)
			{
				this.keyframe = frameIndex;
				var frame = this.animation.keyframes[this.keyframe];

				this.SpawnKeyframeMapObject(frame, instance =>
				{
					this.keyframeMapObject = instance;
					var newActionHandler = this.keyframeMapObject.GetComponent<EditorActionHandler>();
					newActionHandler.onMove += this.HandleKeyframeChanged;
					newActionHandler.onResize += this.HandleKeyframeChanged;
					newActionHandler.onRotate += this.HandleKeyframeChanged;
				});
			}
		}

		public void AddKeyframe()
		{
			this.animation.AddKeyframe();
			this.SetKeyframe(this.animation.keyframes.Count - 1);
		}

		private void SpawnKeyframeMapObject(AnimationKeyframe frame, Action<GameObject> cb)
		{
			var frameData = (SpatialMapObject) MapsExtendedEditor.instance.mapObjectManager.Serialize(this.animation.gameObject);
			frameData.active = true;
			frameData.position = frame.position;
			frameData.scale = frame.scale;
			frameData.rotation = frame.rotation;

			MapsExtendedEditor.instance.SpawnObject(this.editor.content, frameData, cb);
		}

		private void HandleKeyframeChanged()
		{
			var frame = this.animation.keyframes[this.keyframe];
			frame.position = this.keyframeMapObject.transform.position;
			frame.scale = this.keyframeMapObject.transform.localScale;
			frame.rotation = this.keyframeMapObject.transform.rotation;
		}

		public void OnGUI()
		{
			if (this.editor.isSimulating)
			{
				return;
			}

			GUI.Window(0, new Rect(10, 30, 200, 500), this.BuildAnimationWindow, "Animation");
		}

		private void BuildAnimationWindow(int windowId)
		{
			GUI.enabled =
				this.animation == null &&
				editor.selectedMapObjects.Count == 1 &&
				editor.selectedMapObjects[0].GetComponent<SpatialMapObjectInstance>() != null;

			if (GUI.Button(new Rect(10, 40, 100, 20), "Animate"))
			{
				if (editor.selectedMapObjects[0].GetComponent<MapObjectAnimation>())
				{
					this.SetAnimation(editor.selectedMapObjects[0].GetComponent<MapObjectAnimation>());
				}
				else
				{
					this.AddAnimation(editor.selectedMapObjects[0]);
				}
			}

			GUI.enabled = this.animation != null;

			if (GUI.Button(new Rect(10, 70, 100, 20), "Stop Animating"))
			{
				this.SetAnimation(null);
			}

			if (GUI.Button(new Rect(10, 100, 100, 20), "Add Keyframe"))
			{
				this.AddKeyframe();
			}

			for (int i = 0; i < (this.animation?.keyframes.Count ?? 0); i++)
			{
				if (GUI.Button(new Rect(10, 130 + (30 * i), 100, 20), $"Keyframe {i + 1}"))
				{
					this.SetKeyframe(i);
				}
			}

			GUI.enabled = true;
		}
	}
}

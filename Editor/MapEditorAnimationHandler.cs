using UnityEngine;
using MapsExt.MapObjects;
using System;
using System.Collections.Generic;
using UnboundLib;

namespace MapsExt.Editor
{
	public class MapEditorAnimationHandler : MonoBehaviour
	{
		public MapEditor editor;
		private MapObjectAnimation animation;
		private int keyframe;

		private GameObject keyframeMapObject;
		private LineRenderer traceLine;

		public void Awake()
		{
			this.editor = this.gameObject.GetComponent<MapEditor>();

			var traceLineGo = new GameObject("Animation Trace Line");
			traceLineGo.transform.SetParent(this.transform);
			this.traceLine = traceLineGo.AddComponent<LineRenderer>();
			this.traceLine.material = new Material(Shader.Find("Sprites/Default"));
			this.traceLine.widthMultiplier = 0.15f;
			this.traceLine.numCornerVertices = 4;
			this.traceLine.startColor = new Color(1f, 1f, 1f, 0.01f);
			this.traceLine.endColor = this.traceLine.startColor;
			this.traceLine.loop = false;
		}

		public void Update()
		{
			if (this.editor.isSimulating)
			{
				return;
			}

			this.UpdateTraceLine();
		}

		private void AddAnimation(GameObject go)
		{
			go.SetActive(false);
			var anim = go.AddComponent<MapObjectAnimation>();
			anim.playOnAwake = false;
			go.SetActive(true);

			Action baseChanged = () =>
			{
				var frame = anim.keyframes[0];
				frame.position = go.transform.position;
				frame.scale = go.transform.localScale;
				frame.rotation = go.transform.rotation;
			};

			var baseActionHandler = go.GetComponent<EditorActionHandler>();
			baseActionHandler.onAction += baseChanged;

			this.SetAnimation(anim);
		}

		private void SetAnimation(MapObjectAnimation newAnimation)
		{
			if (newAnimation == null)
			{
				var baseObject = this.animation?.gameObject;
				if (baseObject)
				{
					var firstFrame = this.animation.keyframes[0];
					baseObject.transform.position = firstFrame.position;
					baseObject.transform.localScale = firstFrame.scale;
					baseObject.transform.rotation = firstFrame.rotation;
					baseObject.SetActive(true);
				}

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
				prevActionHandler.onAction -= this.HandleKeyframeChanged;
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
					newActionHandler.onAction += this.HandleKeyframeChanged;
				});
			}

			this.UpdateTraceLine();
		}

		public void AddKeyframe()
		{
			this.animation.AddKeyframe();
			this.SetKeyframe(this.animation.keyframes.Count - 1);
		}

		private void UpdateTraceLine()
		{
			if (this.animation == null || this.keyframe <= 0)
			{
				this.traceLine.positionCount = 0;
				this.traceLine.SetPositions(new Vector3[] { });
			}
			else
			{
				var positions = new List<Vector3>();
				for (int i = 0; i <= this.keyframe; i++)
				{
					if (i == this.keyframe)
					{
						positions.Add(this.keyframeMapObject.transform.position);
					}
					else
					{
						positions.Add(this.animation.keyframes[i].position);
					}
				}

				this.traceLine.positionCount = positions.Count;
				this.traceLine.SetPositions(positions.ToArray());
			}
		}

		private void SpawnKeyframeMapObject(AnimationKeyframe frame, Action<GameObject> cb)
		{
			var frameData = (SpatialMapObject) MapsExtendedEditor.instance.mapObjectManager.Serialize(this.animation.gameObject);
			frameData.active = true;
			frameData.position = frame.position;
			frameData.scale = frame.scale;
			frameData.rotation = frame.rotation;
			frameData.animationKeyframes.Clear();

			MapsExtendedEditor.instance.SpawnObject(this.editor.content, frameData, cb);
		}

		private void HandleKeyframeChanged()
		{
			var frame = this.animation.keyframes[this.keyframe];
			frame.position = this.keyframeMapObject.transform.position;
			frame.scale = this.keyframeMapObject.transform.localScale;
			frame.rotation = this.keyframeMapObject.transform.rotation;
			this.UpdateTraceLine();
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

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using MapsExt.MapObjects;
using System;
using System.Collections.Generic;
using MapsExt.UI;

namespace MapsExt.Editor
{
	public class MapEditorAnimationHandler : MonoBehaviour
	{
		public MapEditor editor;
		public MapObjectAnimation animation;
		public GameObject keyframeMapObject;
		public Action onAnimationChanged;

		public int Keyframe { get; private set; }

		private SmoothLineRenderer lineRenderer;
		private int prevKeyframe = -1;
		private GameObject curtain;

		public void Awake()
		{
			this.lineRenderer = this.gameObject.AddComponent<SmoothLineRenderer>();

			this.curtain = new GameObject("Curtain");
			this.curtain.transform.SetParent(this.transform);
			this.curtain.SetActive(false);

			var canvas = this.curtain.AddComponent<Canvas>();
			canvas.sortingLayerID = SortingLayer.NameToID("MapParticle");
			canvas.sortingOrder = 10;

			var renderer = this.curtain.GetComponent<CanvasRenderer>();
			var image = this.curtain.AddComponent<Image>();
			image.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
			image.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
		}

		public void OnEnable()
		{
			if (this.animation == null)
			{
				return;
			}

			this.animation.gameObject.SetActive(false);
			this.curtain.SetActive(true);
			this.SetKeyframe(this.prevKeyframe);
		}

		public void OnDisable()
		{
			if (this.animation == null)
			{
				return;
			}

			this.curtain.SetActive(false);

			var baseObject = this.animation.gameObject;
			var firstFrame = this.animation.keyframes[0];
			baseObject.transform.position = firstFrame.position;
			baseObject.transform.localScale = firstFrame.scale;
			baseObject.transform.rotation = firstFrame.rotation;
			baseObject.SetActive(true);

			this.prevKeyframe = this.Keyframe;

			this.SetKeyframe(-1);
		}

		public void Update()
		{
			if (!this.editor || this.editor.isSimulating)
			{
				return;
			}

			this.UpdateTraceLine();
		}

		public void AddAnimation(GameObject go)
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

		public void SetAnimation(MapObjectAnimation newAnimation)
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

				this.curtain.SetActive(false);
				this.animation = null;
				this.SetKeyframe(-1);
				return;
			}

			this.animation = newAnimation;
			this.animation.gameObject.SetActive(false);
			this.curtain.SetActive(true);

			if (this.animation.keyframes.Count == 0)
			{
				var mapObject = (SpatialMapObject) MapsExtendedEditor.instance.mapObjectManager.Serialize(this.animation.gameObject);
				this.animation.Initialize(mapObject);
			}

			this.SetKeyframe(0);
			this.onAnimationChanged?.Invoke();
		}

		public void SetKeyframe(int frameIndex)
		{
			if (this.keyframeMapObject)
			{
				var prevActionHandler = this.keyframeMapObject.GetComponent<EditorActionHandler>();
				prevActionHandler.onAction -= this.HandleKeyframeChanged;
				GameObject.Destroy(this.keyframeMapObject);
				this.keyframeMapObject = null;
			}

			this.editor.ClearSelected();
			this.Keyframe = frameIndex;

			if (this.Keyframe >= 0)
			{
				var frame = this.animation.keyframes[this.Keyframe];

				this.SpawnKeyframeMapObject(frame, instance =>
				{
					this.keyframeMapObject = instance;
					var newActionHandler = this.keyframeMapObject.GetComponent<EditorActionHandler>();
					newActionHandler.onAction += this.HandleKeyframeChanged;

					foreach (var mask in instance.GetComponentsInChildren<SpriteMask>())
					{
						mask.backSortingOrder = 20;
						mask.frontSortingOrder = 21;
					}

					this.keyframeMapObject.transform.SetAsFirstSibling();
					this.editor.AddSelected(this.keyframeMapObject);
				});
			}

			this.UpdateTraceLine();
		}

		public void AddKeyframe()
		{
			this.animation.AddKeyframe();
			this.SetKeyframe(this.animation.keyframes.Count - 1);
		}

		public void DeleteKeyframe(int index)
		{
			this.animation.DeleteKeyframe(index);
			this.SetKeyframe(Mathf.Min(index + 1, this.animation.keyframes.Count - 1));
		}

		private void UpdateTraceLine()
		{
			var points = new List<Vector3>();

			if (this.animation != null && this.Keyframe > 0)
			{
				for (int i = 0; i <= this.Keyframe; i++)
				{
					points.Add(i == this.Keyframe ? this.keyframeMapObject.transform.position : this.animation.keyframes[i].position);
				}
			}

			this.lineRenderer.SetPositions(points);
		}

		private void SpawnKeyframeMapObject(AnimationKeyframe frame, Action<GameObject> cb)
		{
			var frameData = (SpatialMapObject) MapsExtendedEditor.instance.mapObjectManager.Serialize(this.animation.gameObject);
			frameData.active = true;
			frameData.position = frame.position;
			frameData.scale = frame.scale;
			frameData.rotation = frame.rotation;
			frameData.animationKeyframes.Clear();

			MapsExtendedEditor.instance.SpawnObject(this.gameObject, frameData, cb);
		}

		private void HandleKeyframeChanged()
		{
			var frame = this.animation.keyframes[this.Keyframe];
			frame.position = this.keyframeMapObject.transform.position;
			frame.scale = this.keyframeMapObject.transform.localScale;
			frame.rotation = this.keyframeMapObject.transform.rotation;
			this.UpdateTraceLine();
		}

		public void RefreshCurrentFrame()
		{
			var frame = this.animation.keyframes[this.Keyframe];
			this.keyframeMapObject.transform.position = frame.position;
			this.keyframeMapObject.transform.localScale = frame.scale;
			this.keyframeMapObject.transform.rotation = frame.rotation;
		}
	}
}

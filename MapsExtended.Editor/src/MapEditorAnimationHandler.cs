using UnityEngine;
using UnityEngine.UI;
using MapsExt.Editor.ActionHandlers;
using System;
using System.Collections.Generic;
using MapsExt.Editor.UI;
using MapsExt.MapObjects.Properties;
using System.Linq;
using Sirenix.Utilities;

namespace MapsExt.Editor
{
	public class MapEditorAnimationHandler : MonoBehaviour
	{
		public MapEditor editor;
		public MapObjectAnimation animation;
		public GameObject keyframeMapObject;
		public Action onAnimationChanged;
		public Camera animationCamera;

		public int KeyframeIndex { get; private set; }

		public AnimationKeyframe Keyframe => this.animation && this.KeyframeIndex >= 0 && this.KeyframeIndex < this.animation.Keyframes.Count
			? this.animation.Keyframes[this.KeyframeIndex]
			: null;

		private SmoothLineRenderer lineRenderer;
		private int prevKeyframe = -1;

		private GameObject curtain;
		private GameObject particles;

		protected virtual void Awake()
		{
			this.SetupLayerCurtain();

			this.lineRenderer = this.gameObject.AddComponent<SmoothLineRenderer>();
			this.lineRenderer.Renderer.sortingLayerID = SortingLayer.NameToID("MostFront");
			this.lineRenderer.Renderer.sortingOrder = 9;
		}

		/* Creates a transparent image that "separates" the bottom and top layers. The bottom layer (and curtain)
		 * is rendered by the main camera, while the top layer is rendered by a second camera.
		 */
		private void SetupLayerCurtain()
		{
			this.curtain = new GameObject("Curtain");
			this.curtain.transform.SetParent(this.transform);
			this.curtain.SetActive(false);

			var canvas = this.curtain.AddComponent<Canvas>();
			canvas.sortingLayerID = SortingLayer.NameToID("MostFront");
			canvas.sortingOrder = 10;

			var image = this.curtain.AddComponent<Image>();
			image.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
			image.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
		}

		protected virtual void OnEnable()
		{
			if (this.animation == null)
			{
				return;
			}

			this.RefreshParticles();

			this.animation.gameObject.SetActive(false);
			this.curtain.SetActive(true);
			this.SetKeyframe(this.prevKeyframe);
		}

		protected virtual void OnDisable()
		{
			if (this.animation == null)
			{
				return;
			}

			this.RefreshParticles();
			this.curtain.SetActive(false);

			var baseObject = this.animation.gameObject;
			var firstFrame = this.animation.Keyframes[0];

			for (int i = 0; i < firstFrame.ComponentValues.Count; i++)
			{
				var value = firstFrame.ComponentValues[i];
				MapsExtendedEditor.instance.propertyManager.GetSerializer(value.GetType()).Deserialize(value, baseObject);
			}

			baseObject.SetActive(true);

			this.prevKeyframe = this.KeyframeIndex;
			this.SetKeyframe(-1);
		}

		protected virtual void Update()
		{
			if (!this.editor || this.editor.IsSimulating || !this.animation)
			{
				return;
			}

			this.editor.ActiveObject = this.keyframeMapObject;

			this.Refresh();
		}

		public void Refresh()
		{
			if (!this.animation)
			{
				return;
			}

			if (this.animation.gameObject.activeSelf)
			{
				this.animation.gameObject.SetActive(false);
			}

			if (this.KeyframeIndex >= this.animation.Keyframes.Count)
			{
				this.SetKeyframe(this.animation.Keyframes.Count - 1);
			}

			this.RefreshKeyframeMapObject();
			this.UpdateTraceLine();
		}

		/* Many map objects (such as ground) show a particle effect background. Since our
		 * second camera can only render stuff on a specific layer, we need to duplicate the
		 * particle rendering stuff on it.
		 */
		public void RefreshParticles()
		{
			if (this.particles)
			{
				GameObjectUtils.DestroyImmediateSafe(this.particles);
			}

			this.particles = GameObject.Instantiate(MapsExtendedEditor.instance.frontParticles, Vector3.zero, Quaternion.identity, this.transform);

			foreach (var p in this.particles.GetComponentsInChildren<ParticleSystem>())
			{
				// Render these particles on the second camera's layer
				p.gameObject.layer = MapsExtendedEditor.LAYER_ANIMATION_MAPOBJECT;
				p.Play();
			}
		}

		public void AddAnimation(GameObject go)
		{
			if (go.GetComponent<MapObjectAnimation>() != null)
			{
				throw new ArgumentException($"{nameof(MapObjectAnimation)} already exists on {go.name}");
			}

			go.SetActive(false);
			var anim = go.AddComponent<MapObjectAnimation>();

			anim.PlayOnAwake = false;
			go.SetActive(true);
			this.SetAnimation(anim);
		}

		public void SetAnimation(MapObjectAnimation newAnimation)
		{
			if (newAnimation == this.animation)
			{
				return;
			}

			if (newAnimation == null)
			{
				this.animation.gameObject.SetActive(true);
				this.curtain.SetActive(false);
				this.animation = null;
				this.SetKeyframe(0);
				this.RefreshParticles();
				return;
			}

			this.animation = newAnimation;
			this.animation.gameObject.SetActive(false);
			this.curtain.SetActive(true);
			this.RefreshParticles();

			var data = MapsExtendedEditor.instance.mapObjectManager.Serialize(this.animation.gameObject);
			var animatableProperties = MapsExtendedEditor.instance.propertyManager.GetProperties<ILinearProperty>(data);

			if (this.animation.Keyframes.Count == 0)
			{
				this.animation.Initialize(new AnimationKeyframe(animatableProperties));
			}

			this.animation.Keyframes[0].ComponentValues = animatableProperties.ToList();

			this.SetKeyframe(0);
			this.onAnimationChanged?.Invoke();
		}

		public void AddKeyframe()
		{
			var newFrame = new AnimationKeyframe(this.animation.Keyframes.Last());
			int frameIndex = this.animation.Keyframes.Count;

			if (this.animation.Keyframes.Count == 0)
			{
				var data = MapsExtendedEditor.instance.mapObjectManager.Serialize(this.animation.gameObject);
				var animatableProperties = MapsExtendedEditor.instance.propertyManager.GetProperties<ILinearProperty>(data);

				this.animation.PlayOnAwake = false;
				this.animation.Initialize(new AnimationKeyframe(animatableProperties));
			}

			this.animation.Keyframes.Insert(frameIndex, newFrame);
			this.editor.animationHandler.SetKeyframe(frameIndex);
			this.editor.TakeSnaphot();
		}

		public void DeleteKeyframe(int index)
		{
			this.animation.Keyframes.RemoveAt(index);

			if (this.editor.animationHandler.KeyframeIndex >= this.animation.Keyframes.Count)
			{
				this.editor.animationHandler.SetKeyframe(this.animation.Keyframes.Count - 1);
			}

			this.editor.TakeSnaphot();
		}

		public void ToggleAnimation(GameObject target)
		{
			if (this.animation)
			{
				this.SetAnimation(null);
			}
			else if (target.GetComponent<MapObjectAnimation>())
			{
				this.SetAnimation(target.GetComponent<MapObjectAnimation>());
			}
			else
			{
				this.AddAnimation(target);
			}
		}

		public void SetKeyframe(int frameIndex)
		{
			if (this.keyframeMapObject)
			{
				GameObjectUtils.DestroyImmediateSafe(this.keyframeMapObject);
				this.keyframeMapObject = null;
			}

			this.KeyframeIndex = frameIndex;

			if (this.Keyframe != null)
			{
				this.SpawnKeyframeMapObject(this.Keyframe, instance =>
				{
					/* To show the map object on top of the curtain, set its layer so that
					 * it's only rendered by the second camera.
					 */
					foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
					{
						renderer.gameObject.layer = MapsExtendedEditor.LAYER_ANIMATION_MAPOBJECT;
					}

					foreach (var handler in instance.GetComponentsInChildren<ActionHandler>())
					{
						handler.OnChange += () =>
						{
							var data = MapsExtendedEditor.instance.mapObjectManager.Serialize(instance);
							var animatableProperties = MapsExtendedEditor.instance.propertyManager.GetProperties<ILinearProperty>(data);

							this.animation.Keyframes[frameIndex].ComponentValues = animatableProperties.ToList();
							this.Refresh();
						};
					}

					instance.transform.SetAsLastSibling();

					this.keyframeMapObject = instance;
					this.editor.ClearSelected();
					this.editor.AddSelected(instance);
				});
			}
			else
			{
				this.editor.ClearSelected();
			}

			this.UpdateTraceLine();
		}

		private void UpdateTraceLine()
		{
			var points = new List<Vector3>();

			if (this.animation != null && this.KeyframeIndex > 0)
			{
				for (int i = 0; i <= this.KeyframeIndex; i++)
				{
					points.Add(
						i == this.KeyframeIndex
							? this.keyframeMapObject.transform.position
							: (Vector3) (this.animation.Keyframes[i].GetComponentValue<PositionProperty>() ?? new PositionProperty())
					);
				}
			}

			this.lineRenderer.SetPositions(points);
		}

		private void SpawnKeyframeMapObject(AnimationKeyframe frame, Action<GameObject> cb)
		{
			var frameData = MapsExtendedEditor.instance.mapObjectManager.Serialize(this.animation.gameObject);
			frameData.mapObjectId = $"{frameData.mapObjectId}:keyframeMapObject";
			frameData.active = true;

			var animatableProperties = MapsExtendedEditor.instance.propertyManager.GetProperties<ILinearProperty>(frameData);
			var newAnimationProperty = new AnimationProperty(animatableProperties);
			MapsExtendedEditor.instance.propertyManager.SetProperty(frameData, newAnimationProperty);

			MapsExtendedEditor.instance.SpawnObject(this.gameObject, frameData, instance =>
			{
				System.Diagnostics.Debug.Assert(
					instance.GetComponent<MapObjectAnimation>() == null,
					$"Spawned object should not have a {nameof(MapObjectAnimation)} component"
				);

				for (int i = 0; i < this.animation.Keyframes[0].ComponentValues.Count; i++)
				{
					var value = frame.ComponentValues[i];
					var serializer = MapsExtendedEditor.instance.propertyManager.GetSerializer(value.GetType());
					serializer.Deserialize(value, instance);
				}

				cb(instance);
			});
		}

		public void RefreshKeyframeMapObject()
		{
			if (this.keyframeMapObject == null || this.Keyframe == null)
			{
				return;
			}

			for (int i = 0; i < this.animation.Keyframes[0].ComponentValues.Count; i++)
			{
				var baseFrameValue = this.animation.Keyframes[0].ComponentValues[i];
				var currentFrameValue = this.Keyframe.ComponentValues[i];

				var serializer = MapsExtendedEditor.instance.propertyManager.GetSerializer(baseFrameValue.GetType());
				serializer.Deserialize(baseFrameValue, this.animation.gameObject);
				serializer.Deserialize(currentFrameValue, this.keyframeMapObject);
			}
		}
	}
}

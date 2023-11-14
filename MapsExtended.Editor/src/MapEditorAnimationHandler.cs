using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using MapsExt.Editor.UI;
using MapsExt.Properties;
using System.Linq;
using UnityEngine.Rendering.PostProcessing;
using UnboundLib;
using Sirenix.Utilities;
using MapsExt.Utils;

namespace MapsExt.Editor
{
	public class MapEditorAnimationHandler : MonoBehaviour
	{
		[SerializeField] private MapEditor _editor;
		[SerializeField] private Camera _animationCamera;
		private SmoothLineRenderer _lineRenderer;
		private GameObject _curtain;
		private int _prevKeyframe = -1;
		private GameObject _particles;
		private GameObject _keyframeMapObject;
		private GameObject _postProcessCamera;

		public MapEditor Editor { get => this._editor; set => this._editor = value; }
		public Camera AnimationCamera { get => this._animationCamera; set => this._animationCamera = value; }
		public SmoothLineRenderer LineRenderer { get => this._lineRenderer; set => this._lineRenderer = value; }

		public MapObjectAnimation Animation { get; private set; }
		public GameObject KeyframeMapObject { get => this._keyframeMapObject; private set => this._keyframeMapObject = value; }
		public int KeyframeIndex { get; private set; }

		public AnimationKeyframe Keyframe => this.Animation && this.KeyframeIndex >= 0 && this.KeyframeIndex < this.Animation.Keyframes.Count
			? this.Animation.Keyframes[this.KeyframeIndex]
			: null;

		protected virtual void Awake()
		{
			this.SetupLayerCurtain();
			this.SetupPostProcessing();

			this._lineRenderer = this.gameObject.AddComponent<SmoothLineRenderer>();
			this._lineRenderer.Renderer.sortingLayerID = SortingLayer.NameToID("MostFront");
			this._lineRenderer.Renderer.sortingOrder = 9;
		}

		private void SetupPostProcessing()
		{
			this._postProcessCamera = new GameObject("PostProcessCamera");
			this._postProcessCamera.transform.SetParent(this.transform);

			var camera = this._postProcessCamera.AddComponent<Camera>();
			camera.CopyFrom(MainCam.instance.cam);
			camera.depth = 2;
			camera.cullingMask = 0; // Render nothing, only apply post-processing fx

			var layer = this._postProcessCamera.AddComponent<PostProcessLayer>();
			layer.Init((PostProcessResources) MainCam.instance.gameObject.GetComponent<PostProcessLayer>().GetFieldValue("m_Resources"));
			layer.volumeTrigger = this._postProcessCamera.transform;
			layer.volumeLayer = 1 << LayerMask.NameToLayer("Default Post");
			layer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
		}

		/* Creates a transparent image that "separates" the bottom and top layers. The bottom layer (and curtain)
		 * is rendered by the main camera, while the top layer is rendered by a second camera.
		 */
		private void SetupLayerCurtain()
		{
			this._curtain = new GameObject("Curtain");
			this._curtain.transform.SetParent(this.transform);
			this._curtain.SetActive(false);

			var canvas = this._curtain.AddComponent<Canvas>();
			canvas.sortingLayerID = SortingLayer.NameToID("MostFront");
			canvas.sortingOrder = 10;

			var image = this._curtain.AddComponent<Image>();
			image.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
			image.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
		}

		protected virtual void OnEnable()
		{
			MainCam.instance.gameObject.GetComponent<PostProcessLayer>().enabled = false;
			this._postProcessCamera.SetActive(true);

			if (this.Animation == null)
			{
				return;
			}

			this.RefreshParticles();

			this.Animation.gameObject.SetActive(false);
			this._curtain.SetActive(true);
			this.SetKeyframe(this._prevKeyframe);
		}

		protected virtual void OnDisable()
		{
			this._postProcessCamera.SetActive(false);
			MainCam.instance.gameObject.GetComponent<PostProcessLayer>().enabled = true;

			if (this.Animation == null)
			{
				return;
			}

			this.RefreshParticles();
			this._curtain.SetActive(false);

			var baseObject = this.Animation.gameObject;
			var firstFrame = this.Animation.Keyframes[0];

			for (int i = 0; i < firstFrame.ComponentValues.Count; i++)
			{
				baseObject.WriteProperty(firstFrame.ComponentValues[i]);
			}

			baseObject.SetActive(true);

			this._prevKeyframe = this.KeyframeIndex;
			this.SetKeyframe(-1);
		}

		protected virtual void Update()
		{
			if (!this.Editor || this.Editor.IsSimulating || !this.Animation)
			{
				return;
			}

			this.Refresh();
		}

		public void Refresh()
		{
			if (!this.Animation)
			{
				return;
			}

			if (this.Animation.gameObject.activeSelf)
			{
				this.Animation.gameObject.SetActive(false);
			}

			if (this.KeyframeIndex >= this.Animation.Keyframes.Count)
			{
				this.SetKeyframe(this.Animation.Keyframes.Count - 1);
			}

			this.RefreshKeyframeValues();
			this.UpdateTraceLine();
		}

		/* Many map objects (such as ground) show a particle effect background. Since our
		 * second camera can only render stuff on a specific layer, we need to duplicate the
		 * particle rendering stuff on it.
		 */
		private void RefreshParticles()
		{
			if (this._particles)
			{
				MapsExt.Utils.GameObjectUtils.DestroyImmediateSafe(this._particles);
			}

			var defaultParticles = GameObject.Find("/Game/Visual/Rendering /FrontParticles");
			this._particles = GameObject.Instantiate(defaultParticles, Vector3.zero, Quaternion.identity, this.transform);

			foreach (var p in this._particles.GetComponentsInChildren<ParticleSystem>())
			{
				// Render these particles on the second camera's layer
				p.gameObject.layer = MapsExtendedEditor.MapObjectAnimationLayer;
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
			if (newAnimation == this.Animation)
			{
				return;
			}

			if (newAnimation == null)
			{
				this.Animation.gameObject.SetActive(true);
				this._curtain.SetActive(false);
				this.Editor.OverrideActiveMapObject(null);
				this.Editor.ClearSelected();
				this.Editor.AddSelected(this.Animation.gameObject);
				this.Animation = null;
				this.SetKeyframe(-1);
				this.RefreshParticles();
				return;
			}

			this.Animation = newAnimation;
			this.Editor.ClearSelected();
			this.Animation.gameObject.SetActive(false);
			this._curtain.SetActive(true);
			this.RefreshParticles();

			var animatableProperties = this.Animation.ReadMapObject().GetProperties<ILinearProperty>();

			if (this.Animation.Keyframes.Count == 0)
			{
				this.Animation.Initialize(new AnimationKeyframe(animatableProperties));
			}

			this.Animation.Keyframes[0].ComponentValues = animatableProperties.ToList();

			this.SetKeyframe(0);
		}

		public void AddKeyframe()
		{
			var newFrame = new AnimationKeyframe(this.Animation.Keyframes.Last());
			int frameIndex = this.Animation.Keyframes.Count;

			if (this.Animation.Keyframes.Count == 0)
			{
				var animatableProperties = this.Animation.ReadMapObject().GetProperties<ILinearProperty>();

				this.Animation.PlayOnAwake = false;
				this.Animation.Initialize(new AnimationKeyframe(animatableProperties));
			}

			this.Animation.Keyframes.Insert(frameIndex, newFrame);
			this.Editor.AnimationHandler.SetKeyframe(frameIndex);
			this.Editor.TakeSnaphot();
		}

		public void DeleteKeyframe(int index)
		{
			this.Animation.Keyframes.RemoveAt(index);

			if (this.Editor.AnimationHandler.KeyframeIndex >= this.Animation.Keyframes.Count)
			{
				this.Editor.AnimationHandler.SetKeyframe(this.Animation.Keyframes.Count - 1);
			}

			this.Editor.TakeSnaphot();
		}

		public void ToggleAnimation(GameObject target)
		{
			if (this.Animation)
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
			if (this.KeyframeMapObject)
			{
				MapsExt.Utils.GameObjectUtils.DestroyImmediateSafe(this.KeyframeMapObject);
				this.KeyframeMapObject = null;
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
						renderer.gameObject.layer = MapsExtendedEditor.MapObjectAnimationLayer;
					}

					instance.transform.SetAsLastSibling();

					this.KeyframeMapObject = instance;
					this.Editor.ClearSelected();
					this.Editor.AddSelected(instance);
					this.Editor.OverrideActiveMapObject(instance);
				});
			}

			this.UpdateTraceLine();
		}

		private void UpdateTraceLine()
		{
			var points = new List<Vector3>();

			if (this.Animation != null && this.KeyframeIndex > 0)
			{
				for (int i = 0; i <= this.KeyframeIndex; i++)
				{
					points.Add(
						i == this.KeyframeIndex
							? this.KeyframeMapObject.transform.position
							: (Vector3) (this.Animation.Keyframes[i].GetComponentValue<PositionProperty>() ?? new PositionProperty())
					);
				}
			}

			this._lineRenderer.SetPositions(points);
		}

		private void SpawnKeyframeMapObject(AnimationKeyframe frame, Action<GameObject> cb)
		{
			var frameData = this.Animation.ReadMapObject();
			frameData.MapObjectId = $"{frameData.MapObjectId}:keyframeMapObject";
			frameData.Active = true;
			frameData.SetProperty(new AnimationProperty());

			MapsExtendedEditor.MapObjectManager.Instantiate(frameData, this.transform, instance =>
			{
				System.Diagnostics.Debug.Assert(
					instance.GetComponent<MapObjectAnimation>() == null,
					$"Spawned object should not have a {nameof(MapObjectAnimation)} component"
				);

				for (int i = 0; i < this.Animation.Keyframes[0].ComponentValues.Count; i++)
				{
					instance.WriteProperty(frame.ComponentValues[i]);
				}

				cb(instance);
			});
		}

		private void RefreshKeyframeValues()
		{
			if (this.KeyframeMapObject == null || this.Keyframe == null)
			{
				return;
			}

			var animatableProperties = this.KeyframeMapObject.ReadMapObject().GetProperties<ILinearProperty>();
			this.Keyframe.ComponentValues = animatableProperties.ToList();

			if (this.KeyframeIndex == 0)
			{
				this.RefreshBaseMapObject();
			}
		}

		private void RefreshBaseMapObject()
		{
			if (this.Animation?.Keyframes.IsNullOrEmpty() != false)
			{
				return;
			}

			for (int i = 0; i < this.Animation.Keyframes[0].ComponentValues.Count; i++)
			{
				this.Animation.WriteProperty(this.Animation.Keyframes[0].ComponentValues[i]);
			}
		}
	}
}

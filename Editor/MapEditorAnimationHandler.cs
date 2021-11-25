using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using MapsExt.MapObjects;
using System;
using System.Collections.Generic;
using MapsExt.UI;

namespace MapsExt.Editor
{
	public class MapEditorAnimationHandler : MonoBehaviour
	{
		private const int POSTPROCESS_LAYER = 31;
		private const int MAPOBJECT_LAYER = 31;

		public MapEditor editor;
		public MapObjectAnimation animation;
		public GameObject keyframeMapObject;
		public Action onAnimationChanged;

		public int Keyframe { get; private set; }

		private SmoothLineRenderer lineRenderer;
		private int prevKeyframe = -1;

		private GameObject curtain;
		private Camera animationCamera;
		private GameObject particles;

		public void Awake()
		{
			this.SetupLayerCamera();
			this.SetupLayerCurtain();

			this.lineRenderer = this.gameObject.AddComponent<SmoothLineRenderer>();
			this.lineRenderer.Renderer.sortingLayerID = SortingLayer.NameToID("MostFront");
			this.lineRenderer.Renderer.sortingOrder = 9;
		}

		// Creates a transparent image that "separates" the bottom and top layers
		private void SetupLayerCurtain()
		{
			this.curtain = new GameObject("Curtain");
			this.curtain.transform.SetParent(this.transform);
			this.curtain.SetActive(false);

			var canvas = this.curtain.AddComponent<Canvas>();
			canvas.sortingLayerID = SortingLayer.NameToID("MostFront");
			canvas.sortingOrder = 10;

			var renderer = this.curtain.GetComponent<CanvasRenderer>();
			var image = this.curtain.AddComponent<Image>();
			image.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
			image.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
		}

		/* The bottom layer (and curtain) is rendered by the main camera, while the top layer
		 * is rendered by a second camera.
		 */
		private void SetupLayerCamera()
		{
			var cameraGo = new GameObject("Animation Camera");
			cameraGo.transform.SetParent(this.transform);
			this.animationCamera = cameraGo.AddComponent<Camera>();
			this.animationCamera.CopyFrom(MainCam.instance.cam);
			this.animationCamera.cullingMask = (1 << MAPOBJECT_LAYER);
			this.animationCamera.depth = 2;
			MainCam.instance.cam.cullingMask = MainCam.instance.cam.cullingMask & ~(1 << MAPOBJECT_LAYER);

			cameraGo.AddComponent<CameraZoomHandler>();
		}

		public void OnEnable()
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

		public void OnDisable()
		{
			if (this.animation == null)
			{
				return;
			}

			this.RefreshParticles();
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

		/* Many map objects (such as ground) show a particle effect background. Since our
		 * second camera can only render stuff on a specific layer, we need to duplicate the
		 * particle rendering stuff on it.
		 */
		public void RefreshParticles()
		{
			if (this.particles)
			{
				GameObject.Destroy(this.particles);
			}

			this.particles = GameObject.Instantiate(MapsExtendedEditor.instance.frontParticles, Vector3.zero, Quaternion.identity, this.transform);

			foreach (var p in this.particles.GetComponentsInChildren<ParticleSystem>())
			{
				// Render these particles on the second camera's layer
				p.gameObject.layer = MAPOBJECT_LAYER;
				p.Play();
			}
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
				this.RefreshParticles();
				return;
			}

			this.animation = newAnimation;
			this.animation.gameObject.SetActive(false);
			this.curtain.SetActive(true);
			this.RefreshParticles();

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

					/* To show the map object on top of the curtain, set its layer so that
					 * it's only rendered by the second camera.
					 */
					foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
					{
						renderer.gameObject.layer = MAPOBJECT_LAYER;
					}

					this.keyframeMapObject.transform.SetAsLastSibling();
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

using System.Collections.Generic;
using UnityEngine;
using UnboundLib;
using Photon.Pun;
using MapsExt.MapObjects.Properties;

namespace MapsExt
{
	public class MapObjectAnimation : MonoBehaviour
	{
		private class RigidBodyParams
		{
			public float gravityScale;
			public bool isKinematic;
			public bool useFullKinematicContacts;

			public RigidBodyParams(Rigidbody2D rb)
			{
				this.gravityScale = rb.gravityScale;
				this.isKinematic = rb.isKinematic;
				this.useFullKinematicContacts = rb.useFullKinematicContacts;
			}
		}

		private readonly float syncThreshold = 1f / 60f;

		public List<AnimationKeyframe> keyframes = new List<AnimationKeyframe>();
		public bool playOnAwake = true;
		public bool IsPlaying { get; private set; }

		private float elapsedTime;
		private int currentFrameIndex;
		private bool mapEntered;

		private string rpcKey;

		protected virtual void Awake()
		{
			var rb = this.gameObject.GetOrAddComponent<Rigidbody2D>();
			rb.gravityScale = 0;
			rb.constraints = RigidbodyConstraints2D.FreezeAll;
			rb.isKinematic = true;
			rb.useFullKinematicContacts = true;
		}

		protected virtual void OnEnable()
		{
			this.ExecuteAfterFrames(1, () =>
			{
				var photonMapObject = this.GetComponent<PhotonMapObject>();
				if (photonMapObject && !(bool) photonMapObject.GetFieldValue("photonSpawned"))
				{
					return;
				}

				if (this.playOnAwake)
				{
					this.Play();
				}
			});
		}

		protected virtual void OnDisable()
		{
			this.Stop();
		}

		protected virtual void Start()
		{
			var map = this.GetComponentInParent<Map>();
			this.mapEntered = map.hasEntered;
			map.mapIsReadyAction += () => this.mapEntered = true;
			map.mapMovingOutAction += () => this.mapEntered = false;

			this.rpcKey = $"MapObject {map.GetFieldValue("levelID")} {this.transform.GetSiblingIndex()}";
			var childRPC = MapManager.instance.GetComponent<ChildRPC>();

			childRPC.childRPCsVector2[this.rpcKey] = this.RPCA_SyncAnimation;
		}

		private void OnDestroy()
		{
			var childRPC = MapManager.instance?.GetComponent<ChildRPC>();
			if (this.rpcKey != null && childRPC.childRPCsVector2.ContainsKey(this.rpcKey))
			{
				childRPC.childRPCsVector2.Remove(this.rpcKey);
			}
		}

		protected virtual void Update()
		{
			if (!this.IsPlaying || !this.mapEntered || PlayerManager.instance.GetExtraData().MovingPlayers)
			{
				return;
			}

			this.ApplyKeyframe(this.currentFrameIndex, this.elapsedTime);
			this.elapsedTime += TimeHandler.deltaTime;

			// The first frame is considered as having a zero duration
			if (this.currentFrameIndex == 0 || this.elapsedTime > this.keyframes[this.currentFrameIndex].duration)
			{
				this.elapsedTime -= this.currentFrameIndex == 0 ? 0 : this.keyframes[this.currentFrameIndex].duration;
				this.currentFrameIndex = (this.currentFrameIndex + 1) % this.keyframes.Count;

				if (PhotonNetwork.IsMasterClient)
				{
					MapManager.instance.GetComponent<ChildRPC>().CallFunction(this.rpcKey, new Vector2(this.currentFrameIndex, this.elapsedTime));
				}
			}
		}

		public void Initialize(IAnimated anim)
		{
			this.keyframes.Clear();
			this.keyframes.Add(new AnimationKeyframe(anim.Animation.keyframes[0]));
		}

		public void Play()
		{
			this.IsPlaying = true;
			this.elapsedTime = 0;
			this.currentFrameIndex = 0;
		}

		public void Stop()
		{
			this.IsPlaying = false;
		}

		private void ApplyKeyframe(int frameIndex, float time = 0)
		{
			if (frameIndex > this.keyframes.Count - 1)
			{
				return;
			}

			var startFrame = frameIndex > 0 ? this.keyframes[frameIndex - 1] : this.keyframes[0];
			var endFrame = this.keyframes[frameIndex];

			float curveValue = endFrame.curve.Evaluate(time / endFrame.duration);

			for (int i = 0; i < startFrame.componentValues.Count; i++)
			{
				var startValue = startFrame.componentValues[i];
				var endValue = endFrame.componentValues[i];
				var nextValue = startValue.Lerp(endValue, curveValue);
				var serializer = MapsExtended.instance.propertyManager.GetSerializer(startValue.GetType());
				serializer.Deserialize(nextValue, this.gameObject);
			}
		}

		private void RPCA_SyncAnimation(Vector2 frameAndTime)
		{
			int newFrame = (int) frameAndTime[0];
			float newElapsedTime = frameAndTime[1];

			if (
				this.currentFrameIndex != newFrame ||
				Mathf.Abs(this.elapsedTime - newElapsedTime) > this.syncThreshold
			)
			{
				this.currentFrameIndex = newFrame;
				this.elapsedTime = newElapsedTime;
			}
		}
	}
}

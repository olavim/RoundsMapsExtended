using System.Collections.Generic;
using UnityEngine;
using UnboundLib;
using Photon.Pun;
using MapsExt.Properties;
using System.Linq;

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

		private const float SyncThreshold = 1f / 60f;

		private float _elapsedTime;
		private int _currentFrameIndex;
		private bool _mapEntered;
		private string _rpcKey;

		public List<AnimationKeyframe> Keyframes { get; set; } = new();
		public bool PlayOnAwake { get; set; } = true;
		public bool IsPlaying { get; private set; }

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

				if (this.PlayOnAwake)
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
			this._mapEntered = map.hasEntered;
			map.mapIsReadyAction += () => this._mapEntered = true;
			map.mapMovingOutAction += () => this._mapEntered = false;

			this._rpcKey = $"MapObject {map.GetFieldValue("levelID")} {this.transform.GetSiblingIndex()}";
			var childRPC = MapManager.instance.GetComponent<ChildRPC>();

			childRPC.childRPCsVector2[this._rpcKey] = this.RPCA_SyncAnimation;
		}

		private void OnDestroy()
		{
			var childRPC = MapManager.instance?.GetComponent<ChildRPC>();
			if (this._rpcKey != null && childRPC.childRPCsVector2.ContainsKey(this._rpcKey))
			{
				childRPC.childRPCsVector2.Remove(this._rpcKey);
			}
		}

		protected virtual void Update()
		{
			if (!this.IsPlaying || !this._mapEntered || PlayerManager.instance.GetExtraData().PlayersBeingMoved?.Any() == true)
			{
				return;
			}

			this.ApplyKeyframe(this._currentFrameIndex, this._elapsedTime);
			this._elapsedTime += TimeHandler.deltaTime;

			// The first frame is considered as having a zero duration
			if (this._currentFrameIndex == 0 || this._elapsedTime > this.Keyframes[this._currentFrameIndex].Duration)
			{
				this._elapsedTime -= this._currentFrameIndex == 0 ? 0 : this.Keyframes[this._currentFrameIndex].Duration;
				this._currentFrameIndex = (this._currentFrameIndex + 1) % this.Keyframes.Count;

				if (PhotonNetwork.IsMasterClient)
				{
					MapManager.instance.GetComponent<ChildRPC>().CallFunction(this._rpcKey, new Vector2(this._currentFrameIndex, this._elapsedTime));
				}
			}
		}

		public void Initialize(AnimationKeyframe keyFrame)
		{
			this.Keyframes.Clear();
			this.Keyframes.Add(keyFrame);
		}

		public void Play()
		{
			this.IsPlaying = true;
			this._elapsedTime = 0;
			this._currentFrameIndex = 0;
		}

		public void Stop()
		{
			this.IsPlaying = false;
		}

		private void ApplyKeyframe(int frameIndex, float time = 0)
		{
			if (frameIndex > this.Keyframes.Count - 1)
			{
				return;
			}

			var startFrame = frameIndex > 0 ? this.Keyframes[frameIndex - 1] : this.Keyframes[0];
			var endFrame = this.Keyframes[frameIndex];

			float curveValue = endFrame.Curve.EvaluateForDistance(time / endFrame.Duration);

			for (int i = 0; i < startFrame.ComponentValues.Count; i++)
			{
				var startValue = startFrame.ComponentValues[i];
				var endValue = endFrame.GetComponentValue(startValue.GetType());
				var nextValue = startValue.Lerp(endValue, curveValue);
				MapsExtended.PropertyManager.Write(nextValue, this.gameObject);
			}
		}

		private void RPCA_SyncAnimation(Vector2 frameAndTime)
		{
			int newFrame = (int) frameAndTime[0];
			float newElapsedTime = frameAndTime[1];

			if (
				this._currentFrameIndex != newFrame ||
				Mathf.Abs(this._elapsedTime - newElapsedTime) > SyncThreshold
			)
			{
				this._currentFrameIndex = newFrame;
				this._elapsedTime = newElapsedTime;
			}
		}
	}
}

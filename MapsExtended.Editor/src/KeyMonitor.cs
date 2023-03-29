using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapsExt.Editor
{
	public class KeyMonitor : MonoBehaviour
	{
		private const float HeldThreshold = 0.5f;
		private const float HeldInterval = 0.05f;

		private Dictionary<KeyCode, float> _heldKeys;
		private Dictionary<KeyCode, Action> _listeners;

		protected virtual void Awake()
		{
			this._heldKeys = new Dictionary<KeyCode, float>();
			this._listeners = new Dictionary<KeyCode, Action>();
		}

		public void AddListener(KeyCode code, Action action)
		{
			if (!this._listeners.ContainsKey(code))
			{
				this._listeners.Add(code, () => { });
				this._heldKeys.Add(code, 0);
			}

			this._listeners[code] += action;
		}

		public void RemoveListener(KeyCode code, Action action)
		{
			if (this._listeners.ContainsKey(code))
			{
				this._listeners[code] -= action;

				if (this._listeners[code].GetInvocationList().Length == 0)
				{
					this._listeners.Remove(code);
					this._heldKeys.Remove(code);
				}
			}
		}

		protected virtual void Update()
		{
			foreach (var key in this._listeners.Keys)
			{
				if (this.GetHeldKey(key))
				{
					this._listeners[key]();
				}
			}
		}

		private bool GetHeldKey(KeyCode code)
		{
			if (!EditorInput.GetKey(code))
			{
				this._heldKeys[code] = 0;
				return false;
			}

			float timeHeld = this._heldKeys[code];
			this._heldKeys[code] += Time.deltaTime;

			bool firstPress = timeHeld == 0;
			bool longPress = timeHeld > HeldThreshold + HeldInterval;

			if (longPress)
			{
				this._heldKeys[code] = HeldThreshold;
			}

			return firstPress || longPress;
		}
	}
}

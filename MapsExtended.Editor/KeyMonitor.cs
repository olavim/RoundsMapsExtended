using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapsExt.Editor
{
	public class KeyMonitor : MonoBehaviour
	{
		private readonly float heldThreshold = 0.5f;
		private readonly float heldInterval = 0.05f;

		private Dictionary<KeyCode, float> heldKeys;
		private Dictionary<KeyCode, Action> listeners;

		private void Awake()
		{
			this.heldKeys = new Dictionary<KeyCode, float>();
			this.listeners = new Dictionary<KeyCode, Action>();
		}

		public void AddListener(KeyCode code, Action action)
		{
			if (!this.listeners.ContainsKey(code))
			{
				this.listeners.Add(code, () => { });
				this.heldKeys.Add(code, 0);
			}

			this.listeners[code] += action;
		}

		public void RemoveListener(KeyCode code, Action action)
		{
			if (this.listeners.ContainsKey(code))
			{
				this.listeners[code] -= action;

				if (this.listeners[code].GetInvocationList().Length == 0)
				{
					this.listeners.Remove(code);
					this.heldKeys.Remove(code);
				}
			}
		}

		private void Update()
		{
			foreach (var key in this.listeners.Keys)
			{
				if (this.GetHeldKey(key))
				{
					this.listeners[key]();
				}
			}
		}

		private bool GetHeldKey(KeyCode code)
		{
			if (!EditorInput.GetKey(code))
			{
				this.heldKeys[code] = 0;
				return false;
			}

			float timeHeld = this.heldKeys[code];
			this.heldKeys[code] += Time.deltaTime;

			bool firstPress = timeHeld == 0;
			bool longPress = timeHeld > this.heldThreshold + this.heldInterval;

			if (longPress)
			{
				this.heldKeys[code] = this.heldThreshold;
			}

			return firstPress || longPress;
		}
	}
}

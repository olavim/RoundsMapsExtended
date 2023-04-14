using System.Collections.Generic;
using UnityEngine;
using MapsExt.Editor;

namespace MapsExt.Editor.Tests
{
	class TtlDictionary<TKey, TValue>
	{
		private Dictionary<TKey, TValue> dict = new();
		private Dictionary<TKey, int> ttl = new();

		public void Set(TKey key, TValue value, int ttl)
		{
			this.dict[key] = value;
			this.ttl[key] = ttl;
		}

		public void Tick()
		{
			var newDict = new Dictionary<TKey, TValue>();
			var newTtl = new Dictionary<TKey, int>();

			foreach (var key in this.dict.Keys)
			{
				if (this.ttl[key] > 0)
				{
					newDict[key] = this.dict[key];
					newTtl[key] = this.ttl[key] - 1;
				}
			}

			this.dict = newDict;
			this.ttl = newTtl;
		}

		public TValue this[TKey key]
		{
			get { return this.dict.GetValueOrDefault(key, default); }
		}
	}

	public class SimulatedInputSource : MonoBehaviour, IInputSource
	{
		public Vector2 MouseScrollDelta { get; private set; }
		public Vector3 MousePosition { get; private set; }

		private readonly Dictionary<KeyCode, bool> _keyDict = new();
		private readonly Dictionary<int, bool> _mouseButtonDict = new();

		private readonly TtlDictionary<KeyCode, bool> _keyDownDict = new();
		private readonly TtlDictionary<KeyCode, bool> _keyUpDict = new();
		private readonly TtlDictionary<int, bool> _mouseButtonDownDict = new();
		private readonly TtlDictionary<int, bool> _mouseButtonUpDict = new();

		private void LateUpdate()
		{
			this._keyDownDict.Tick();
			this._keyUpDict.Tick();
			this._mouseButtonDownDict.Tick();
			this._mouseButtonUpDict.Tick();
		}

		public bool GetKey(KeyCode key)
		{
			return this._keyDict.GetValueOrDefault(key, false);
		}

		public void SetKey(KeyCode key, bool value = true)
		{
			this._keyDict[key] = value;
		}

		public bool GetKeyDown(KeyCode key)
		{
			return this._keyDownDict[key];
		}

		public void SetKeyDown(KeyCode key, bool value = true)
		{
			this._keyDownDict.Set(key, value, 1);
		}

		public bool GetKeyUp(KeyCode key)
		{
			return this._keyUpDict[key];
		}

		public void SetKeyUp(KeyCode key, bool value = true)
		{
			this._keyUpDict.Set(key, value, 1);
		}

		public bool GetMouseButtonDown(int button)
		{
			return this._mouseButtonDownDict[button];
		}

		public void SetMouseButtonDown(int button, bool value = true)
		{
			this._mouseButtonDownDict.Set(button, value, 1);
		}

		public bool GetMouseButtonUp(int button)
		{
			return this._mouseButtonUpDict[button];
		}

		public void SetMouseButtonUp(int button, bool value = true)
		{
			this._mouseButtonUpDict.Set(button, value, 1);
		}

		public bool GetMouseButton(int button)
		{
			return this._mouseButtonDict.GetValueOrDefault(button, false);
		}

		public void SetMouseButton(int button, bool value = true)
		{
			this._mouseButtonDict[button] = value;
		}

		public void SetMousePosition(Vector3 position)
		{
			this.MousePosition = position;
		}

		public void SetMouseScrollDelta(Vector2 delta)
		{
			this.MouseScrollDelta = delta;
		}
	}
}

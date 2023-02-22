using MapsExt.Editor;
using System.Collections.Generic;
using UnityEngine;

namespace MapsExt.Test
{
	class TtlDictionary<TKey, TValue>
	{
		private Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();
		private Dictionary<TKey, int> ttl = new Dictionary<TKey, int>();

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
			get { return this.dict.GetValueOrDefault(key, default(TValue)); }
		}
	}

	public class SimulatedInputSource : MonoBehaviour, IInputSource
	{
		public Vector2 mouseScrollDelta { get; private set; }
		public Vector3 mousePosition { get; private set; }

		private Dictionary<KeyCode, bool> keyDict = new Dictionary<KeyCode, bool>();
		private Dictionary<int, bool> mouseButtonDict = new Dictionary<int, bool>();

		private TtlDictionary<KeyCode, bool> keyDownDict = new TtlDictionary<KeyCode, bool>();
		private TtlDictionary<KeyCode, bool> keyUpDict = new TtlDictionary<KeyCode, bool>();
		private TtlDictionary<int, bool> mouseButtonDownDict = new TtlDictionary<int, bool>();
		private TtlDictionary<int, bool> mouseButtonUpDict = new TtlDictionary<int, bool>();

		public void LateUpdate()
		{
			this.keyDownDict.Tick();
			this.keyUpDict.Tick();
			this.mouseButtonDownDict.Tick();
			this.mouseButtonUpDict.Tick();
		}

		public bool GetKey(KeyCode key)
		{
			return this.keyDict.GetValueOrDefault(key, false);
		}

		public void SetKey(KeyCode key, bool value = true)
		{
			this.keyDict[key] = value;
		}

		public bool GetKeyDown(KeyCode key)
		{
			return this.keyDownDict[key];
		}

		public void SetKeyDown(KeyCode key, bool value = true)
		{
			this.keyDownDict.Set(key, value, 1);
		}

		public bool GetKeyUp(KeyCode key)
		{
			return this.keyUpDict[key];
		}

		public void SetKeyUp(KeyCode key, bool value = true)
		{
			this.keyUpDict.Set(key, value, 1);
		}

		public bool GetMouseButtonDown(int button)
		{
			return this.mouseButtonDownDict[button];
		}

		public void SetMouseButtonDown(int button, bool value = true)
		{
			this.mouseButtonDownDict.Set(button, value, 1);
		}

		public bool GetMouseButtonUp(int button)
		{
			return this.mouseButtonUpDict[button];
		}

		public void SetMouseButtonUp(int button, bool value = true)
		{
			this.mouseButtonUpDict.Set(button, value, 1);
		}

		public bool GetMouseButton(int button)
		{
			return this.mouseButtonDict.GetValueOrDefault(button, false);
		}

		public void SetMouseButton(int button, bool value = true)
		{
			this.mouseButtonDict[button] = value;
		}

		public void SetMousePosition(Vector3 position)
		{
			this.mousePosition = position;
		}

		public void SetMouseScrollDelta(Vector2 delta)
		{
			this.mouseScrollDelta = delta;
		}
	}
}

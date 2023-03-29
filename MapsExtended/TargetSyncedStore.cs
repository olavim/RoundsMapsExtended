using System;
using System.Collections;
using System.Collections.Generic;

namespace MapsExt
{
	public class TargetSyncedStore<T> where T : IEquatable<T>
	{
		private object _currentTarget;
		private readonly Dictionary<int, T> _values = new Dictionary<int, T>();
		private readonly Dictionary<int, bool> _valueSet = new Dictionary<int, bool>();

		public int Allocate(object target)
		{
			if (target != this._currentTarget)
			{
				this._values.Clear();
				this._valueSet.Clear();
				this._currentTarget = target;
			}

			int id = this._values.Count;

			if (!this._values.ContainsKey(id))
			{
				this._values.Add(id, default);
				this._valueSet.Add(id, false);
			}

			return id;
		}

		public bool TargetEquals(object target)
		{
			return target == this._currentTarget;
		}

		public bool IsValueSet(int id)
		{
			return this._values.ContainsKey(id) && this._valueSet[id];
		}

		public IEnumerator WaitForValue(object target, int id)
		{
			while (target == this._currentTarget && !this.IsValueSet(id))
			{
				yield return null;
			}
		}

		public T Get(int id)
		{
			return this._values[id];
		}

		public void Set(int id, T value)
		{
			if (!this._values.ContainsKey(id))
			{
				this._values.Add(id, value);
				this._valueSet.Add(id, true);
			}
			else
			{
				this._values[id] = value;
				this._valueSet[id] = true;
			}
		}
	}
}

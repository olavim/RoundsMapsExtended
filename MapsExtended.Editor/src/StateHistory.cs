using System.Collections.Generic;

namespace MapsExt
{
	public class StateHistory<T>
	{
		public T CurrentState => this._states[this._stateIndex];

		private readonly List<T> _states;
		private int _stateIndex;

		public StateHistory(T initialState)
		{
			this._states = new List<T>() { initialState };
			this._stateIndex = 0;
		}

		public bool CanRedo()
		{
			return this._stateIndex < this._states.Count - 1;
		}

		public bool CanUndo()
		{
			return this._stateIndex > 0;
		}

		public void AddState(T state)
		{
			while (this._stateIndex < this._states.Count - 1)
			{
				this._states.RemoveAt(this._stateIndex + 1);
			}

			this._states.Add(state);
			this._stateIndex++;
		}

		public void Redo()
		{
			this._stateIndex++;
		}

		public void Undo()
		{
			this._stateIndex--;
		}
	}
}

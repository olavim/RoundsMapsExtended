using System.Collections.Generic;

namespace MapsExt
{
	public class StateHistory<T>
	{
		public T CurrentState => this.states[this.stateIndex];

		private readonly List<T> states;
		private int stateIndex;

		public StateHistory(T initialState)
		{
			this.states = new List<T>() { initialState };
			this.stateIndex = 0;
		}

		public bool CanRedo()
		{
			return this.stateIndex < this.states.Count - 1;
		}

		public bool CanUndo()
		{
			return this.stateIndex > 0;
		}

		public void AddState(T state)
		{
			while (this.stateIndex < this.states.Count - 1)
			{
				this.states.RemoveAt(this.stateIndex + 1);
			}

			this.states.Add(state);
			this.stateIndex++;
		}

		public void Redo()
		{
			this.stateIndex++;
		}

		public void Undo()
		{
			this.stateIndex--;
		}
	}
}

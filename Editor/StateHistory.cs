using System.Collections.Generic;

namespace MapsExt
{
	public class StateHistory
	{
		public CustomMap CurrentState => this.states[this.stateIndex];

		private List<CustomMap> states;
		private int stateIndex;

		public StateHistory(CustomMap initialState)
		{
			this.states = new List<CustomMap>() { initialState };
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

		public void AddState(CustomMap state)
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

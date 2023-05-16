using UnityEngine;

namespace MapsExt.Editor.UI
{
	public abstract class Vector2Element : InspectorElement
	{
		private readonly string _name;
		private Vector2Input _input;

		public Vector2 Value
		{
			get => this.GetValue();
			set => this.OnChange(value);
		}

		protected Vector2Element(string name)
		{
			this._name = name;
		}

		protected override GameObject GetInstance()
		{
			var instance = GameObject.Instantiate(Assets.InspectorVector2InputPrefab);
			var inspectorInput = instance.GetComponent<InspectorVector2Input>();
			inspectorInput.Label.text = this._name;
			this._input = inspectorInput.Input;
			this._input.OnChanged += this.OnChange;
			return instance;
		}

		public override void OnUpdate()
		{
			if (!this._input.IsFocused)
			{
				this._input.SetWithoutEvent(this.Value);
			}
		}

		protected abstract Vector2 GetValue();
		protected abstract void OnChange(Vector2 value);
	}
}

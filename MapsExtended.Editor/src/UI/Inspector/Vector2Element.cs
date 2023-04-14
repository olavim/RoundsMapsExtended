using UnityEngine;

namespace MapsExt.Editor.UI
{
	public abstract class Vector2Element : InspectorElement
	{
		private readonly string _name;
		private InspectorVector2 _input;

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
			var instance = GameObject.Instantiate(Assets.InspectorVector2Prefab);
			this._input = instance.GetComponent<InspectorVector2>();
			this._input.Label.text = this._name;
			this._input.Input.OnChanged += this.OnChange;
			return instance;
		}

		public override void OnUpdate()
		{
			this._input.Input.SetWithoutEvent(this.Value);
		}

		protected abstract Vector2 GetValue();
		protected abstract void OnChange(Vector2 value);
	}
}

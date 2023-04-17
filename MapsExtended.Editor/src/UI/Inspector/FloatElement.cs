using UnityEngine;

namespace MapsExt.Editor.UI
{
	public abstract class FloatElement : InspectorElement
	{
		private readonly string _name;
		private TextSliderInput _input;

		public float Value
		{
			get => this.GetValue();
			set => this.OnChange(value, ChangeType.ChangeEnd);
		}

		protected FloatElement(string name)
		{
			this._name = name;
		}

		protected override GameObject GetInstance()
		{
			var instance = GameObject.Instantiate(Assets.InspectorQuaternionPrefab);
			var quaternionInput = instance.GetComponent<InspectorQuaternion>();
			quaternionInput.Label.text = this._name;
			this._input = quaternionInput.Input;
			this._input.OnChanged += this.OnChange;
			return instance;
		}

		public override void OnUpdate()
		{
			this._input.SetWithoutEvent(this.Value);
		}

		protected abstract float GetValue();
		protected abstract void OnChange(float angle, ChangeType changeType);
	}
}

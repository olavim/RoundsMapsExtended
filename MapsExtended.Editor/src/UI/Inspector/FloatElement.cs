using UnityEngine;

namespace MapsExt.Editor.UI
{
	public abstract class FloatElement : InspectorElement
	{
		private readonly string _name;
		private readonly float _min;
		private readonly float _max;
		private TextSliderInput _input;

		public float Value
		{
			get => this.GetValue();
			set => this.OnChange(value, ChangeType.ChangeEnd);
		}

		protected FloatElement(string name, float min, float max)
		{
			this._name = name;
			this._min = min;
			this._max = max;
		}

		protected override GameObject GetInstance()
		{
			var instance = GameObject.Instantiate(Assets.InspectorQuaternionPrefab);
			var quaternionInput = instance.GetComponent<InspectorQuaternion>();
			quaternionInput.Label.text = this._name;
			this._input = quaternionInput.Input;
			this._input.Slider.minValue = this._min;
			this._input.Slider.maxValue = this._max;
			this._input.OnChanged += this.OnChange;
			return instance;
		}

		public override void OnUpdate()
		{
			if (!this._input.Input.isFocused)
			{
				this._input.SetWithoutEvent(this.Value);
			}
		}

		protected abstract float GetValue();
		protected abstract void OnChange(float angle, ChangeType changeType);
	}
}

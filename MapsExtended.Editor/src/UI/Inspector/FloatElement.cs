using UnityEngine;

namespace MapsExt.Editor.UI
{
	public abstract class FloatElement : InspectorElement
	{
		private readonly string _name;
		private readonly float _min;
		private readonly float _max;
		private readonly float _sliderMin;
		private readonly float _sliderMax;
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
			this._sliderMin = min;
			this._sliderMax = max;
		}

		protected FloatElement(string name, float min, float max, float sliderMin, float sliderMax)
		{
			this._name = name;
			this._min = min;
			this._max = max;
			this._sliderMin = sliderMin;
			this._sliderMax = sliderMax;
		}

		protected override GameObject GetInstance()
		{
#pragma warning disable IDE0002
			var instance = GameObject.Instantiate(Assets.InspectorSliderInputPrefab);
#pragma warning restore IDE0002
			var inspectorInput = instance.GetComponent<InspectorSliderInput>();
			inspectorInput.Label.text = this._name;
			this._input = inspectorInput.Input;
			this._input.MinValue = this._min;
			this._input.MaxValue = this._max;
			this._input.Slider.minValue = this._sliderMin;
			this._input.Slider.maxValue = this._sliderMax;
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

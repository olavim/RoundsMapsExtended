using UnityEngine.UI;
using UnityEngine;
using System;

namespace MapsExt.UI
{
	public class TextSliderInput : MonoBehaviour
	{
		public Slider slider;
		public InputField input;
		public Action<float> onChanged;

		private float inputValue;

		public float Value
		{
			get => this.inputValue;
			set => this.input.text = value.ToString();
		}

		private float maxSliderValue;

		public void Start()
		{
			this.slider.onValueChanged.AddListener(this.UpdateValue);
			this.input.onValueChanged.AddListener(this.UpdateValue);
			this.maxSliderValue = this.slider.maxValue;
		}

		private void UpdateValue(float origValue)
		{
			float value = Mathf.Min(this.maxSliderValue, origValue);
			value = (float) Math.Round(value * 10f) / 10f;

			if (this.slider.maxValue > this.maxSliderValue)
			{
				this.slider.maxValue = this.maxSliderValue;
			}

			if (value != origValue)
			{
				this.slider.value = value;
				return;
			}

			this.input.onValueChanged.RemoveListener(this.UpdateValue);
			this.input.text = value.ToString("0.0");
			this.input.onValueChanged.AddListener(this.UpdateValue);

			this.inputValue = value;
			this.onChanged?.Invoke(this.Value);
		}

		private void UpdateValue(string valueStr)
		{
			if (valueStr.EndsWith("."))
			{
				return;
			}

			float value = this.inputValue;

			if (valueStr == "")
			{
				value = 1f;
			}
			else if (float.TryParse(valueStr, out value))
			{
				this.slider.onValueChanged.RemoveListener(this.UpdateValue);
				this.slider.maxValue = (value > this.maxSliderValue) ? value : this.maxSliderValue;
				this.slider.value = value;
				this.slider.onValueChanged.AddListener(this.UpdateValue);

				this.inputValue = value;
			}

			this.onChanged?.Invoke(this.Value);
		}
	}
}

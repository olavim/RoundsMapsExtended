using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using System;

namespace MapsExt.UI
{
	public class TextSliderInput : MonoBehaviour
	{
		public enum ChangeType
		{
			ChangeStart,
			Change,
			ChangeEnd
		}

		public Slider slider;
		public InputField input;

		public Action<float, ChangeType> onChanged;

		private float inputValue;

		public float Value
		{
			get => this.inputValue;
			set
			{
				this.input.text = value.ToString();
			}
		}

		private float maxSliderValue;
		private float valueOnSliderMouseDown;
		private bool isMouseDownOnSlider = false;

		public void Awake()
		{
			this.slider.onValueChanged.AddListener(this.UpdateValueSlider);
			this.input.onValueChanged.AddListener(this.UpdateValueTextInput);
			this.maxSliderValue = this.slider.maxValue;
		}

		public void Start()
		{
			var eventTrigger = this.slider.gameObject.GetComponent<EventTrigger>() ?? this.slider.gameObject.AddComponent<EventTrigger>();

			var downEntry = new EventTrigger.Entry();
			downEntry.eventID = EventTriggerType.PointerDown;
			downEntry.callback.AddListener(data =>
			{
				this.isMouseDownOnSlider = true;
				this.valueOnSliderMouseDown = this.Value;
				this.onChanged?.Invoke(this.Value, ChangeType.ChangeStart);
			});

			var upEntry = new EventTrigger.Entry();
			upEntry.eventID = EventTriggerType.PointerUp;
			upEntry.callback.AddListener(data =>
			{
				this.isMouseDownOnSlider = false;
				if (this.Value != this.valueOnSliderMouseDown)
				{
					this.onChanged?.Invoke(this.Value, ChangeType.ChangeEnd);
				}
			});

			eventTrigger.triggers.Add(downEntry);
			eventTrigger.triggers.Add(upEntry);
		}

		private void UpdateValueSlider(float origValue)
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

			this.input.onValueChanged.RemoveListener(this.UpdateValueTextInput);
			this.input.text = value.ToString("0.0");
			this.input.onValueChanged.AddListener(this.UpdateValueTextInput);

			this.inputValue = value;

			this.onChanged?.Invoke(this.Value, ChangeType.Change);
		}

		private void UpdateValueTextInput(string valueStr)
		{
			if (valueStr.EndsWith("."))
			{
				return;
			}

			if (valueStr == "")
			{
				this.inputValue = 1f;
			}
			else if (float.TryParse(valueStr, out float value))
			{
				this.slider.onValueChanged.RemoveListener(this.UpdateValueSlider);
				this.slider.maxValue = (value > this.maxSliderValue) ? value : this.maxSliderValue;
				this.slider.value = value;
				this.slider.onValueChanged.AddListener(this.UpdateValueSlider);

				this.inputValue = value;
			}

			this.onChanged?.Invoke(this.Value, ChangeType.ChangeStart);
			this.onChanged?.Invoke(this.Value, ChangeType.Change);
			this.onChanged?.Invoke(this.Value, ChangeType.ChangeEnd);
		}

		public void SetEnabled(bool enabled)
		{
			this.slider.interactable = enabled;
			this.input.interactable = enabled;
		}
	}
}

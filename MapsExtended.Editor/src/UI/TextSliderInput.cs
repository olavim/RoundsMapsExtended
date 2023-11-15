using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using System;

namespace MapsExt.Editor.UI
{
	public class TextSliderInput : MonoBehaviour
	{
		[SerializeField] private Slider _slider;
		[SerializeField] private InputField _input;
		[SerializeField] private float _minValue = float.MinValue;
		[SerializeField] private float _maxValue = float.MaxValue;
		private float _inputValue;
		private float _startValue;
		private bool _textWasFocused;

		public Slider Slider { get => this._slider; set => this._slider = value; }
		public InputField Input { get => this._input; set => this._input = value; }
		public float MinValue { get => this._minValue; set => this._minValue = value; }
		public float MaxValue { get => this._maxValue; set => this._maxValue = value; }

		public Action<float, ChangeType> OnChanged { get; set; }

		public float Value
		{
			get => this._inputValue;
			set => this.Input.text = value.ToString();
		}

		protected virtual void Awake()
		{
			this.Slider.onValueChanged.AddListener(this.UpdateValueSlider);
			this.Input.onValueChanged.AddListener(this.UpdateValueTextInput);
		}

		protected virtual void Start()
		{
			var eventTrigger = this.Slider.gameObject.GetComponent<EventTrigger>() ?? this.Slider.gameObject.AddComponent<EventTrigger>();

			var downEntry = new EventTrigger.Entry
			{
				eventID = EventTriggerType.PointerDown
			};
			downEntry.callback.AddListener(_ =>
			{
				this._startValue = this.Value;
				this.OnChanged?.Invoke(this.Value, ChangeType.ChangeStart);
			});

			var upEntry = new EventTrigger.Entry
			{
				eventID = EventTriggerType.PointerUp
			};
			upEntry.callback.AddListener(_ =>
			{
				if (this.Value != this._startValue)
				{
					this.OnChanged?.Invoke(this.Value, ChangeType.ChangeEnd);
				}
			});

			eventTrigger.triggers.Add(downEntry);
			eventTrigger.triggers.Add(upEntry);
		}

		protected virtual void Update()
		{
			if (!this._textWasFocused && this.Input.isFocused)
			{
				this.OnChanged?.Invoke(this.Value, ChangeType.ChangeStart);
				this._startValue = this.Value;
			}

			if (this._textWasFocused && !this.Input.isFocused && this.Value != this._startValue)
			{
				this.OnChanged?.Invoke(this.Value, ChangeType.ChangeEnd);
			}

			this._textWasFocused = this.Input.isFocused;
		}

		private void UpdateValueSlider(float origValue)
		{
			float value = (float) Math.Round(origValue * 10f) / 10f;
			value = Mathf.Clamp(value, this.Slider.minValue, this.Slider.maxValue);
			value = Mathf.Clamp(value, this.MinValue, this.MaxValue);

			if (value != origValue)
			{
				this.Slider.value = value;
				return;
			}

			this.Input.onValueChanged.RemoveListener(this.UpdateValueTextInput);
			this.Input.text = value.ToString("0.0");
			this.Input.onValueChanged.AddListener(this.UpdateValueTextInput);

			this._inputValue = value;

			this.OnChanged?.Invoke(this.Value, ChangeType.Change);
		}

		private void UpdateValueTextInput(string valueStr)
		{
			if (valueStr.EndsWith("."))
			{
				return;
			}

			if (valueStr?.Length == 0)
			{
				this._inputValue = 0;
			}
			else if (float.TryParse(valueStr, out float value))
			{
				value = Mathf.Clamp(value, this.MinValue, this.MaxValue);
				this.Slider.onValueChanged.RemoveListener(this.UpdateValueSlider);
				this.Slider.value = value;
				this.Slider.onValueChanged.AddListener(this.UpdateValueSlider);

				this._inputValue = value;
			}

			this.OnChanged?.Invoke(this.Value, ChangeType.Change);
		}

		public void SetWithoutEvent(float value)
		{
			var cb = this.OnChanged;
			this.OnChanged = null;
			this.Value = value;
			this.OnChanged = cb;
		}

		public void SetEnabled(bool enabled)
		{
			this.Slider.interactable = enabled;
			this.Input.interactable = enabled;

			var col = enabled ? Color.white : new Color(0.78f, 0.78f, 0.78f, 0.4f);
			foreach (var text in this.gameObject.GetComponentsInChildren<Text>())
			{
				text.color = col;
			}
		}
	}
}

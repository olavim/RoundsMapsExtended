using UnityEngine.UI;
using UnityEngine;
using System;

namespace MapsExt.UI
{
	public class KeyframeSettings : MonoBehaviour
	{
		public Foldout contentFoldout;
		public Slider durationSlider;
		public InputField durationInput;
		public Dropdown easingDropdown;
		public ColorBlock colors;

		public Action onClick;
		public Action<float> onDurationChanged;
		public Action<string> onEasingChanged;

		public float Duration { get; private set; }
		public string Easing { get; private set; }

		private bool isSelected = false;
		private float maxDurationSliderValue;

		public void Start()
		{
			this.contentFoldout.foldoutToggle.onClick.AddListener(() =>
			{
				this.UpdateFoldoutColors();
				this.onClick?.Invoke();
			});

			this.durationSlider.onValueChanged.AddListener(this.UpdateDuration);
			this.durationInput.onValueChanged.AddListener(this.UpdateDuration);
			this.maxDurationSliderValue = this.durationSlider.maxValue;

			this.easingDropdown.onValueChanged.AddListener(this.UpdateEasing);

			this.UpdateFoldoutColors();
		}

		private void UpdateFoldoutColors()
		{
			this.contentFoldout.foldoutToggle.colors = new ColorBlock()
			{
				colorMultiplier = this.colors.colorMultiplier,
				fadeDuration = this.colors.fadeDuration,
				normalColor = this.isSelected ? this.colors.pressedColor : this.colors.normalColor,
				highlightedColor = this.colors.highlightedColor,
				pressedColor = this.colors.pressedColor,
				disabledColor = this.colors.disabledColor
			};
		}

		private void UpdateDuration(float origValue)
		{
			float value = Mathf.Min(this.maxDurationSliderValue, origValue);
			value = (float) Math.Round(value * 10f) / 10f;

			if (this.durationSlider.maxValue > this.maxDurationSliderValue)
			{
				this.durationSlider.maxValue = this.maxDurationSliderValue;
			}

			if (value != origValue)
			{
				this.durationSlider.value = value;
				return;
			}

			this.durationInput.onValueChanged.RemoveListener(this.UpdateDuration);
			this.durationInput.text = value.ToString("0.0");
			this.durationInput.onValueChanged.AddListener(this.UpdateDuration);

			this.Duration = value;
			this.onDurationChanged?.Invoke(this.Duration);
		}

		private void UpdateDuration(string valueStr)
		{
			if (valueStr.EndsWith("."))
			{
				return;
			}

			float value = this.Duration;

			if (valueStr == "")
			{
				value = 1f;
			}
			else if (float.TryParse(valueStr, out value))
			{
				this.durationSlider.onValueChanged.RemoveListener(this.UpdateDuration);
				this.durationSlider.maxValue = (value > this.maxDurationSliderValue) ? value : this.maxDurationSliderValue;
				this.durationSlider.value = value;
				this.durationSlider.onValueChanged.AddListener(this.UpdateDuration);

				this.Duration = value;
			}

			this.onDurationChanged?.Invoke(this.Duration);
		}

		private void UpdateEasing(int index)
		{
			this.Easing = this.easingDropdown.options[index].text;
			this.onEasingChanged?.Invoke(this.Easing);
		}

		public void Select()
		{
			this.SetSelected(true);
		}

		public void Deselect()
		{
			this.SetSelected(false);
		}

		public void SetSelected(bool selected)
		{
			this.isSelected = selected;

			if (!selected)
			{
				this.contentFoldout.SetOpen(false);
			}

			this.UpdateFoldoutColors();
		}
	}
}

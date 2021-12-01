using UnityEngine.UI;
using UnityEngine;
using System;

namespace MapsExt.UI
{
	public class KeyframeSettings : MonoBehaviour
	{
		public Foldout contentFoldout;
		public TextSliderInput durationInput;
		public Dropdown easingDropdown;
		public ColorBlock colors;

		public Action onClick;
		public Action<float> onDurationChangedStart;
		public Action<float> onDurationChangedEnd;
		public Action<float> onDurationChanged;
		public Action<string> onEasingChanged;

		public float Duration { get; private set; }
		public string Easing { get; private set; }

		private bool isSelected = false;

		public void Start()
		{
			this.contentFoldout.foldoutToggle.onClick.AddListener(() =>
			{
				this.UpdateFoldoutColors();
				this.onClick?.Invoke();
			});

			this.durationInput.onChanged += this.UpdateDuration;
			this.durationInput.onChangedStart += this.UpdateDurationStart;
			this.durationInput.onChangedEnd += this.UpdateDurationEnd;
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

		private void UpdateDuration(float value)
		{
			this.Duration = value;
			this.onDurationChanged?.Invoke(this.Duration);
		}

		private void UpdateDurationStart(float value)
		{
			this.Duration = value;
			this.onDurationChangedStart?.Invoke(this.Duration);
		}

		private void UpdateDurationEnd(float value)
		{
			this.Duration = value;
			this.onDurationChangedEnd?.Invoke(this.Duration);
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

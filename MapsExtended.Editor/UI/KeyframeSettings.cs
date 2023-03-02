using UnityEngine.UI;
using UnityEngine;
using System;

namespace MapsExt.Editor.UI
{
	public class KeyframeSettings : MonoBehaviour
	{
		public Foldout contentFoldout;
		public TextSliderInput durationInput;
		public Dropdown easingDropdown;
		public ColorBlock colors;

		public Action onClick;
		public Action<float, ChangeType> onDurationChanged;
		public Action<string> onEasingChanged;

		public float Duration { get; private set; }
		public string Easing { get; private set; }

		private bool isSelected = false;

		private void Start()
		{
			this.contentFoldout.foldoutToggle.onClick.AddListener(() =>
			{
				this.UpdateFoldoutColors();
				this.onClick?.Invoke();
			});

			this.durationInput.onChanged += this.UpdateDuration;
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

		private void UpdateDuration(float value, ChangeType type)
		{
			this.Duration = value;
			this.onDurationChanged?.Invoke(this.Duration, type);
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

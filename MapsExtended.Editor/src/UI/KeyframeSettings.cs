using UnityEngine.UI;
using UnityEngine;
using System;

namespace MapsExt.Editor.UI
{
	public class KeyframeSettings : MonoBehaviour
	{
		[SerializeField] private Foldout _contentFoldout;
		[SerializeField] private TextSliderInput _durationInput;
		[SerializeField] private Dropdown _easingDropdown;
		[SerializeField] private ColorBlock _colors;
		private bool _isSelected;

		public Foldout ContentFoldout { get => this._contentFoldout; set => this._contentFoldout = value; }
		public TextSliderInput DurationInput { get => this._durationInput; set => this._durationInput = value; }
		public Dropdown EasingDropdown { get => this._easingDropdown; set => this._easingDropdown = value; }
		public ColorBlock Colors { get => this._colors; set => this._colors = value; }

		public Action OnClick { get; set; }
		public Action<float, ChangeType> OnDurationChanged { get; set; }
		public Action<string> OnEasingChanged { get; set; }

		public float Duration { get; private set; }
		public string Easing { get; private set; }

		protected virtual void Start()
		{
			this.ContentFoldout.FoldoutToggle.onClick.AddListener(() =>
			{
				this.UpdateFoldoutColors();
				this.OnClick?.Invoke();
			});

			this.DurationInput.OnChanged += this.UpdateDuration;
			this.EasingDropdown.onValueChanged.AddListener(this.UpdateEasing);

			this.UpdateFoldoutColors();
		}

		private void UpdateFoldoutColors()
		{
			this.ContentFoldout.FoldoutToggle.colors = new ColorBlock()
			{
				colorMultiplier = this.Colors.colorMultiplier,
				fadeDuration = this.Colors.fadeDuration,
				normalColor = this._isSelected ? this.Colors.pressedColor : this.Colors.normalColor,
				highlightedColor = this.Colors.highlightedColor,
				pressedColor = this.Colors.pressedColor,
				disabledColor = this.Colors.disabledColor
			};
		}

		private void UpdateDuration(float value, ChangeType type)
		{
			this.Duration = value;
			this.OnDurationChanged?.Invoke(this.Duration, type);
		}

		private void UpdateEasing(int index)
		{
			this.Easing = this.EasingDropdown.options[index].text;
			this.OnEasingChanged?.Invoke(this.Easing);
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
			this._isSelected = selected;

			if (!selected)
			{
				this.ContentFoldout.SetOpen(false);
			}

			this.UpdateFoldoutColors();
		}
	}
}

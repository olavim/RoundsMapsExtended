using MapsExt.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.UI
{
	public sealed class AnimationWindow : Window
	{
		[SerializeField] private Button _deleteButton;
		[SerializeField] private Button _addButton;
		[SerializeField] private MapEditor _editor;
		[SerializeField] private MapObjectInspector _inspector;

		public Button DeleteButton { get => this._deleteButton; set => this._deleteButton = value; }
		public Button AddButton { get => this._addButton; set => this._addButton = value; }
		public MapEditor Editor { get => this._editor; set => this._editor = value; }
		public MapObjectInspector Inspector { get => this._inspector; set => this._inspector = value; }

		protected override void Start()
		{
			this.CloseButton.onClick.AddListener(this.Close);
			this.DeleteButton.onClick.AddListener(this.DeleteAnimationKeyframe);
			this.AddButton.onClick.AddListener(this.AddAnimationKeyframe);
		}

		public void Refresh()
		{
			GameObjectUtils.DestroyChildrenImmediateSafe(this.Content);

			var anim = this.Editor.AnimationHandler.Animation;

			if (!anim)
			{
				return;
			}

			for (int i = 0; i < anim.Keyframes.Count; i++)
			{
				var settings = this.AddAnimationKeyframeSettings(i);
				settings.SetSelected(i == this.Editor.AnimationHandler.KeyframeIndex);
			}

			this.DeleteButton.interactable = this.Editor.AnimationHandler.KeyframeIndex > 0;
		}

		private KeyframeSettings AddAnimationKeyframeSettings(int keyframe)
		{
			var anim = this.Editor.AnimationHandler.Animation;
			var keyframeSettings = GameObject.Instantiate(Assets.KeyframeSettingsPrefab, this.Content.transform).GetComponent<KeyframeSettings>();

			keyframeSettings.ContentFoldout.Label.text = keyframe == 0 ? "Base" : $"Keyframe {keyframe}";

			if (keyframe == 0)
			{
				keyframeSettings.ContentFoldout.Label.text = "Base";
				GameObjectUtils.DestroyImmediateSafe(keyframeSettings.ContentFoldout.Content);
			}
			else
			{
				keyframeSettings.ContentFoldout.Label.text = $"Keyframe {keyframe}";
			}

			keyframeSettings.OnDurationChanged += (value, type) =>
			{
				float durationDelta = value - anim.Keyframes[keyframe].Duration;
				anim.Keyframes[keyframe].Duration = value;
				anim.Keyframes[keyframe].UpdateCurve();

				if (type == ChangeType.ChangeEnd)
				{
					this.Editor.TakeSnaphot();
				}
			};

			keyframeSettings.OnEasingChanged += value =>
			{
				var curveType =
					value == "In" ? CurveType.EaseIn :
					value == "Out" ? CurveType.EaseOut :
					value == "In and Out" ? CurveType.EaseInOut :
					CurveType.Linear;

				anim.Keyframes[keyframe].CurveType = curveType;
				anim.Keyframes[keyframe].UpdateCurve();

				this.Editor.TakeSnaphot();
			};

			keyframeSettings.OnClick += () =>
			{
				foreach (var settings in this.Content.GetComponentsInChildren<KeyframeSettings>())
				{
					settings.SetSelected(settings == keyframeSettings);
				}

				this.Editor.AnimationHandler.SetKeyframe(keyframe);
				this.DeleteButton.interactable = keyframe > 0;
			};

			keyframeSettings.DurationInput.Value = anim.Keyframes[keyframe].Duration;
			keyframeSettings.EasingDropdown.value = (int) anim.Keyframes[keyframe].CurveType;

			return keyframeSettings;
		}

		private void AddAnimationKeyframe()
		{
			this.Editor.AnimationHandler.AddKeyframe();
			this.Refresh();
		}

		private void DeleteAnimationKeyframe()
		{
			this.Editor.AnimationHandler.DeleteKeyframe(this.Editor.AnimationHandler.KeyframeIndex);
			this.Refresh();
		}

		public void Open()
		{
			if (this.Editor.AnimationHandler.Animation == null)
			{
				var anim = this.Editor.ActiveMapObjectPart.GetComponent<MapObjectAnimation>();

				if (anim)
				{
					this.Editor.AnimationHandler.SetAnimation(anim);
				}
				else
				{
					this.Editor.AnimationHandler.AddAnimation(this.Editor.ActiveMapObjectPart);
				}
			}

			this.Refresh();
			this.gameObject.SetActive(true);
		}

		public void Close()
		{
			this.gameObject.SetActive(false);
			this.Editor.AnimationHandler.SetAnimation(null);
			GameObjectUtils.DestroyChildrenImmediateSafe(this.Content);
		}

		public void SetOpen(bool open)
		{
			if (open)
			{
				this.Open();
			}
			else
			{
				this.Close();
			}
		}
	}
}

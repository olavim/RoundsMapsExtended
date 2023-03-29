using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.UI
{
	public class AnimationWindow : Window
	{
		public Button deleteButton;
		public Button addButton;
		public MapEditor editor;
		public MapObjectInspector inspector;

		protected new virtual void Start()
		{
			this.closeButton.onClick.AddListener(this.Close);
			this.deleteButton.onClick.AddListener(this.DeleteAnimationKeyframe);
			this.addButton.onClick.AddListener(this.AddAnimationKeyframe);
		}

		public void Refresh()
		{
			GameObjectUtils.DestroyChildrenImmediateSafe(this.content);

			var anim = this.editor.animationHandler.animation;

			if (!anim)
			{
				return;
			}

			for (int i = 0; i < anim.keyframes.Count; i++)
			{
				var settings = this.AddAnimationKeyframeSettings(i);
				settings.SetSelected(i == this.editor.animationHandler.KeyframeIndex);
			}

			this.deleteButton.interactable = this.editor.animationHandler.KeyframeIndex > 0;
		}

		private KeyframeSettings AddAnimationKeyframeSettings(int keyframe)
		{
			var anim = this.editor.animationHandler.animation;
			var keyframeSettings = GameObject.Instantiate(Assets.KeyframeSettingsPrefab, this.content.transform).GetComponent<KeyframeSettings>();

			keyframeSettings.contentFoldout.label.text = keyframe == 0 ? "Base" : $"Keyframe {keyframe}";

			if (keyframe == 0)
			{
				keyframeSettings.contentFoldout.label.text = "Base";
				GameObjectUtils.DestroyImmediateSafe(keyframeSettings.contentFoldout.content);
			}
			else
			{
				keyframeSettings.contentFoldout.label.text = $"Keyframe {keyframe}";
			}

			keyframeSettings.onDurationChanged += (value, type) =>
			{
				float durationDelta = value - anim.keyframes[keyframe].Duration;
				anim.keyframes[keyframe].Duration = value;
				anim.keyframes[keyframe].UpdateCurve();

				if (type == ChangeType.ChangeEnd)
				{
					this.editor.TakeSnaphot();
				}
			};

			keyframeSettings.onEasingChanged += value =>
			{
				var curveType =
					value == "In" ? CurveType.EaseIn :
					value == "Out" ? CurveType.EaseOut :
					value == "In and Out" ? CurveType.EaseInOut :
					CurveType.Linear;

				anim.keyframes[keyframe].CurveType = curveType;
				anim.keyframes[keyframe].UpdateCurve();

				this.editor.TakeSnaphot();
			};

			keyframeSettings.onClick += () =>
			{
				foreach (var settings in this.content.GetComponentsInChildren<KeyframeSettings>())
				{
					settings.SetSelected(settings == keyframeSettings);
				}

				this.editor.animationHandler.SetKeyframe(keyframe);
				this.deleteButton.interactable = keyframe > 0;
			};

			keyframeSettings.durationInput.Value = anim.keyframes[keyframe].Duration;
			keyframeSettings.easingDropdown.value = (int) anim.keyframes[keyframe].CurveType;

			return keyframeSettings;
		}

		private void AddAnimationKeyframe()
		{
			this.editor.animationHandler.AddKeyframe();
			this.Refresh();
		}

		private void DeleteAnimationKeyframe()
		{
			this.editor.animationHandler.DeleteKeyframe(this.editor.animationHandler.KeyframeIndex);
			this.Refresh();
		}

		public void Open()
		{
			if (this.editor.animationHandler.animation == null)
			{
				var anim = this.editor.activeObject.GetComponent<MapObjectAnimation>();

				if (anim)
				{
					this.editor.animationHandler.SetAnimation(anim);
				}
				else
				{
					this.editor.animationHandler.AddAnimation(this.editor.activeObject);
				}
			}

			this.Refresh();
			this.gameObject.SetActive(true);
		}

		public void Close()
		{
			this.gameObject.SetActive(false);
			this.editor.animationHandler.SetAnimation(null);
			GameObjectUtils.DestroyChildrenImmediateSafe(this.content);
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

using UnityEngine;
using UnityEngine.UI;
using MapsExt.Editor.Commands;
using MapsExt.Editor.ActionHandlers;
using MapsExt.MapObjects;

namespace MapsExt.Editor.UI
{
	public class AnimationWindow : Window
	{
		public Button deleteButton;
		public Button addButton;
		public MapEditor editor;
		public MapObjectInspector inspector;

		private new void Start()
		{
			this.closeButton.onClick.AddListener(this.Close);
			this.deleteButton.onClick.AddListener(this.DeleteAnimationKeyframe);
			this.addButton.onClick.AddListener(this.AddAnimationKeyframe);
		}

		public void Refresh()
		{
			foreach (Transform child in this.content.transform)
			{
				GameObject.Destroy(child.gameObject);
			}

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
				GameObject.Destroy(keyframeSettings.contentFoldout.content);
			}
			else
			{
				keyframeSettings.contentFoldout.label.text = $"Keyframe {keyframe}";
			}

			keyframeSettings.onDurationChanged += (value, type) =>
			{
				if (type == TextSliderInput.ChangeType.ChangeStart)
				{
					this.editor.commandHistory.PreventNextMerge();
				}

				float durationDelta = value - anim.keyframes[keyframe].duration;
				var cmd = new ChangeKeyframeDurationCommand(anim.gameObject, durationDelta, keyframe);
				this.editor.commandHistory.Add(cmd, true);

				anim.keyframes[keyframe].UpdateCurve();
			};

			keyframeSettings.onEasingChanged += value =>
			{
				var curveType =
					value == "In" ? AnimationKeyframe.CurveType.EaseIn :
					value == "Out" ? AnimationKeyframe.CurveType.EaseOut :
					value == "In and Out" ? AnimationKeyframe.CurveType.EaseInOut :
					AnimationKeyframe.CurveType.Linear;

				var cmd = new ChangeKeyframeEasingCommand(anim.gameObject, curveType, keyframe);
				this.editor.commandHistory.Add(cmd);
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

			keyframeSettings.durationInput.Value = anim.keyframes[keyframe].duration;
			keyframeSettings.easingDropdown.value = (int) anim.keyframes[keyframe].curveType;

			return keyframeSettings;
		}

		private void AddAnimationKeyframe()
		{
			var anim = this.editor.animationHandler.animation;
			var cmd = new AddKeyframeCommand(anim.gameObject, new AnimationKeyframe(anim.keyframes[anim.keyframes.Count - 1]), anim.keyframes.Count);
			this.editor.commandHistory.Add(cmd);

			this.inspector.Unlink();
			this.inspector.Link(this.editor.animationHandler.keyframeMapObject);
			this.Refresh();
		}

		private void DeleteAnimationKeyframe()
		{
			var cmd = new DeleteKeyframeCommand(this.editor.animationHandler.animation.gameObject, this.editor.animationHandler.KeyframeIndex);
			this.editor.commandHistory.Add(cmd);

			this.inspector.Unlink();
			this.inspector.Link(this.editor.animationHandler.keyframeMapObject);
			this.Refresh();
		}

		public void Open()
		{
			var handler = this.inspector.interactionTarget.GetComponent<EditorActionHandler>();
			var anim = handler.GetComponent<MapObjectAnimation>();

			if (anim)
			{
				this.editor.animationHandler.SetAnimation(anim);
			}
			else
			{
				this.editor.animationHandler.AddAnimation(handler.GetComponentInParent<MapObjectInstance>().gameObject);
			}

			this.Refresh();
			this.gameObject.SetActive(true);

		}

		public void Close()
		{
			this.gameObject.SetActive(false);
			this.editor.animationHandler.SetAnimation(null);

			foreach (Transform child in this.content.transform)
			{
				GameObject.Destroy(child.gameObject);
			}
		}
	}
}

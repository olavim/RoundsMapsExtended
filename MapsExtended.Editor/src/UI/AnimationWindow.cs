using UnityEngine;
using UnityEngine.UI;
using MapsExt.MapObjects;

namespace MapsExt.Editor.UI
{
	public class AnimationWindow : Window
	{
		public Button deleteButton;
		public Button addButton;
		public MapEditor editor;
		public MapObjectInspector inspector;

		private void Start()
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
				float durationDelta = value - anim.keyframes[keyframe].duration;
				anim.keyframes[keyframe].duration = value;
				anim.keyframes[keyframe].UpdateCurve();

				if (type == ChangeType.ChangeEnd)
				{
					this.editor.TakeSnaphot();
				}
			};

			keyframeSettings.onEasingChanged += value =>
			{
				var curveType =
					value == "In" ? AnimationKeyframe.CurveType.EaseIn :
					value == "Out" ? AnimationKeyframe.CurveType.EaseOut :
					value == "In and Out" ? AnimationKeyframe.CurveType.EaseInOut :
					AnimationKeyframe.CurveType.Linear;

				anim.keyframes[keyframe].curveType = curveType;
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

			keyframeSettings.durationInput.Value = anim.keyframes[keyframe].duration;
			keyframeSettings.easingDropdown.value = (int) anim.keyframes[keyframe].curveType;

			return keyframeSettings;
		}

		private void AddAnimationKeyframe()
		{
			var anim = this.editor.animationHandler.animation;
			var newFrame = new AnimationKeyframe(anim.keyframes[anim.keyframes.Count - 1]);
			int frameIndex = anim.keyframes.Count;

			if (anim.keyframes.Count == 0)
			{
				anim.playOnAwake = false;
				anim.Initialize((SpatialMapObjectData) MapsExtendedEditor.instance.mapObjectManager.Serialize(anim.gameObject));
			}

			anim.keyframes.Insert(frameIndex, newFrame);
			this.editor.animationHandler.SetKeyframe(frameIndex);
			this.editor.TakeSnaphot();

			this.Refresh();
		}

		private void DeleteAnimationKeyframe()
		{
			var anim = this.editor.animationHandler.animation;
			anim.keyframes.RemoveAt(this.editor.animationHandler.KeyframeIndex);

			if (this.editor.animationHandler.KeyframeIndex >= anim.keyframes.Count)
			{
				this.editor.animationHandler.SetKeyframe(anim.keyframes.Count - 1);
			}

			this.editor.TakeSnaphot();
			this.Refresh();
		}

		public void Open()
		{
			var anim = this.inspector.target.GetComponent<MapObjectAnimation>();

			if (anim)
			{
				this.editor.animationHandler.SetAnimation(anim);
			}
			else
			{
				this.editor.animationHandler.AddAnimation(this.inspector.target.gameObject);
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

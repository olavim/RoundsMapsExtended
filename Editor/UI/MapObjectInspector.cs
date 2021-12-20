using UnityEngine.UI;
using UnityEngine;
using MapsExt.Editor.ActionHandlers;
using MapsExt.Editor.Commands;
using System;

namespace MapsExt.Editor.UI
{
	public class MapObjectInspector : MonoBehaviour
	{
		[AttributeUsage(AttributeTargets.Field, Inherited = true)]
		public class Vector2Field : Attribute { }
		[AttributeUsage(AttributeTargets.Field, Inherited = true)]
		public class NumberField : Attribute { }
		[AttributeUsage(AttributeTargets.Field, Inherited = true)]
		public class BooleanField : Attribute { }

		public Vector2Input positionInput;
		public Vector2Input sizeInput;
		public TextSliderInput rotationInput;
		public Button animationButton;
		public AnimationWindow animationWindow;

		public GameObject visualTarget;
		public GameObject interactionTarget;
		public MapEditor editor;

		private void Start()
		{
			this.animationButton.onClick.AddListener(this.OnClickAnimationButton);
		}

		private void Update()
		{
			bool animWindowOpen = this.animationWindow.gameObject.activeSelf;
			this.animationButton.gameObject.GetComponentInChildren<Text>().text = animWindowOpen
				? "Close Animation"
				: "Edit Animation";
		}

		public void Link(GameObject interactionTarget)
		{
			this.Link(interactionTarget, interactionTarget);
		}

		public void Link(GameObject interactionTarget, GameObject visualTarget)
		{
			this.gameObject.SetActive(true);

			var actionHandler = visualTarget.GetComponent<EditorActionHandler>();

			this.positionInput.Value = visualTarget.transform.position;
			this.sizeInput.Value = visualTarget.transform.localScale;
			this.rotationInput.Value = visualTarget.transform.rotation.eulerAngles.z;

			this.positionInput.onChanged += this.PositionChanged;
			this.sizeInput.onChanged += this.SizeChanged;
			this.rotationInput.onChanged += this.RotationChanged;

			this.visualTarget = visualTarget;
			this.interactionTarget = interactionTarget;

			this.sizeInput.SetEnabled(actionHandler.CanResize());
			this.rotationInput.SetEnabled(actionHandler.CanRotate());
		}

		public void Unlink()
		{
			this.positionInput.onChanged -= this.PositionChanged;
			this.sizeInput.onChanged -= this.SizeChanged;
			this.rotationInput.onChanged -= this.RotationChanged;

			this.visualTarget = null;
			this.interactionTarget = null;
			this.gameObject.SetActive(false);
		}

		private void PositionChanged(Vector2 value)
		{
			var delta = (Vector3) value - this.visualTarget.transform.position;
			var cmd = new MoveCommand(this.visualTarget.GetComponent<EditorActionHandler>(), delta);
			this.editor.commandHistory.Add(cmd);
			this.editor.UpdateRopeAttachments(false);
		}

		private void SizeChanged(Vector2 value)
		{
			var delta = (Vector3) value - this.visualTarget.transform.localScale;
			var cmd = new ResizeCommand(this.visualTarget.GetComponent<EditorActionHandler>(), delta, 0);
			this.editor.commandHistory.Add(cmd);
			this.editor.UpdateRopeAttachments(false);
		}

		private void RotationChanged(float value, TextSliderInput.ChangeType type)
		{
			if (type == TextSliderInput.ChangeType.ChangeStart)
			{
				this.editor.commandHistory.PreventNextMerge();
			}

			var fromRotation = this.visualTarget.transform.rotation;
			var toRotation = Quaternion.AngleAxis(value, Vector3.forward);
			var actionHandler = this.visualTarget.GetComponent<EditorActionHandler>();
			var cmd = new RotateCommand(actionHandler, fromRotation, toRotation);
			this.editor.commandHistory.Add(cmd, true);

			if (type == TextSliderInput.ChangeType.ChangeEnd)
			{
				this.editor.UpdateRopeAttachments(false);
			}
		}

		private void OnClickAnimationButton()
		{
			if (this.animationWindow.gameObject.activeSelf)
			{
				this.animationWindow.Close();
			}
			else
			{
				this.animationWindow.Open();
			}
		}
	}
}

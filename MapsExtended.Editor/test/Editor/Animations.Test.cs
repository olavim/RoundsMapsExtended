using System.Collections;
using FluentAssertions;
using MapsExt.MapObjects;
using UnityEngine;
using Surity;
using MapsExt.Editor.ActionHandlers;
using MapsExt.MapObjects.Properties;
using System;

namespace MapsExt.Editor.Tests
{
	[TestClass]
	public class AnimationTests
	{
		private MapEditor editor;
		private SimulatedInputSource inputSource;
		private EditorTestUtils utils;

		private MapEditorAnimationHandler animationHandler;
		private MapObjectAnimation animation;
		private GameObject animationMapObject;

		[BeforeAll]
		public void SetInputSource()
		{
			var rootGo = MapsExtendedEditor.instance.gameObject;
			this.inputSource = rootGo.AddComponent<SimulatedInputSource>();
			EditorInput.SetInputSource(this.inputSource);
		}

		[AfterAll]
		public void ResetInputSource()
		{
			GameObject.Destroy(this.inputSource);
			this.inputSource = null;
			EditorInput.SetInputSource(EditorInput.DefaultInputSource);
		}

		[AfterAll]
		[BeforeEach]
		public IEnumerator OpenEditor()
		{
			yield return MapsExtendedEditor.instance.OpenEditorCoroutine();
			var rootGo = GameObject.Find("/Map");
			this.editor = rootGo.GetComponentInChildren<MapEditor>();
			this.utils = new EditorTestUtils(this.editor, this.inputSource);
			this.animationHandler = this.editor.animationHandler;
		}

		[AfterEach]
		public IEnumerator CloseEditor()
		{
			yield return MapsExtendedEditor.instance.CloseEditorCoroutine();
			this.editor = null;
			this.utils = null;
			this.animationHandler = null;
		}

		[BeforeEach]
		public IEnumerator AddAnimation()
		{
			yield return this.SpawnWithAnimation<BoxData>();
			this.animation = this.editor.animationHandler.animation;
			this.animationMapObject = this.animation.gameObject;
		}

		[Test]
		public void Test_AddAnimation()
		{
			this.editor.content.transform.GetChild(0).gameObject.Should().BeSameAs(this.animationMapObject);
			this.animationMapObject.Should().BeSameAs(this.animation.gameObject);
			this.animationMapObject.GetComponent<MapObjectAnimation>().Should().BeSameAs(this.animation);
			this.editor.ActiveObject.Should().NotBeSameAs(this.animationMapObject);
			this.editor.ActiveObject.Should().BeSameAs(this.animationHandler.keyframeMapObject);
		}

		[Test]
		public void Test_AddKeyframe()
		{
			this.animationHandler.AddKeyframe();
			this.animation.Keyframes.Count.Should().Be(2);
			this.animationMapObject.Should().NotBeSameAs(this.animationHandler.keyframeMapObject);
			this.editor.ActiveObject.Should().BeSameAs(this.animationHandler.keyframeMapObject);
		}

		[Test]
		public void Test_DeleteKeyframe()
		{
			this.animationHandler.AddKeyframe();
			this.animationHandler.DeleteKeyframe(1);
			this.animation.Keyframes.Count.Should().Be(1);
		}

		[Test]
		public void Test_ToggleAnimation()
		{
			this.animationHandler.ToggleAnimation(this.animationMapObject);
			this.editor.ActiveObject.Should().BeNull();
			this.animationHandler.ToggleAnimation(this.animationMapObject);
			this.editor.ActiveObject.Should().BeSameAs(this.animationHandler.keyframeMapObject);
		}

		private void GeneratePropertyTests<T>(int keyframeCount, T prop1, T prop2, Func<int, T> KeyframeProperty) where T : IProperty
		{
			for (int i = 1; i < keyframeCount; i++)
			{
				this.animationHandler.AddKeyframe();
			}

			this.animationMapObject.GetHandlerValue<T>().Should().Be(prop1);

			for (int i = 0; i < keyframeCount; i++)
			{
				this.animationHandler.SetKeyframe(i);
				var nextProp = KeyframeProperty(i);
				this.GetKeyframeValue<T>(i).Should().Be(prop1);
				this.animationHandler.keyframeMapObject.SetHandlerValue(nextProp);
				this.animationHandler.keyframeMapObject.GetHandlerValue<T>().Should().Be(nextProp);
				this.GetKeyframeValue<T>(i).Should().Be(nextProp);
			}

			this.animationMapObject.GetHandlerValue<T>().Should().Be(KeyframeProperty(0));

			this.animationHandler.ToggleAnimation(this.animationMapObject);

			this.animationMapObject.GetHandlerValue<T>().Should().Be(KeyframeProperty(0));
			this.animationMapObject.SetHandlerValue(prop2);

			for (int i = 0; i < keyframeCount; i++)
			{
				this.GetKeyframeValue<T>(i).Should().Be(KeyframeProperty(i));
			}

			this.animationHandler.ToggleAnimation(this.animationMapObject);
			this.GetKeyframeValue<T>(0).Should().Be(prop2);

			for (int i = 1; i < keyframeCount; i++)
			{
				this.animationHandler.DeleteKeyframe(1);
			}
			this.animationHandler.keyframeMapObject.SetHandlerValue(prop1);
		}

		[Test]
		public void Test_MoveKeyframe()
		{
			var prop1 = new PositionProperty(0, 0);
			var prop2 = new PositionProperty(5, 5);

			PositionProperty KeyframeProperty(int keyframe)
			{
				var delta = new Vector2(1, 1) * (keyframe + 1);
				return new PositionProperty(prop1.Value + delta);
			}

			for (int i = 1; i <= 4; i++)
			{
				this.GeneratePropertyTests(i, prop1, prop2, KeyframeProperty);
			}
		}

		[Test]
		public IEnumerator Test_MoveKeyframeWithMouse()
		{
			var delta = new PositionProperty(1, 1);
			yield return this.utils.MoveSelectedWithMouse(delta);

			var exceptedPosition = new PositionProperty(1, 1);
			this.editor.ActiveObject.GetHandlerValue<PositionProperty>().Should().Be(exceptedPosition);
			this.GetKeyframeValue<PositionProperty>(0).Should().Be(exceptedPosition);
		}

		[Test]
		public void Test_ResizeKeyframe()
		{
			var prop1 = new ScaleProperty(2, 2);
			var prop2 = new ScaleProperty(5, 5);

			ScaleProperty KeyframeProperty(int keyframe)
			{
				var delta = new Vector2(1, 1) * (keyframe + 1);
				return new ScaleProperty(prop1.Value + delta);
			}

			for (int i = 1; i <= 4; i++)
			{
				this.GeneratePropertyTests(i, prop1, prop2, KeyframeProperty);
			}
		}

		[Test]
		public IEnumerator Test_ResizeKeyframeWithMouse()
		{
			var delta = new ScaleProperty(1, 1);
			yield return this.utils.ResizeSelectedWithMouse(delta, AnchorPosition.MiddleRight);

			var expectedScale = new ScaleProperty(3, 2);
			this.editor.ActiveObject.GetHandlerValue<ScaleProperty>().Should().Be(expectedScale);
			this.GetKeyframeValue<ScaleProperty>(0).Should().Be(expectedScale);
		}

		[Test]
		public void Test_RotateKeyframe()
		{
			var prop1 = new RotationProperty(Quaternion.identity);
			var prop2 = new RotationProperty(Quaternion.Euler(0, 0, 45));

			RotationProperty KeyframeProperty(int keyframe)
			{
				float angle = 15 * (keyframe + 1);
				return new RotationProperty(Quaternion.Euler(0, 0, angle));
			}

			for (int i = 1; i <= 4; i++)
			{
				this.GeneratePropertyTests(i, prop1, prop2, KeyframeProperty);
			}
		}

		[Test]
		public IEnumerator Test_RotateKeyframeWithMouse()
		{
			yield return this.utils.RotateSelectedWithMouse(45);

			var expectedRotation = new RotationProperty(45);
			this.editor.ActiveObject.GetHandlerValue<RotationProperty>().Should().Be(expectedRotation);
			this.GetKeyframeValue<RotationProperty>(0).Should().Be(expectedRotation);
		}

		private IEnumerator SpawnWithAnimation<T>() where T : MapObjectData
		{
			yield return this.utils.SpawnMapObject<T>();
			this.editor.animationHandler.AddAnimation(this.editor.ActiveObject);
		}

		private T GetKeyframeValue<T>(int keyframeIndex) where T : IProperty
		{
			return this.animation.Keyframes[keyframeIndex].GetComponentValue<T>();
		}
	}
}

using System.Collections;
using FluentAssertions;
using MapsExt.MapObjects;
using UnityEngine;
using Surity;
using MapsExt.Properties;
using System;
using MapsExt.Editor.Properties;

namespace MapsExt.Editor.Tests
{
	[TestClass]
	internal class AnimationTests : EditorTestBase
	{
		private MapObjectAnimation animation;
		private GameObject animationMapObject;

		[BeforeEach]
		public IEnumerator AddAnimation()
		{
			yield return this.SpawnWithAnimation<BoxData>();
			this.animation = this.Editor.AnimationHandler.Animation;
			this.animationMapObject = this.animation.gameObject;
		}

		[Test]
		public void Test_AddAnimation()
		{
			this.Editor.Content.transform.GetChild(0).gameObject.Should().BeSameAs(this.animationMapObject);
			this.animationMapObject.Should().BeSameAs(this.animation.gameObject);
			this.animationMapObject.GetComponent<MapObjectAnimation>().Should().BeSameAs(this.animation);
			this.Editor.ActiveMapObjectPart.Should().NotBeSameAs(this.animationMapObject);
			this.Editor.ActiveMapObjectPart.Should().BeSameAs(this.AnimationHandler.KeyframeMapObject);
		}

		[Test]
		public void Test_AddKeyframe()
		{
			this.AnimationHandler.AddKeyframe();
			this.animation.Keyframes.Count.Should().Be(2);
			this.animationMapObject.Should().NotBeSameAs(this.AnimationHandler.KeyframeMapObject);
			this.Editor.ActiveMapObjectPart.Should().BeSameAs(this.AnimationHandler.KeyframeMapObject);
		}

		[Test]
		public void Test_DeleteKeyframe()
		{
			this.AnimationHandler.AddKeyframe();
			this.AnimationHandler.DeleteKeyframe(1);
			this.animation.Keyframes.Count.Should().Be(1);
		}

		[Test]
		public void Test_ToggleAnimation()
		{
			this.AnimationHandler.ToggleAnimation(this.animationMapObject);
			this.Editor.ActiveMapObjectPart.Should().BeNull();
			this.AnimationHandler.ToggleAnimation(this.animationMapObject);
			this.Editor.ActiveMapObjectPart.Should().BeSameAs(this.AnimationHandler.KeyframeMapObject);
		}

		private IEnumerator GeneratePropertyTests<T>(int keyframeCount, T prop1, T prop2, Func<int, T> KeyframeProperty) where T : IProperty
		{
			for (int i = 1; i < keyframeCount; i++)
			{
				this.AnimationHandler.AddKeyframe();
			}

			this.animationMapObject.ReadProperty<T>().Should().Be(prop1);

			for (int i = 0; i < keyframeCount; i++)
			{
				this.AnimationHandler.SetKeyframe(i);
				var nextProp = KeyframeProperty(i);
				this.GetKeyframeValue<T>(i).Should().Be(prop1);
				this.AnimationHandler.KeyframeMapObject.WriteProperty(nextProp);
				this.AnimationHandler.KeyframeMapObject.ReadProperty<T>().Should().Be(nextProp);
				yield return null;
				this.GetKeyframeValue<T>(i).Should().Be(nextProp);
			}

			this.animationMapObject.ReadProperty<T>().Should().Be(KeyframeProperty(0));

			this.AnimationHandler.ToggleAnimation(this.animationMapObject);

			this.animationMapObject.ReadProperty<T>().Should().Be(KeyframeProperty(0));
			this.animationMapObject.WriteProperty(prop2);
			this.GetKeyframeValue<T>(0).Should().Be(prop2);
			yield return null;

			for (int i = 1; i < keyframeCount; i++)
			{
				this.GetKeyframeValue<T>(i).Should().Be(KeyframeProperty(i));
			}

			this.AnimationHandler.ToggleAnimation(this.animationMapObject);
			this.GetKeyframeValue<T>(0).Should().Be(prop2);

			for (int i = 1; i < keyframeCount; i++)
			{
				this.AnimationHandler.DeleteKeyframe(1);
			}
			this.AnimationHandler.KeyframeMapObject.WriteProperty(prop1);
		}

		[Test]
		public IEnumerator Test_MoveKeyframe()
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
				yield return this.GeneratePropertyTests(i, prop1, prop2, KeyframeProperty);
			}
		}

		[Test]
		public IEnumerator Test_MoveKeyframeWithMouse()
		{
			var delta = new PositionProperty(1, 1);
			yield return this.Utils.MoveSelectedWithMouse(delta);

			var exceptedPosition = new PositionProperty(1, 1);
			this.Editor.ActiveMapObjectPart.ReadProperty<PositionProperty>().Should().Be(exceptedPosition);
			this.GetKeyframeValue<PositionProperty>(0).Should().Be(exceptedPosition);
		}

		[Test]
		public IEnumerator Test_ResizeKeyframe()
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
				yield return this.GeneratePropertyTests(i, prop1, prop2, KeyframeProperty);
			}
		}

		[Test]
		public IEnumerator Test_ResizeKeyframeWithMouse()
		{
			var delta = new ScaleProperty(1, 1);
			yield return this.Utils.ResizeSelectedWithMouse(delta, Direction2D.East);

			var expectedScale = new ScaleProperty(3, 2);
			this.Editor.ActiveMapObjectPart.ReadProperty<ScaleProperty>().Should().Be(expectedScale);
			this.GetKeyframeValue<ScaleProperty>(0).Should().Be(expectedScale);
		}

		[Test]
		public IEnumerator Test_RotateKeyframe()
		{
			var prop1 = new RotationProperty();
			var prop2 = new RotationProperty(45);

			static RotationProperty KeyframeProperty(int keyframe)
			{
				return 15 * (keyframe + 1);
			}

			for (int i = 1; i <= 4; i++)
			{
				yield return this.GeneratePropertyTests(i, prop1, prop2, KeyframeProperty);
			}
		}

		[Test]
		public IEnumerator Test_RotateKeyframeWithMouse()
		{
			yield return this.Utils.RotateSelectedWithMouse(45);

			var expectedRotation = new RotationProperty(45);
			this.Editor.ActiveMapObjectPart.ReadProperty<RotationProperty>().Should().Be(expectedRotation);
			this.GetKeyframeValue<RotationProperty>(0).Should().Be(expectedRotation);
		}

		private IEnumerator SpawnWithAnimation<T>() where T : MapObjectData
		{
			yield return this.Utils.SpawnMapObject<T>();
			this.Editor.AnimationHandler.AddAnimation(this.Editor.ActiveMapObjectPart);
		}

		private T GetKeyframeValue<T>(int keyframeIndex) where T : IProperty
		{
			return this.animationMapObject.ReadProperty<AnimationProperty>().Keyframes[keyframeIndex].GetComponentValue<T>();
		}
	}
}

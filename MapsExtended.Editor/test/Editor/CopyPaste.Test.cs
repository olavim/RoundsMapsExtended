using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using MapsExt.Editor.MapObjects;
using MapsExt.Properties;
using Surity;
using UnityEngine;
using MapsExt.MapObjects;
using System.Linq;
using MapsExt.Editor.Properties;

namespace MapsExt.Editor.Tests
{
	[TestClass]
	internal class CopyPasteTests : EditorTestBase
	{
		[TestGenerator]
		public IEnumerable<TestInfo> Gen_PropertiesAreCopied()
		{
			foreach (var type in this.Utils.GetMapObjects())
			{
				var dataType = type.GetCustomAttribute<EditorMapObjectAttribute>().DataType;
				yield return new TestInfo($"Gen_PropertiesAreCopied > {type.Name}", () => this.Test_PropertiesAreCopied(dataType));
			}
		}

		private IEnumerator Test_PropertiesAreCopied(Type type)
		{
			yield return this.Utils.SpawnMapObject(type);
			var obj = this.Editor.ActiveMapObject;

			// Set properties to some non-default values
			this.Editor.ActiveMapObject.TrySetEditorMapObjectProperty(new PositionProperty(5, 5));
			this.Editor.ActiveMapObject.TrySetEditorMapObjectProperty(new ScaleProperty(5, 5));
			this.Editor.ActiveMapObject.TrySetEditorMapObjectProperty(new RotationProperty(25));
			this.Editor.ActiveMapObject.TrySetEditorMapObjectProperty(new DamageableProperty(false));
			this.Editor.ActiveMapObject.TrySetEditorMapObjectProperty(new RopePositionProperty(new(-5, -5), new(5, 5)));

			var pos1 = this.Editor.ActiveMapObject.GetEditorMapObjectProperty<PositionProperty>();
			var size1 = this.Editor.ActiveMapObject.GetEditorMapObjectProperty<ScaleProperty>();
			var rot1 = this.Editor.ActiveMapObject.GetEditorMapObjectProperty<RotationProperty>();
			var dmg1 = this.Editor.ActiveMapObject.GetEditorMapObjectProperty<DamageableProperty>();
			var rpos1 = this.Editor.ActiveMapObject.GetEditorMapObjectProperty<RopePositionProperty>();

			this.Editor.OnCopy();
			yield return this.Editor.OnPaste();

			this.Editor.ActiveMapObject.Should().NotBeSameAs(obj);

			var pos2 = this.Editor.ActiveMapObject.GetEditorMapObjectProperty<PositionProperty>();
			(pos2 is null).Should().Be(pos1 is null);
			pos2?.Should().Be(pos1 + new PositionProperty(1, -1));

			var size2 = this.Editor.ActiveMapObject.GetEditorMapObjectProperty<ScaleProperty>();
			size2.Should().Be(size1);

			var rot2 = this.Editor.ActiveMapObject.GetEditorMapObjectProperty<RotationProperty>();
			rot2.Should().Be(rot1);

			var dmg2 = this.Editor.ActiveMapObject.GetEditorMapObjectProperty<DamageableProperty>();
			dmg2.Should().Be(dmg1);

			var rpos2 = this.Editor.ActiveMapObject.GetEditorMapObjectProperty<RopePositionProperty>();
			(rpos2 is null).Should().Be(rpos1 is null);
			rpos2?.Should().Be(
				new RopePositionProperty(
					rpos1.StartPosition + new Vector2(1, -1),
					rpos1.EndPosition + new Vector2(1, -1)
				)
			);
		}

		[Test]
		public IEnumerator Test_GroupPropertiesAreCopied()
		{
			foreach (var type in this.Utils.GetMapObjects())
			{
				var dataType = type.GetCustomAttribute<EditorMapObjectAttribute>().DataType;
				yield return this.Utils.SpawnMapObject(dataType);
			}

			var pos1 = new List<PositionProperty>();
			var size1 = new List<ScaleProperty>();
			var rot1 = new List<RotationProperty>();
			var dmg1 = new List<DamageableProperty>();
			var rpos1 = new List<RopePositionProperty>();

			foreach (var mapObject in this.Editor.MapObjects)
			{
				// Set properties to some non-default values
				mapObject.TrySetEditorMapObjectProperty(new PositionProperty(5, 5));
				mapObject.TrySetEditorMapObjectProperty(new ScaleProperty(5, 5));
				mapObject.TrySetEditorMapObjectProperty(new RotationProperty(25));
				mapObject.TrySetEditorMapObjectProperty(new DamageableProperty(false));
				mapObject.TrySetEditorMapObjectProperty(new RopePositionProperty(new(-5, -5), new(5, 5)));

				pos1.Add(mapObject.GetEditorMapObjectProperty<PositionProperty>());
				size1.Add(mapObject.GetEditorMapObjectProperty<ScaleProperty>());
				rot1.Add(mapObject.GetEditorMapObjectProperty<RotationProperty>());
				dmg1.Add(mapObject.GetEditorMapObjectProperty<DamageableProperty>());
				rpos1.Add(mapObject.GetEditorMapObjectProperty<RopePositionProperty>());
			}

			this.Editor.SelectAll();
			int originalSelectedCount = this.Editor.SelectedMapObjects.Count();

			this.Editor.OnCopy();
			yield return this.Editor.OnPaste();

			var selectedMapObjects = this.Editor.SelectedMapObjects.ToList();

			selectedMapObjects.Count.Should().Be(originalSelectedCount);

			for (int i = 0; i < selectedMapObjects.Count; i++)
			{
				var pos2 = selectedMapObjects[i].GetEditorMapObjectProperty<PositionProperty>();
				pos1[i]?.Should().Be(pos2 + new PositionProperty(-1, 1));

				var size2 = selectedMapObjects[i].GetEditorMapObjectProperty<ScaleProperty>();
				size1[i].Should().Be(size2);

				var rot2 = selectedMapObjects[i].GetEditorMapObjectProperty<RotationProperty>();
				rot1[i].Should().Be(rot2);

				var dmg2 = selectedMapObjects[i].GetEditorMapObjectProperty<DamageableProperty>();
				dmg1[i].Should().Be(dmg2);

				var rpos2 = selectedMapObjects[i].GetEditorMapObjectProperty<RopePositionProperty>();
				rpos1[i]?.Should().Be(
					new RopePositionProperty(
						rpos2.StartPosition + new Vector2(-1, 1),
						rpos2.EndPosition + new Vector2(-1, 1)
					)
				);
			}
		}

		[Test]
		public IEnumerator Test_AnimationIsCopied()
		{
			yield return this.Utils.SpawnMapObject<BoxData>();
			var anim1 = new AnimationProperty(
				new AnimationKeyframe(new PositionProperty(), new ScaleProperty(), new RotationProperty()),
				new AnimationKeyframe(new PositionProperty(1, 0), new ScaleProperty(2, 1), new RotationProperty(45))
			);
			this.Editor.ActiveMapObject.SetEditorMapObjectProperty(anim1);

			this.Editor.OnCopy();
			yield return this.Editor.OnPaste();

			var anim2 = this.Editor.ActiveMapObject.GetEditorMapObjectProperty<AnimationProperty>();
			anim2.Keyframes.Length.Should().Be(anim1.Keyframes.Length);

			for (int i = 0; i < anim1.Keyframes.Length; i++)
			{
				anim2.Keyframes[i].GetComponentValue<PositionProperty>().Should().Be(anim1.Keyframes[i].GetComponentValue<PositionProperty>());
				anim2.Keyframes[i].GetComponentValue<ScaleProperty>().Should().Be(anim1.Keyframes[i].GetComponentValue<ScaleProperty>());
				anim2.Keyframes[i].GetComponentValue<RotationProperty>().Should().Be(anim1.Keyframes[i].GetComponentValue<RotationProperty>());
			}
		}
	}
}

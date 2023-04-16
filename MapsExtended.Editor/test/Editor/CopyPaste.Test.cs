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

			var data1 = MapsExtendedEditor.instance.MapObjectManager.Serialize(this.Editor.ActiveMapObject);

			// Set properties to some non-default values
			MapsExtendedEditor.instance.PropertyManager.TrySetProperty(data1, new PositionProperty(5, 5));
			MapsExtendedEditor.instance.PropertyManager.TrySetProperty(data1, new ScaleProperty(5, 5));
			MapsExtendedEditor.instance.PropertyManager.TrySetProperty(data1, new RotationProperty(25));
			MapsExtendedEditor.instance.PropertyManager.TrySetProperty(data1, new DamageableProperty(false));
			MapsExtendedEditor.instance.PropertyManager.TrySetProperty(data1, new RopePositionProperty(new(-5, -5), new(5, 5)));
			MapsExtendedEditor.instance.MapObjectManager.Deserialize(data1, this.Editor.ActiveMapObject);

			this.Editor.OnCopy();
			yield return this.Editor.OnPaste();

			this.Editor.ActiveMapObject.Should().NotBeSameAs(obj);

			var data2 = MapsExtendedEditor.instance.MapObjectManager.Serialize(this.Editor.ActiveMapObject);

			var pos1 = MapsExtendedEditor.instance.PropertyManager.GetProperty<PositionProperty>(data1);
			var pos2 = MapsExtendedEditor.instance.PropertyManager.GetProperty<PositionProperty>(data2);
			pos1?.Should().Be(pos2 + new PositionProperty(-1, 1));

			var size1 = MapsExtendedEditor.instance.PropertyManager.GetProperty<ScaleProperty>(data1);
			var size2 = MapsExtendedEditor.instance.PropertyManager.GetProperty<ScaleProperty>(data2);
			size1.Should().Be(size2);

			var rot1 = MapsExtendedEditor.instance.PropertyManager.GetProperty<RotationProperty>(data1);
			var rot2 = MapsExtendedEditor.instance.PropertyManager.GetProperty<RotationProperty>(data2);
			rot1.Should().Be(rot2);

			var dmg1 = MapsExtendedEditor.instance.PropertyManager.GetProperty<DamageableProperty>(data1);
			var dmg2 = MapsExtendedEditor.instance.PropertyManager.GetProperty<DamageableProperty>(data2);
			dmg1.Should().Be(dmg2);

			var rpos1 = MapsExtendedEditor.instance.PropertyManager.GetProperty<RopePositionProperty>(data1);
			var rpos2 = MapsExtendedEditor.instance.PropertyManager.GetProperty<RopePositionProperty>(data2);
			rpos1?.Should().Be(
				new RopePositionProperty(
					rpos2.StartPosition + new Vector2(-1, 1),
					rpos2.EndPosition + new Vector2(-1, 1)
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

			var originalData = new List<MapObjectData>();

			foreach (var mapObject in this.Editor.MapObjects)
			{
				var data = MapsExtendedEditor.instance.MapObjectManager.Serialize(mapObject);

				// Set properties to some non-default values
				MapsExtendedEditor.instance.PropertyManager.TrySetProperty(data, new PositionProperty(5, 5));
				MapsExtendedEditor.instance.PropertyManager.TrySetProperty(data, new ScaleProperty(5, 5));
				MapsExtendedEditor.instance.PropertyManager.TrySetProperty(data, new RotationProperty(25));
				MapsExtendedEditor.instance.PropertyManager.TrySetProperty(data, new DamageableProperty(false));
				MapsExtendedEditor.instance.PropertyManager.TrySetProperty(data, new RopePositionProperty(new(-5, -5), new(5, 5)));
				MapsExtendedEditor.instance.MapObjectManager.Deserialize(data, mapObject);

				originalData.Add(data);
			}

			this.Editor.SelectAll();
			this.Editor.OnCopy();
			yield return this.Editor.OnPaste();

			var selectedMapObjects = this.Editor.SelectedMapObjects.ToList();

			selectedMapObjects.Count.Should().Be(originalData.Count);

			for (int i = 0; i < selectedMapObjects.Count; i++)
			{
				var data1 = originalData[i];
				var data2 = MapsExtendedEditor.instance.MapObjectManager.Serialize(selectedMapObjects[i]);

				var pos1 = MapsExtendedEditor.instance.PropertyManager.GetProperty<PositionProperty>(data1);
				var pos2 = MapsExtendedEditor.instance.PropertyManager.GetProperty<PositionProperty>(data2);
				pos1?.Should().Be(pos2 + new PositionProperty(-1, 1));

				var size1 = MapsExtendedEditor.instance.PropertyManager.GetProperty<ScaleProperty>(data1);
				var size2 = MapsExtendedEditor.instance.PropertyManager.GetProperty<ScaleProperty>(data2);
				size1.Should().Be(size2);

				var rot1 = MapsExtendedEditor.instance.PropertyManager.GetProperty<RotationProperty>(data1);
				var rot2 = MapsExtendedEditor.instance.PropertyManager.GetProperty<RotationProperty>(data2);
				rot1.Should().Be(rot2);

				var dmg1 = MapsExtendedEditor.instance.PropertyManager.GetProperty<DamageableProperty>(data1);
				var dmg2 = MapsExtendedEditor.instance.PropertyManager.GetProperty<DamageableProperty>(data2);
				dmg1.Should().Be(dmg2);

				var rpos1 = MapsExtendedEditor.instance.PropertyManager.GetProperty<RopePositionProperty>(data1);
				var rpos2 = MapsExtendedEditor.instance.PropertyManager.GetProperty<RopePositionProperty>(data2);
				rpos1?.Should().Be(
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
			var data1 = (BoxData) MapsExtendedEditor.instance.MapObjectManager.Serialize(this.Editor.ActiveMapObject);
			data1.Animation = new AnimationProperty(
				new AnimationKeyframe(new PositionProperty(), new ScaleProperty(), new RotationProperty()),
				new AnimationKeyframe(new PositionProperty(1, 0), new ScaleProperty(2, 1), new RotationProperty(45))
			);
			MapsExtendedEditor.instance.MapObjectManager.Deserialize(data1, this.Editor.ActiveMapObject);

			this.Editor.OnCopy();
			yield return this.Editor.OnPaste();

			var data2 = (BoxData) MapsExtendedEditor.instance.MapObjectManager.Serialize(this.Editor.ActiveMapObject);
			data2.Animation.Keyframes.Length.Should().Be(data1.Animation.Keyframes.Length);

			for (int i = 0; i < data1.Animation.Keyframes.Length; i++)
			{
				data2.Animation.Keyframes[i].GetComponentValue<PositionProperty>().Should().Be(data1.Animation.Keyframes[i].GetComponentValue<PositionProperty>());
				data2.Animation.Keyframes[i].GetComponentValue<ScaleProperty>().Should().Be(data1.Animation.Keyframes[i].GetComponentValue<ScaleProperty>());
				data2.Animation.Keyframes[i].GetComponentValue<RotationProperty>().Should().Be(data1.Animation.Keyframes[i].GetComponentValue<RotationProperty>());
			}
		}
	}
}

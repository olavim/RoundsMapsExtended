using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MapsExt.Editor.Events;
using MapsExt.Editor.MapObjects;
using MapsExt.Editor.Properties;
using MapsExt.MapObjects;
using MapsExt.Properties;
using Surity;
using UnityEngine;

namespace MapsExt.Editor.Tests
{
	[TestClass]
	internal class HistoryTests : EditorTestBase
	{
		[Test]
		public IEnumerator Test_Spawn()
		{
			yield return this.Utils.SpawnMapObject<BoxData>();
			var go = this.Editor.ActiveObject;
			var id = go.GetComponent<MapObjectInstance>().MapObjectId;

			this.Editor.Undo();
			this.Editor.Content.transform.childCount.Should().Be(0);
			this.Editor.ActiveObject.Should().BeNull();
			this.Editor.Redo();
			this.Editor.Content.transform.childCount.Should().Be(1);
			this.Editor.ActiveObject.Should().BeNull();

			var instance = this.Editor.Content.transform.GetChild(0).GetComponent<MapObjectInstance>();
			instance.MapObjectId.Should().Be(id);
		}

		[Test]
		public IEnumerator Test_MoveWithMouse()
		{
			yield return this.Utils.SpawnMapObject<BoxData>();
			var go = this.Editor.ActiveObject;

			var pos1 = go.ReadProperty<PositionProperty>();
			var pos2 = new PositionProperty(1, 0);

			yield return this.Utils.MoveSelectedWithMouse(pos2);
			go.ReadProperty<PositionProperty>().Should().Be(pos2);

			this.Editor.Undo();
			go.ReadProperty<PositionProperty>().Should().Be(pos1);
			this.Editor.Redo();
			go.ReadProperty<PositionProperty>().Should().Be(pos2);
		}

		[Test]
		public IEnumerator Test_ResizeWithMouse()
		{
			yield return this.Utils.SpawnMapObject<BoxData>();
			var go = this.Editor.ActiveObject;

			var size1 = go.ReadProperty<ScaleProperty>();
			var size2 = new ScaleProperty(4, 2);

			yield return this.Utils.ResizeSelectedWithMouse(size2 - size1, Direction2D.East);
			go.ReadProperty<ScaleProperty>().Should().Be(size2);

			this.Editor.Undo();
			go.ReadProperty<ScaleProperty>().Should().Be(size1);
			this.Editor.Redo();
			go.ReadProperty<ScaleProperty>().Should().Be(size2);
		}

		[Test]
		public IEnumerator Test_RotateWithMouse()
		{
			yield return this.Utils.SpawnMapObject<BoxData>();
			var go = this.Editor.ActiveObject;

			var rot1 = go.ReadProperty<RotationProperty>();
			var rot2 = new RotationProperty(45);

			yield return this.Utils.RotateSelectedWithMouse(rot2 - rot1);
			go.ReadProperty<RotationProperty>().Should().Be(rot2);

			this.Editor.Undo();
			go.ReadProperty<RotationProperty>().Should().Be(rot1);
			this.Editor.Redo();
			go.ReadProperty<RotationProperty>().Should().Be(rot2);
		}

		[Test]
		public IEnumerator Test_MoveWithNudge()
		{
			yield return this.Utils.SpawnMapObject<BoxData>();
			var go = this.Editor.ActiveObject;

			var pos1 = go.ReadProperty<PositionProperty>();
			var pos2 = new PositionProperty(0.25f, 0);

			this.InputSource.SetKey(KeyCode.RightArrow);
			yield return null;
			this.InputSource.SetKey(KeyCode.RightArrow, false);
			go.ReadProperty<PositionProperty>().Should().Be(pos2);

			this.Editor.Undo();
			go.ReadProperty<PositionProperty>().Should().Be(pos1);
			this.Editor.Redo();
			go.ReadProperty<PositionProperty>().Should().Be(pos2);
		}

		[Test]
		public IEnumerator Test_RopeAttachment()
		{
			yield return this.Utils.SpawnMapObject<BoxData>();
			var boxGo = this.Editor.ActiveObject;
			yield return this.Utils.SpawnMapObject<RopeData>();
			var rope = this.Editor.SelectedObjects.First().GetComponentInParent<EditorRope.RopeInstance>();

			var list = new List<Vector2>
			{
				rope.GetAnchor(0).GetAnchoredPosition()
			};

			rope.WriteProperty(new RopePositionProperty(new(0.25f, 0.5f), new(0, 5)));
			this.Editor.TakeSnaphot();

			list.Add(rope.GetAnchor(0).GetAnchoredPosition());

			this.Editor.ClearSelected();
			this.Editor.AddSelected(boxGo);

			yield return this.Utils.MoveSelectedWithMouse(new Vector3(1, 0, 0));
			list.Add(rope.GetAnchor(0).GetAnchoredPosition());

			yield return this.Utils.ResizeSelectedWithMouse(new Vector3(4, 2, 0), Direction2D.East);
			list.Add(rope.GetAnchor(0).GetAnchoredPosition());

			yield return this.Utils.RotateSelectedWithMouse(45);
			list.Add(rope.GetAnchor(0).GetAnchoredPosition());

			this.Editor.DeleteSelectedMapObjects();
			yield return null;

			this.Editor.ActiveObject.Should().BeNull();
			this.Editor.Content.transform.childCount.Should().Be(1);

			var iter = ListIterator.From(list);

			this.Editor.Undo(); // Undo delete
			this.Editor.Content.transform.childCount.Should().Be(2);

			rope.GetAnchor(0).GetAnchoredPosition().Should().BeApproximately(iter.MoveLast().Current);
			this.Editor.Undo(); // Undo rotate
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MovePrevious().Current);
			this.Editor.Undo(); // Undo resize
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MovePrevious().Current);
			this.Editor.Undo(); // Undo move
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MovePrevious().Current);
			this.Editor.Undo(); // Undo rope position
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MovePrevious().Current);
			this.Editor.Undo(); // Undo spawn rope
			this.Editor.Undo(); // Undo spawn box
			this.Editor.Content.transform.childCount.Should().Be(0);

			this.Editor.Redo(); // Redo spawn box
			this.Editor.Redo(); // Redo spawn rope
			this.Editor.Content.transform.childCount.Should().Be(2);
			rope = this.Editor.Content.transform.GetChild(1).GetComponentInParent<EditorRope.RopeInstance>();
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.Current);

			this.Editor.Redo(); // Redo rope position
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MoveNext().Current);

			this.Editor.Redo(); // Redo move
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MoveNext().Current);
			this.Editor.Redo(); // Redo resize
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MoveNext().Current);
			this.Editor.Redo(); // Redo rotate
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MoveNext().Current);
			this.Editor.Redo(); // Redo Delete
			this.Editor.ActiveObject.Should().BeNull();
			this.Editor.Content.transform.childCount.Should().Be(1);
		}

		[Test]
		public IEnumerator Test_V0Box_MoveWithMouse()
		{
			yield return this.Utils.SpawnMapObject<V0Box>();
			var go = this.Editor.ActiveObject;
			((Vector2) go.transform.position).Should().Be(new(0, 0));

			yield return this.Utils.MoveSelectedWithMouse(new(1, 1));
			((Vector2) go.transform.position).Should().Be(new(1, 1));

			this.Editor.Undo();
			((Vector2) go.transform.position).Should().Be(new(0, 0));
			this.Editor.Redo();
			((Vector2) go.transform.position).Should().Be(new(1, 1));
		}

		[Test]
		public IEnumerator Test_Inspector_Position()
		{
			yield return this.Utils.SpawnMapObject<BoxData>();

			var element = (PositionElement) this.Inspector.GetElement<PositionProperty>();

			var val1 = new PositionProperty(0, 0);
			var val2 = new PositionProperty(1, 1);

			element.Value.Should().Be(val1.Value);
			element.Value = val2.Value;
			element.Value.Should().Be(val2.Value);
			this.Editor.Undo();
			element.Value.Should().Be(val1.Value);
			this.Editor.Redo();
			element.Value.Should().Be(val2.Value);
		}

		[Test]
		public IEnumerator Test_Inspector_Scale()
		{
			yield return this.Utils.SpawnMapObject<BoxData>();

			var element = (ScaleElement) this.Inspector.GetElement<ScaleProperty>();

			var val1 = new ScaleProperty(2, 2);
			var val2 = new ScaleProperty(3, 3);

			element.Value.Should().Be(val1.Value);
			element.Value = val2.Value;
			element.Value.Should().Be(val2.Value);
			this.Editor.Undo();
			element.Value.Should().Be(val1.Value);
			this.Editor.Redo();
			element.Value.Should().Be(val2.Value);
		}

		[Test]
		public IEnumerator Test_Inspector_Rotation()
		{
			yield return this.Utils.SpawnMapObject<BoxData>();

			var element = (RotationElement) this.Inspector.GetElement<RotationProperty>();

			var val1 = new RotationProperty();
			var val2 = new RotationProperty(45);

			element.Value.Should().Be(val1.Value);
			element.Value = val2.Value;
			element.Value.Should().Be(val2.Value);
			this.Editor.Undo();
			element.Value.Should().Be(val1.Value);
			this.Editor.Redo();
			element.Value.Should().Be(val2.Value);
		}

		[Test]
		public IEnumerator Test_Inspector_Damageable()
		{
			yield return this.Utils.SpawnMapObject<BoxDestructibleData>();

			var element = (DamageableElement) this.Inspector.GetElement<DamageableProperty>();

			var val1 = new DamageableProperty(true);
			var val2 = new DamageableProperty(false);

			element.Value.Should().Be(val1.Value);
			element.Value = val2.Value;
			element.Value.Should().Be(val2.Value);
			this.Editor.Undo();
			element.Value.Should().Be(val1.Value);
			this.Editor.Redo();
			element.Value.Should().Be(val2.Value);
		}

		[Test]
		public IEnumerator Test_Inspector_RopePosition()
		{
			yield return this.Utils.SpawnMapObject<RopeData>();

			var element = (RopePositionElement) this.Inspector.GetElement<RopePositionProperty>();

			var val1 = new RopePositionProperty(new(0, 1), new(0, -1));
			var val2 = new RopePositionProperty(new(0, 2), new(0, -2));

			element.Value.Should().Be(val1);
			element.Value = val2;
			element.Value.Should().Be(val2);
			this.Editor.Undo();
			element.Value.Should().Be(val1);
			this.Editor.Redo();
			element.Value.Should().Be(val2);
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MapsExt.Editor.ActionHandlers;
using MapsExt.Editor.MapObjects;
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

			this.Editor.OnUndo();
			this.Editor.Content.transform.childCount.Should().Be(0);
			this.Editor.ActiveObject.Should().BeNull();
			this.Editor.OnRedo();
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

			var pos1 = go.GetHandlerValue<PositionProperty>();
			var pos2 = new PositionProperty(1, 0);

			yield return this.Utils.MoveSelectedWithMouse(pos2);
			go.GetHandlerValue<PositionProperty>().Should().Be(pos2);

			this.Editor.OnUndo();
			go.GetHandlerValue<PositionProperty>().Should().Be(pos1);
			this.Editor.OnRedo();
			go.GetHandlerValue<PositionProperty>().Should().Be(pos2);
		}

		[Test]
		public IEnumerator Test_ResizeWithMouse()
		{
			yield return this.Utils.SpawnMapObject<BoxData>();
			var go = this.Editor.ActiveObject;

			var size1 = go.GetHandlerValue<ScaleProperty>();
			var size2 = new ScaleProperty(4, 2);

			yield return this.Utils.ResizeSelectedWithMouse(size2 - size1, AnchorPosition.MiddleRight);
			go.GetHandlerValue<ScaleProperty>().Should().Be(size2);

			this.Editor.OnUndo();
			go.GetHandlerValue<ScaleProperty>().Should().Be(size1);
			this.Editor.OnRedo();
			go.GetHandlerValue<ScaleProperty>().Should().Be(size2);
		}

		[Test]
		public IEnumerator Test_RotateWithMouse()
		{
			yield return this.Utils.SpawnMapObject<BoxData>();
			var go = this.Editor.ActiveObject;

			var rot1 = go.GetHandlerValue<RotationProperty>();
			var rot2 = new RotationProperty(45);

			yield return this.Utils.RotateSelectedWithMouse(rot2.Value.eulerAngles.z - rot1.Value.eulerAngles.z);
			go.GetHandlerValue<RotationProperty>().Should().Be(rot2);

			this.Editor.OnUndo();
			go.GetHandlerValue<RotationProperty>().Should().Be(rot1);
			this.Editor.OnRedo();
			go.GetHandlerValue<RotationProperty>().Should().Be(rot2);
		}

		[Test]
		public IEnumerator Test_MoveWithNudge()
		{
			yield return this.Utils.SpawnMapObject<BoxData>();
			var go = this.Editor.ActiveObject;

			var pos1 = go.GetHandlerValue<PositionProperty>();
			var pos2 = new PositionProperty(0.25f, 0);

			this.Editor.OnKeyDown(KeyCode.RightArrow);
			go.GetHandlerValue<PositionProperty>().Should().Be(pos2);

			this.Editor.OnUndo();
			go.GetHandlerValue<PositionProperty>().Should().Be(pos1);
			this.Editor.OnRedo();
			go.GetHandlerValue<PositionProperty>().Should().Be(pos2);
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

			rope.GetAnchor(0).SetHandlerValue(new PositionProperty(0.25f, 0.5f));
			this.Editor.TakeSnaphot();
			rope.GetAnchor(1).SetHandlerValue(new PositionProperty(0, 5));
			this.Editor.TakeSnaphot();

			list.Add(rope.GetAnchor(0).GetAnchoredPosition());

			this.Editor.ClearSelected();
			this.Editor.AddSelected(boxGo);

			yield return this.Utils.MoveSelectedWithMouse(new Vector3(1, 0, 0));
			list.Add(rope.GetAnchor(0).GetAnchoredPosition());

			yield return this.Utils.ResizeSelectedWithMouse(new Vector3(4, 2, 0), AnchorPosition.MiddleRight);
			list.Add(rope.GetAnchor(0).GetAnchoredPosition());

			yield return this.Utils.RotateSelectedWithMouse(45);
			list.Add(rope.GetAnchor(0).GetAnchoredPosition());

			this.Editor.OnDeleteSelectedMapObjects();
			yield return null;

			this.Editor.ActiveObject.Should().BeNull();
			this.Editor.Content.transform.childCount.Should().Be(1);

			var iter = ListIterator.From(list);

			this.Editor.OnUndo(); // Undo delete
			this.Editor.Content.transform.childCount.Should().Be(2);

			rope.GetAnchor(0).GetAnchoredPosition().Should().BeApproximately(iter.MoveLast().Current);
			this.Editor.OnUndo(); // Undo rotate
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MovePrevious().Current);
			this.Editor.OnUndo(); // Undo resize
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MovePrevious().Current);
			this.Editor.OnUndo(); // Undo move
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MovePrevious().Current);
			this.Editor.OnUndo(); // Undo set anchor 2 position
			this.Editor.OnUndo(); // Undo set anchor 1 position
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MovePrevious().Current);
			this.Editor.OnUndo(); // Undo spawn rope
			this.Editor.OnUndo(); // Undo spawn box
			this.Editor.Content.transform.childCount.Should().Be(0);

			this.Editor.OnRedo(); // Redo spawn box
			this.Editor.OnRedo(); // Redo spawn rope
			this.Editor.Content.transform.childCount.Should().Be(2);
			rope = this.Editor.Content.transform.GetChild(1).GetComponentInParent<EditorRope.RopeInstance>();
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.Current);

			this.Editor.OnRedo(); // Redo set anchor 1 position
			this.Editor.OnRedo(); // Redo set anchor 2 position
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MoveNext().Current);

			this.Editor.OnRedo(); // Redo move
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MoveNext().Current);
			this.Editor.OnRedo(); // Redo resize
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MoveNext().Current);
			this.Editor.OnRedo(); // Redo rotate
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MoveNext().Current);
			this.Editor.OnRedo(); // Redo Delete
			this.Editor.ActiveObject.Should().BeNull();
			this.Editor.Content.transform.childCount.Should().Be(1);
		}
	}
}

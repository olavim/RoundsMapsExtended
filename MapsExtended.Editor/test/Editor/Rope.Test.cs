using System.Collections;
using System.Linq;
using FluentAssertions;
using MapsExt.Editor.MapObjects;
using MapsExt.MapObjects;
using UnityEngine;
using Surity;
using MapsExt.Properties;
using MapsExt.Editor.Properties;

namespace MapsExt.Editor.Tests
{
	[TestClass]
	internal class RopeTests : EditorTestBase
	{
		[Test]
		public IEnumerator Test_RopeSpawnsInTheMiddle()
		{
			yield return this.Utils.SpawnMapObject<RopeData>();
			var rope = this.Editor.SelectedObjects.First().GetComponentInParent<EditorRope.RopeInstance>();

			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(new Vector2(0, 1));
			rope.GetAnchor(1).GetAnchoredPosition().Should().Be(new Vector2(0, -1));
		}

		[Test]
		public IEnumerator Test_AnchorMovesWithAttachedObject_MoveObject()
		{
			yield return this.Utils.SpawnMapObject<RopeData>();
			var rope = this.Editor.SelectedObjects.First().GetComponentInParent<EditorRope.RopeInstance>();
			yield return this.Utils.SpawnMapObject<BoxData>();
			var boxGo = this.Editor.ActiveObject;

			rope.SetEditorMapObjectProperty(new RopePositionProperty(new(0, 0), new(0, 5)));

			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(new Vector2(0, 0));
			rope.GetAnchor(1).GetAnchoredPosition().Should().Be(new Vector2(0, 5));

			rope.GetAnchor(0).IsAttached.Should().BeTrue();
			rope.GetAnchor(1).IsAttached.Should().BeFalse();

			boxGo.SetEditorMapObjectProperty<PositionProperty>(new Vector2(0, 1));
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(new Vector2(0, 1));
			rope.GetAnchor(1).GetAnchoredPosition().Should().Be(new Vector2(0, 5));
		}

		[Test]
		public IEnumerator Test_AnchorMovesWithAttachedObject_MoveGroup()
		{
			yield return this.Utils.SpawnMapObject<BoxData>();
			var box1 = this.Editor.ActiveObject;
			yield return this.Utils.SpawnMapObject<BoxData>();
			var box2 = this.Editor.ActiveObject;

			box2.SetEditorMapObjectProperty(new PositionProperty(4, 2));

			yield return this.Utils.SpawnMapObject<RopeData>();
			var rope = this.Editor.SelectedObjects.First().GetComponentInParent<EditorRope.RopeInstance>();
			rope.SetEditorMapObjectProperty(new RopePositionProperty(new(0, 0), new(0, 5)));

			rope.GetAnchor(0).IsAttached.Should().BeTrue();
			rope.GetAnchor(1).IsAttached.Should().BeFalse();

			this.Editor.ClearSelected();
			this.Editor.AddSelected(new GameObject[] { box1, box2, rope.GetAnchor(0).gameObject });

			this.Utils.MoveSelectedWithMouse(new(1, 0));

			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(new Vector2(1, 0));
			rope.GetAnchor(1).GetAnchoredPosition().Should().Be(new Vector2(0, 5));
		}

		[Test]
		public IEnumerator Test_AnchorMovesWithAttachedObject_RotateObject()
		{
			yield return this.Utils.SpawnMapObject<RopeData>();
			var rope = this.Editor.SelectedObjects.First().GetComponentInParent<EditorRope.RopeInstance>();
			yield return this.Utils.SpawnMapObject<BoxData>();
			var boxGo = this.Editor.ActiveObject;

			rope.SetEditorMapObjectProperty(new RopePositionProperty(new(0, 0.25f), new(0, 5)));

			var localPos = (Vector2) boxGo.transform.InverseTransformPoint(rope.GetAnchor(0).GetAnchoredPosition());

			boxGo.SetEditorMapObjectProperty<RotationProperty>(90);

			rope.GetAnchor(0).GetAnchoredPosition().Should().Be((Vector2) boxGo.transform.TransformPoint(localPos));
			rope.GetAnchor(1).GetAnchoredPosition().Should().Be(new Vector2(0, 5));
		}

		[Test]
		public IEnumerator Test_AnchorMovesWithAttachedObject_ResizeObject()
		{
			yield return this.Utils.SpawnMapObject<RopeData>();
			var rope = this.Editor.SelectedObjects.First().GetComponentInParent<EditorRope.RopeInstance>();
			yield return this.Utils.SpawnMapObject<BoxData>();
			var boxGo = this.Editor.ActiveObject;

			rope.SetEditorMapObjectProperty(new RopePositionProperty(new(-0.25f, 0), new(0, 5)));

			var localPos = (Vector2) boxGo.transform.InverseTransformPoint(rope.GetAnchor(0).GetAnchoredPosition());

			boxGo.SetEditorMapObjectProperty(new ScaleProperty(4, 2));
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be((Vector2) boxGo.transform.TransformPoint(localPos));

			boxGo.SetEditorMapObjectProperty(new ScaleProperty(2, 2));
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be((Vector2) boxGo.transform.TransformPoint(localPos));
		}
	}
}

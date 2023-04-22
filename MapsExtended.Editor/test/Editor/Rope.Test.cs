using System.Collections;
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
			var rope = this.Editor.ActiveMapObject.GetComponent<EditorRope.RopeInstance>();

			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(new Vector2(0, 1));
			rope.GetAnchor(1).GetAnchoredPosition().Should().Be(new Vector2(0, -1));
		}

		[Test]
		public IEnumerator Test_AnchorMovesWithAttachedObject_MoveObject()
		{
			yield return this.Utils.SpawnMapObject<RopeData>();
			var rope = this.Editor.ActiveMapObject.GetComponent<EditorRope.RopeInstance>();
			yield return this.Utils.SpawnMapObject<BoxData>();
			var boxGo = this.Editor.ActiveObject;

			rope.WriteProperty(new RopePositionProperty(new(0, 0), new(0, 5)));

			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(new Vector2(0, 0));
			rope.GetAnchor(1).GetAnchoredPosition().Should().Be(new Vector2(0, 5));

			rope.GetAnchor(0).IsAttached.Should().BeTrue();
			rope.GetAnchor(1).IsAttached.Should().BeFalse();

			boxGo.WriteProperty<PositionProperty>(new Vector2(0, 1));
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

			box2.WriteProperty(new PositionProperty(4, 2));

			yield return this.Utils.SpawnMapObject<RopeData>();
			var rope = this.Editor.ActiveMapObject.GetComponent<EditorRope.RopeInstance>();
			rope.WriteProperty(new RopePositionProperty(new(0, 0), new(0, 5)));

			rope.GetAnchor(0).IsAttached.Should().BeTrue();
			rope.GetAnchor(1).IsAttached.Should().BeFalse();

			this.Editor.ClearSelected();
			this.Editor.AddSelected(new GameObject[] { box1, box2, rope.GetAnchor(0).gameObject });

			yield return this.Utils.MoveSelectedWithMouse(new(1, 0));

			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(new Vector2(1, 0));
			rope.GetAnchor(1).GetAnchoredPosition().Should().Be(new Vector2(0, 5));
		}

		[Test]
		public IEnumerator Test_AnchorMovesWithAttachedObject_RotateObject()
		{
			yield return this.Utils.SpawnMapObject<RopeData>();
			var rope = this.Editor.ActiveMapObject.GetComponent<EditorRope.RopeInstance>();
			yield return this.Utils.SpawnMapObject<BoxData>();
			var boxGo = this.Editor.ActiveObject;

			rope.WriteProperty(new RopePositionProperty(new(0, 0.25f), new(0, 5)));

			var localPos = (Vector2) boxGo.transform.InverseTransformPoint(rope.GetAnchor(0).GetAnchoredPosition());

			boxGo.WriteProperty<RotationProperty>(90);

			rope.GetAnchor(0).GetAnchoredPosition().Should().Be((Vector2) boxGo.transform.TransformPoint(localPos));
			rope.GetAnchor(1).GetAnchoredPosition().Should().Be(new Vector2(0, 5));
		}

		[Test]
		public IEnumerator Test_AnchorMovesWithAttachedObject_ResizeObject()
		{
			yield return this.Utils.SpawnMapObject<RopeData>();
			var rope = this.Editor.ActiveMapObject.GetComponent<EditorRope.RopeInstance>();
			yield return this.Utils.SpawnMapObject<BoxData>();
			var boxGo = this.Editor.ActiveObject;

			rope.WriteProperty(new RopePositionProperty(new(-0.25f, 0), new(0, 5)));

			var localPos = (Vector2) boxGo.transform.InverseTransformPoint(rope.GetAnchor(0).GetAnchoredPosition());

			boxGo.WriteProperty(new ScaleProperty(4, 2));
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be((Vector2) boxGo.transform.TransformPoint(localPos));

			boxGo.WriteProperty(new ScaleProperty(2, 2));
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be((Vector2) boxGo.transform.TransformPoint(localPos));
		}

		[Test]
		public IEnumerator Test_AnchorGetsAttachedWhenObjectAppearsOnTop_Mouse_Move()
		{
			yield return this.Utils.SpawnMapObject<RopeData>();
			var rope = this.Editor.ActiveMapObject.GetComponent<EditorRope.RopeInstance>();
			yield return this.Utils.SpawnMapObject<BoxData>();

			rope.WriteProperty(new RopePositionProperty(new(5, 5), new(-5, -5)));
			rope.GetAnchor(0).IsAttached.Should().BeFalse();
			rope.GetAnchor(1).IsAttached.Should().BeFalse();

			yield return this.Utils.MoveSelectedWithMouse(new(5, 5));

			rope.GetAnchor(0).IsAttached.Should().BeTrue();
			rope.GetAnchor(1).IsAttached.Should().BeFalse();
		}

		[Test]
		public IEnumerator Test_AnchorGetsAttachedWhenObjectAppearsOnTop_Mouse_Rotate()
		{
			yield return this.Utils.SpawnMapObject<RopeData>();
			var rope = this.Editor.ActiveMapObject.GetComponent<EditorRope.RopeInstance>();
			yield return this.Utils.SpawnMapObject<BoxData>();

			rope.WriteProperty(new RopePositionProperty(new(1.25f, 0), new(-5, -5)));
			rope.GetAnchor(0).IsAttached.Should().BeFalse();
			rope.GetAnchor(1).IsAttached.Should().BeFalse();

			yield return this.Utils.RotateSelectedWithMouse(45);

			rope.GetAnchor(0).IsAttached.Should().BeTrue();
			rope.GetAnchor(1).IsAttached.Should().BeFalse();
		}

		[Test]
		public IEnumerator Test_AnchorGetsAttachedWhenObjectAppearsOnTop_Mouse_Resize()
		{
			yield return this.Utils.SpawnMapObject<RopeData>();
			var rope = this.Editor.ActiveMapObject.GetComponent<EditorRope.RopeInstance>();
			yield return this.Utils.SpawnMapObject<BoxData>();

			rope.WriteProperty(new RopePositionProperty(new(3, 0), new(-5, -5)));
			rope.GetAnchor(0).IsAttached.Should().BeFalse();
			rope.GetAnchor(1).IsAttached.Should().BeFalse();

			yield return this.Utils.ResizeSelectedWithMouse(new(2, 0), AnchorPosition.MiddleRight);

			rope.GetAnchor(0).IsAttached.Should().BeTrue();
			rope.GetAnchor(1).IsAttached.Should().BeFalse();
		}

		[Test]
		public IEnumerator Test_AnchorGetsAttachedWhenObjectAppearsOnTop_Inspector_Move()
		{
			yield return this.Utils.SpawnMapObject<RopeData>();
			var rope = this.Editor.ActiveMapObject.GetComponent<EditorRope.RopeInstance>();
			yield return this.Utils.SpawnMapObject<BoxData>();

			rope.WriteProperty(new RopePositionProperty(new(5, 5), new(-5, -5)));
			rope.GetAnchor(0).IsAttached.Should().BeFalse();
			rope.GetAnchor(1).IsAttached.Should().BeFalse();

			var element = (PositionElement) this.Inspector.GetElement<PositionProperty>();
			element.Value = new(5, 5);

			rope.GetAnchor(0).IsAttached.Should().BeTrue();
			rope.GetAnchor(1).IsAttached.Should().BeFalse();
		}

		[Test]
		public IEnumerator Test_AnchorGetsAttachedWhenObjectAppearsOnTop_Inspector_Rotate()
		{
			yield return this.Utils.SpawnMapObject<RopeData>();
			var rope = this.Editor.ActiveMapObject.GetComponent<EditorRope.RopeInstance>();
			yield return this.Utils.SpawnMapObject<BoxData>();

			rope.WriteProperty(new RopePositionProperty(new(1.25f, 0), new(-5, -5)));
			rope.GetAnchor(0).IsAttached.Should().BeFalse();
			rope.GetAnchor(1).IsAttached.Should().BeFalse();

			var element = (RotationElement) this.Inspector.GetElement<RotationProperty>();
			element.Value = 45;

			rope.GetAnchor(0).IsAttached.Should().BeTrue();
			rope.GetAnchor(1).IsAttached.Should().BeFalse();
		}

		[Test]
		public IEnumerator Test_AnchorGetsAttachedWhenObjectAppearsOnTop_Inspector_Resize()
		{
			yield return this.Utils.SpawnMapObject<RopeData>();
			var rope = this.Editor.ActiveMapObject.GetComponent<EditorRope.RopeInstance>();
			yield return this.Utils.SpawnMapObject<BoxData>();

			rope.WriteProperty(new RopePositionProperty(new(3, 0), new(-5, -5)));
			rope.GetAnchor(0).IsAttached.Should().BeFalse();
			rope.GetAnchor(1).IsAttached.Should().BeFalse();

			var element = (ScaleElement) this.Inspector.GetElement<ScaleProperty>();
			element.Value = new(8, 2);

			rope.GetAnchor(0).IsAttached.Should().BeTrue();
			rope.GetAnchor(1).IsAttached.Should().BeFalse();
		}
	}
}

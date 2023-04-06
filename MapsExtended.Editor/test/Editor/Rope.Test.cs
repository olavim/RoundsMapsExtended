using System.Collections;
using System.Linq;
using FluentAssertions;
using MapsExt.Editor.ActionHandlers;
using MapsExt.Editor.MapObjects;
using MapsExt.MapObjects;
using UnityEngine;
using Surity;
using MapsExt.Properties;

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

			rope.GetAnchor(0).gameObject.SetHandlerValue<PositionProperty>(new Vector2(0, 0));
			rope.GetAnchor(1).gameObject.SetHandlerValue<PositionProperty>(new Vector2(0, 5));

			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(new Vector2(0, 0));
			rope.GetAnchor(1).GetAnchoredPosition().Should().Be(new Vector2(0, 5));

			rope.GetAnchor(0).IsAttached.Should().BeTrue();
			rope.GetAnchor(1).IsAttached.Should().BeFalse();

			boxGo.SetHandlerValue<PositionProperty>(new Vector2(0, 1));
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

			box2.GetComponent<PositionHandler>().Move(new Vector2(2, 0));

			yield return this.Utils.SpawnMapObject<RopeData>();
			var rope = this.Editor.SelectedObjects.First().GetComponentInParent<EditorRope.RopeInstance>();

			rope.GetAnchor(0).gameObject.SetHandlerValue<PositionProperty>(new Vector2(0, 0));
			rope.GetAnchor(1).gameObject.SetHandlerValue<PositionProperty>(new Vector2(0, 5));

			rope.GetAnchor(0).IsAttached.Should().BeTrue();
			rope.GetAnchor(1).IsAttached.Should().BeFalse();

			this.Editor.ClearSelected();
			this.Editor.AddSelected(new GameObject[] { box1, box2, rope.GetAnchor(0).gameObject });

			this.Editor.ActiveObject.GetComponent<PositionHandler>().Move(new Vector2(1, 0));

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

			rope.GetAnchor(0).gameObject.SetHandlerValue<PositionProperty>(new Vector2(0, 0.25f));
			rope.GetAnchor(1).gameObject.SetHandlerValue<PositionProperty>(new Vector2(0, 5));

			var localPos = (Vector2) boxGo.transform.InverseTransformPoint(rope.GetAnchor(0).GetAnchoredPosition());

			boxGo.SetHandlerValue<RotationProperty>(Quaternion.Euler(0, 0, 90));

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

			rope.GetAnchor(0).gameObject.GetComponent<PositionHandler>().SetValue(new Vector2(-0.25f, 0));
			rope.GetAnchor(1).gameObject.GetComponent<PositionHandler>().SetValue(new Vector2(0, 5));

			var localPos = (Vector2) boxGo.transform.InverseTransformPoint(rope.GetAnchor(0).GetAnchoredPosition());

			boxGo.GetComponent<SizeHandler>().SetValue(new Vector2(4, 2), AnchorPosition.MiddleRight);
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be((Vector2) boxGo.transform.TransformPoint(localPos));

			boxGo.GetComponent<SizeHandler>().SetValue(new Vector2(2, 2), AnchorPosition.MiddleRight);
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be((Vector2) boxGo.transform.TransformPoint(localPos));

			boxGo.GetComponent<SizeHandler>().SetValue(new Vector2(4, 2), AnchorPosition.MiddleLeft);
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be((Vector2) boxGo.transform.TransformPoint(localPos));

			boxGo.GetComponent<SizeHandler>().SetValue(new Vector2(2, 2), AnchorPosition.MiddleLeft);
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be((Vector2) boxGo.transform.TransformPoint(localPos));
		}
	}
}

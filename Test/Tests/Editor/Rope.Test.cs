using System.Collections;
using System.Linq;
using FluentAssertions;
using MapsExt.Editor;
using MapsExt.Editor.ActionHandlers;
using MapsExt.Editor.MapObjects;
using MapsExt.MapObjects;
using UnityEngine;

namespace MapsExt.Test.Tests.Editor
{
	[TestClass]
	public class RopeTests
	{
		private MapEditor editor;
		private EditorTestUtils utils;

		[AfterAll]
		[BeforeEach]
		public IEnumerator OpenEditor()
		{
			yield return MapsExtendedEditor.instance.OpenEditorCoroutine();
			var rootGo = GameObject.Find("/Map");
			this.editor = rootGo.GetComponentInChildren<MapEditor>();
			this.utils = new EditorTestUtils(this.editor);
		}

		[AfterEach]
		public IEnumerator CloseEditor()
		{
			yield return MapsExtendedEditor.instance.CloseEditorCoroutine();
			this.editor = null;
		}

		[Test]
		public IEnumerator Test_RopeSpawnsInTheMiddle()
		{
			yield return this.utils.SpawnMapObject<RopeData>();
			var go = this.editor.selectedObjects.First();
			var rope = go.GetComponentInParent<EditorRopeInstance>();

			((Vector2) rope.GetAnchor(0).GetAnchoredPosition()).Should().Be(new Vector2(0, 1));
			((Vector2) rope.GetAnchor(1).GetAnchoredPosition()).Should().Be(new Vector2(0, -1));
		}

		[Test]
		public IEnumerator Test_AnchorMovesWithAttachedObject_MoveObject()
		{
			yield return this.utils.SpawnMapObject<RopeData>();
			var rope = this.editor.selectedObjects.First().GetComponentInParent<EditorRopeInstance>();
			yield return this.utils.SpawnMapObject<BoxData>();
			var boxGo = this.editor.selectedObjects.First();

			rope.GetAnchor(0).gameObject.GetComponent<PositionHandler>().SetPosition(new Vector3(0, 0, 0));
			rope.GetAnchor(1).gameObject.GetComponent<PositionHandler>().SetPosition(new Vector3(0, 5, 0));

			((Vector2) rope.GetAnchor(0).GetAnchoredPosition()).Should().Be(new Vector2(0, 0));
			((Vector2) rope.GetAnchor(1).GetAnchoredPosition()).Should().Be(new Vector2(0, 5));

			rope.GetAnchor(0).IsAttached.Should().BeTrue();
			rope.GetAnchor(1).IsAttached.Should().BeFalse();

			boxGo.GetComponent<PositionHandler>().SetPosition(new Vector3(0, 1, 0));
			((Vector2) rope.GetAnchor(0).GetAnchoredPosition()).Should().Be(new Vector2(0, 1));
			((Vector2) rope.GetAnchor(1).GetAnchoredPosition()).Should().Be(new Vector2(0, 5));
		}

		[Test]
		public IEnumerator Test_AnchorMovesWithAttachedObject_RotateObject()
		{
			yield return this.utils.SpawnMapObject<RopeData>();
			var rope = this.editor.selectedObjects.First().GetComponentInParent<EditorRopeInstance>();
			yield return this.utils.SpawnMapObject<BoxData>();
			var boxGo = this.editor.selectedObjects.First();

			rope.GetAnchor(0).gameObject.GetComponent<PositionHandler>().SetPosition(new Vector3(0, 0.25f, 0));
			rope.GetAnchor(1).gameObject.GetComponent<PositionHandler>().SetPosition(new Vector3(0, 5, 0));

			var localPos = (Vector2) boxGo.transform.InverseTransformPoint(rope.GetAnchor(0).GetAnchoredPosition());

			boxGo.GetComponent<MapsExt.Editor.ActionHandlers.RotationHandler>().SetRotation(Quaternion.Euler(0, 0, 90));

			((Vector2) rope.GetAnchor(0).GetAnchoredPosition()).Should().Be((Vector2) boxGo.transform.TransformPoint(localPos));
			((Vector2) rope.GetAnchor(1).GetAnchoredPosition()).Should().Be(new Vector2(0, 5));
		}

		[Test]
		public IEnumerator Test_AnchorMovesWithAttachedObject_ResizeObject()
		{
			yield return this.utils.SpawnMapObject<RopeData>();
			var rope = this.editor.selectedObjects.First().GetComponentInParent<EditorRopeInstance>();
			yield return this.utils.SpawnMapObject<BoxData>();
			var boxGo = this.editor.selectedObjects.First();

			rope.GetAnchor(0).gameObject.GetComponent<PositionHandler>().SetPosition(new Vector3(-0.25f, 0, 0));
			rope.GetAnchor(1).gameObject.GetComponent<PositionHandler>().SetPosition(new Vector3(0, 5, 0));

			var localPos = (Vector2) boxGo.transform.InverseTransformPoint(rope.GetAnchor(0).GetAnchoredPosition());

			boxGo.GetComponent<SizeHandler>().SetSize(new Vector3(4, 2, 0), AnchorPosition.MiddleRight);
			((Vector2) rope.GetAnchor(0).GetAnchoredPosition()).Should().Be((Vector2) boxGo.transform.TransformPoint(localPos));

			boxGo.GetComponent<SizeHandler>().SetSize(new Vector3(2, 2, 0), AnchorPosition.MiddleRight);
			((Vector2) rope.GetAnchor(0).GetAnchoredPosition()).Should().Be((Vector2) boxGo.transform.TransformPoint(localPos));

			boxGo.GetComponent<SizeHandler>().SetSize(new Vector3(4, 2, 0), AnchorPosition.MiddleLeft);
			((Vector2) rope.GetAnchor(0).GetAnchoredPosition()).Should().Be((Vector2) boxGo.transform.TransformPoint(localPos));

			boxGo.GetComponent<SizeHandler>().SetSize(new Vector3(2, 2, 0), AnchorPosition.MiddleLeft);
			((Vector2) rope.GetAnchor(0).GetAnchoredPosition()).Should().Be((Vector2) boxGo.transform.TransformPoint(localPos));
		}
	}
}

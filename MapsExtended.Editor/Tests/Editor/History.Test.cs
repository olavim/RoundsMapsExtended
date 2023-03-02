using System.Collections;
using System.Linq;
using FluentAssertions;
using MapsExt.Editor.ActionHandlers;
using MapsExt.Editor.MapObjects;
using MapsExt.MapObjects;
using MapsExt.Testing;
using UnityEngine;

namespace MapsExt.Editor.Tests
{
	[TestClass]
	public class HistoryTests
	{
		private MapEditor editor;
		private SimulatedInputSource inputSource;
		private EditorTestUtils utils;

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
			EditorInput.SetInputSource(EditorInput.defaultInputSource);
		}

		[AfterAll]
		[BeforeEach]
		public IEnumerator OpenEditor()
		{
			yield return MapsExtendedEditor.instance.OpenEditorCoroutine();
			var rootGo = GameObject.Find("/Map");
			this.editor = rootGo.GetComponentInChildren<MapEditor>();
			this.utils = new EditorTestUtils(this.editor, this.inputSource);
		}

		[AfterEach]
		public IEnumerator CloseEditor()
		{
			yield return MapsExtendedEditor.instance.CloseEditorCoroutine();
			this.editor = null;
		}

		[Test]
		public IEnumerator Test_Spawn()
		{
			yield return this.utils.SpawnMapObject<BoxData>();
			var go = this.editor.selectedObjects.First();
			var id = go.GetComponent<MapObjectInstance>().mapObjectId;

			this.editor.OnUndo();
			this.editor.content.transform.childCount.Should().Be(0);
			this.editor.selectedObjects.Count.Should().Be(0);
			this.editor.OnRedo();
			this.editor.content.transform.childCount.Should().Be(1);
			this.editor.selectedObjects.Count.Should().Be(0);

			var instance = this.editor.content.transform.GetChild(0).GetComponent<MapObjectInstance>();
			instance.mapObjectId.Should().Be(id);
		}

		[Test]
		public IEnumerator Test_MoveWithMouse()
		{
			yield return this.utils.SpawnMapObject<BoxData>();
			var go = this.editor.selectedObjects.First();

			yield return this.utils.MoveSelectedWithMouse(new Vector3(1, 0));
			((Vector2) go.transform.position).Should().Be(new Vector2(1, 0));
			this.editor.OnUndo();
			((Vector2) go.transform.position).Should().Be(new Vector2(0, 0));
			this.editor.OnRedo();
			((Vector2) go.transform.position).Should().Be(new Vector2(1, 0));
		}

		[Test]
		public IEnumerator Test_ResizeWithMouse()
		{
			yield return this.utils.SpawnMapObject<BoxData>();
			var go = this.editor.selectedObjects.First();

			yield return this.utils.ResizeSelectedWithMouse(new Vector3(2, 0), AnchorPosition.MiddleRight);
			((Vector2) go.transform.localScale).Should().Be(new Vector2(4, 2));
			this.editor.OnUndo();
			((Vector2) go.transform.localScale).Should().Be(new Vector2(2, 2));
			this.editor.OnRedo();
			((Vector2) go.transform.localScale).Should().Be(new Vector2(4, 2));
		}

		[Test]
		public IEnumerator Test_RotateWithMouse()
		{
			yield return this.utils.SpawnMapObject<BoxData>();
			var go = this.editor.selectedObjects.First();

			yield return this.utils.RotateSelectedWithMouse(45);
			go.transform.rotation.Should().Be(Quaternion.Euler(0, 0, 45));
			this.editor.OnUndo();
			go.transform.rotation.Should().Be(Quaternion.Euler(0, 0, 0));
			this.editor.OnRedo();
			go.transform.rotation.Should().Be(Quaternion.Euler(0, 0, 45));
		}

		[Test]
		public IEnumerator Test_MoveWithNudge()
		{
			yield return this.utils.SpawnMapObject<BoxData>();
			var go = this.editor.selectedObjects.First();

			this.editor.OnKeyDown(KeyCode.RightArrow);
			((Vector2) go.transform.position).Should().Be(new Vector2(0.25f, 0));
			this.editor.OnUndo();
			((Vector2) go.transform.position).Should().Be(new Vector2(0, 0));
			this.editor.OnRedo();
			((Vector2) go.transform.position).Should().Be(new Vector2(0.25f, 0));
		}

		[Test]
		public IEnumerator Test_RopeAttachment()
		{
			yield return this.utils.SpawnMapObject<BoxData>();
			var boxGo = this.editor.selectedObjects.First();
			yield return this.utils.SpawnMapObject<RopeData>();
			var rope = this.editor.content.transform.GetChild(1).GetComponent<EditorRopeInstance>();

			rope.GetAnchor(0).GetComponent<PositionHandler>().SetPosition(new Vector3(0.25f, 0.5f, 0));
			rope.GetAnchor(1).GetComponent<PositionHandler>().SetPosition(new Vector3(0, 5, 0));

			var localPos = (Vector2) boxGo.transform.InverseTransformPoint(rope.GetAnchor(0).GetAnchoredPosition());

			this.editor.ClearSelected();
			this.editor.AddSelected(boxGo);

			yield return this.utils.MoveSelectedWithMouse(new Vector3(1, 0, 0));
			yield return this.utils.ResizeSelectedWithMouse(new Vector3(4, 2, 0), AnchorPosition.MiddleRight);
			yield return this.utils.RotateSelectedWithMouse(45);

			this.editor.OnUndo();
			((Vector2) rope.GetAnchor(0).GetAnchoredPosition()).Should().Be((Vector2) boxGo.transform.TransformPoint(localPos));
			this.editor.OnUndo();
			((Vector2) rope.GetAnchor(0).GetAnchoredPosition()).Should().Be((Vector2) boxGo.transform.TransformPoint(localPos));
			this.editor.OnUndo();
			((Vector2) rope.GetAnchor(0).GetAnchoredPosition()).Should().Be((Vector2) boxGo.transform.TransformPoint(localPos));

			this.editor.OnRedo();
			((Vector2) rope.GetAnchor(0).GetAnchoredPosition()).Should().Be((Vector2) boxGo.transform.TransformPoint(localPos));
			this.editor.OnRedo();
			((Vector2) rope.GetAnchor(0).GetAnchoredPosition()).Should().Be((Vector2) boxGo.transform.TransformPoint(localPos));
			this.editor.OnRedo();
			((Vector2) rope.GetAnchor(0).GetAnchoredPosition()).Should().Be((Vector2) boxGo.transform.TransformPoint(localPos));
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MapsExt.Editor.ActionHandlers;
using MapsExt.Editor.MapObjects;
using MapsExt.MapObjects;
using MapsExt.MapObjects.Properties;
using Surity;
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
			EditorInput.SetInputSource(EditorInput.DefaultInputSource);
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
			var go = this.editor.ActiveObject;
			var id = go.GetComponent<MapObjectInstance>().MapObjectId;

			this.editor.OnUndo();
			this.editor.Content.transform.childCount.Should().Be(0);
			this.editor.ActiveObject.Should().BeNull();
			this.editor.OnRedo();
			this.editor.Content.transform.childCount.Should().Be(1);
			this.editor.ActiveObject.Should().BeNull();

			var instance = this.editor.Content.transform.GetChild(0).GetComponent<MapObjectInstance>();
			instance.MapObjectId.Should().Be(id);
		}

		[Test]
		public IEnumerator Test_MoveWithMouse()
		{
			yield return this.utils.SpawnMapObject<BoxData>();
			var go = this.editor.ActiveObject;

			var pos1 = go.GetHandlerValue<PositionProperty>();
			var pos2 = new PositionProperty(1, 0);

			yield return this.utils.MoveSelectedWithMouse(pos2);
			go.GetHandlerValue<PositionProperty>().Should().Be(pos2);

			this.editor.OnUndo();
			go.GetHandlerValue<PositionProperty>().Should().Be(pos1);
			this.editor.OnRedo();
			go.GetHandlerValue<PositionProperty>().Should().Be(pos2);
		}

		[Test]
		public IEnumerator Test_ResizeWithMouse()
		{
			yield return this.utils.SpawnMapObject<BoxData>();
			var go = this.editor.ActiveObject;

			var size1 = go.GetHandlerValue<ScaleProperty>();
			var size2 = new ScaleProperty(4, 2);

			yield return this.utils.ResizeSelectedWithMouse(size2 - size1, AnchorPosition.MiddleRight);
			go.GetHandlerValue<ScaleProperty>().Should().Be(size2);

			this.editor.OnUndo();
			go.GetHandlerValue<ScaleProperty>().Should().Be(size1);
			this.editor.OnRedo();
			go.GetHandlerValue<ScaleProperty>().Should().Be(size2);
		}

		[Test]
		public IEnumerator Test_RotateWithMouse()
		{
			yield return this.utils.SpawnMapObject<BoxData>();
			var go = this.editor.ActiveObject;

			var rot1 = go.GetHandlerValue<RotationProperty>();
			var rot2 = new RotationProperty(45);

			yield return this.utils.RotateSelectedWithMouse(rot2.Value.eulerAngles.z - rot1.Value.eulerAngles.z);
			go.GetHandlerValue<RotationProperty>().Should().Be(rot2);

			this.editor.OnUndo();
			go.GetHandlerValue<RotationProperty>().Should().Be(rot1);
			this.editor.OnRedo();
			go.GetHandlerValue<RotationProperty>().Should().Be(rot2);
		}

		[Test]
		public IEnumerator Test_MoveWithNudge()
		{
			yield return this.utils.SpawnMapObject<BoxData>();
			var go = this.editor.ActiveObject;

			var pos1 = go.GetHandlerValue<PositionProperty>();
			var pos2 = new PositionProperty(0.25f, 0);

			this.editor.OnKeyDown(KeyCode.RightArrow);
			go.GetHandlerValue<PositionProperty>().Should().Be(pos2);

			this.editor.OnUndo();
			go.GetHandlerValue<PositionProperty>().Should().Be(pos1);
			this.editor.OnRedo();
			go.GetHandlerValue<PositionProperty>().Should().Be(pos2);
		}

		[Test]
		public IEnumerator Test_RopeAttachment()
		{
			yield return this.utils.SpawnMapObject<BoxData>();
			var boxGo = this.editor.ActiveObject;
			yield return this.utils.SpawnMapObject<RopeData>();
			var rope = this.editor.SelectedObjects.First().GetComponentInParent<EditorRopeInstance>();

			var list = new List<Vector2>
			{
				rope.GetAnchor(0).GetAnchoredPosition()
			};

			rope.GetAnchor(0).SetHandlerValue(new PositionProperty(0.25f, 0.5f));
			this.editor.TakeSnaphot();
			rope.GetAnchor(1).SetHandlerValue(new PositionProperty(0, 5));
			this.editor.TakeSnaphot();

			list.Add(rope.GetAnchor(0).GetAnchoredPosition());

			this.editor.ClearSelected();
			this.editor.AddSelected(boxGo);

			yield return this.utils.MoveSelectedWithMouse(new Vector3(1, 0, 0));
			list.Add(rope.GetAnchor(0).GetAnchoredPosition());

			yield return this.utils.ResizeSelectedWithMouse(new Vector3(4, 2, 0), AnchorPosition.MiddleRight);
			list.Add(rope.GetAnchor(0).GetAnchoredPosition());

			yield return this.utils.RotateSelectedWithMouse(45);
			list.Add(rope.GetAnchor(0).GetAnchoredPosition());

			this.editor.OnDeleteSelectedMapObjects();
			yield return null;

			this.editor.ActiveObject.Should().BeNull();
			this.editor.Content.transform.childCount.Should().Be(1);

			var iter = ListIterator.From(list);

			this.editor.OnUndo(); // Undo delete
			this.editor.Content.transform.childCount.Should().Be(2);

			rope.GetAnchor(0).GetAnchoredPosition().Should().BeApproximately(iter.MoveLast().Current);
			this.editor.OnUndo(); // Undo rotate
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MovePrevious().Current);
			this.editor.OnUndo(); // Undo resize
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MovePrevious().Current);
			this.editor.OnUndo(); // Undo move
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MovePrevious().Current);
			this.editor.OnUndo(); // Undo set anchor 2 position
			this.editor.OnUndo(); // Undo set anchor 1 position
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MovePrevious().Current);
			this.editor.OnUndo(); // Undo spawn rope
			this.editor.OnUndo(); // Undo spawn box
			this.editor.Content.transform.childCount.Should().Be(0);

			this.editor.OnRedo(); // Redo spawn box
			this.editor.OnRedo(); // Redo spawn rope
			this.editor.Content.transform.childCount.Should().Be(2);
			rope = this.editor.Content.transform.GetChild(1).GetComponentInParent<EditorRopeInstance>();
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.Current);

			this.editor.OnRedo(); // Redo set anchor 1 position
			this.editor.OnRedo(); // Redo set anchor 2 position
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MoveNext().Current);

			this.editor.OnRedo(); // Redo move
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MoveNext().Current);
			this.editor.OnRedo(); // Redo resize
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MoveNext().Current);
			this.editor.OnRedo(); // Redo rotate
			rope.GetAnchor(0).GetAnchoredPosition().Should().Be(iter.MoveNext().Current);
			this.editor.OnRedo(); // Redo Delete
			this.editor.ActiveObject.Should().BeNull();
			this.editor.Content.transform.childCount.Should().Be(1);
		}
	}
}

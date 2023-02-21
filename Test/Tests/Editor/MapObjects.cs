using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using FluentAssertions;
using MapsExt.Editor;
using MapsExt.MapObjects;
using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Test.Tests.Editor
{
	public class MapObjectTests
	{
		private MapEditor editor;
		private MapEditorUI editorUI;
		private SimulatedInputSource inputSource;

		[BeforeAll]
		public void SetInputSource()
		{
			var rootGo = MapsExtendedTest.instance.gameObject;
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
			this.editorUI = rootGo.GetComponentInChildren<MapEditorUI>();
		}

		[AfterEach]
		public IEnumerator CloseEditor()
		{
			yield return MapsExtendedEditor.instance.CloseEditorCoroutine();
			this.editor = null;
			this.editorUI = null;
		}

		[Test]
		public IEnumerator Test_BoxSpawnsInTheMiddle()
		{
			yield return this.SpawnBox();

			this.editor.content.transform.childCount.Should().Be(1);

			var box = this.editor.selectedObjects.First();
			box.GetComponent<MapObjectInstance>().dataType.Should().Be(typeof(BoxData));
			((Vector2) box.transform.position).Should().Be(Vector2.zero);
		}

		[Test]
		public IEnumerator Test_Mouse_MoveBox()
		{
			yield return this.SpawnBox();

			var box = this.editor.selectedObjects.First();
			var delta = new Vector2(-5, 0);
			yield return MoveSelectedWithMouse(delta);
			((Vector2) box.transform.position).Should().Be(delta);
		}

		[Test]
		public IEnumerator Test_Mouse_SelectUnderlyingBox()
		{
			yield return this.SpawnBox();
			var box1 = this.editor.selectedObjects.First();
			yield return this.SpawnBox();
			var box2 = this.editor.selectedObjects.First();

			this.inputSource.SetMousePosition(MainCam.instance.cam.WorldToScreenPoint(box1.transform.position));
			this.inputSource.SetMouseButtonDown(0);
			this.inputSource.SetMouseButtonUp(0);
			yield return null;

			this.editor.selectedObjects.First().Should().BeSameAs(box1);
		}

		[Test]
		public IEnumerator Test_Mouse_SelectGroup()
		{
			yield return this.SpawnBox();
			yield return this.SpawnBox();

			this.inputSource.SetMousePosition(MainCam.instance.cam.WorldToScreenPoint(new Vector3(-5, -5, 0)));
			this.inputSource.SetMouseButtonDown(0);
			yield return null;
			this.inputSource.SetMousePosition(MainCam.instance.cam.WorldToScreenPoint(new Vector3(5, 5, 0)));
			this.inputSource.SetMouseButtonUp(0);
			yield return null;

			this.editor.selectedObjects.Count.Should().Be(2);
		}

		[Test]
		public IEnumerator Test_Mouse_MoveGroup()
		{
			yield return this.SpawnBox();
			var box1 = this.editor.selectedObjects.First();
			yield return this.SpawnBox();
			var box2 = this.editor.selectedObjects.First();

			this.editor.SelectAll();

			var delta = new Vector2(-5, 0);
			yield return MoveSelectedWithMouse(delta);
			((Vector2) box1.transform.position).Should().Be(delta);
			((Vector2) box2.transform.position).Should().Be(delta);
		}

		private IEnumerator SpawnBox()
		{
			bool collectionChanged = false;

			void OnEditorSelectionChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				if (this.editor.selectedObjects.Count == 1)
				{
					this.editor.selectedObjects.CollectionChanged -= OnEditorSelectionChanged;
					collectionChanged = true;
				}
			}

			this.editor.selectedObjects.CollectionChanged += OnEditorSelectionChanged;

			var btn = this.GetMapObjectWindowButton("Box");
			btn.Should().NotBeNull();
			btn.onClick.Invoke();

			while (!collectionChanged)
			{
				yield return null;
			}
		}

		private Button GetMapObjectWindowButton(string label)
		{
			var boxText = this.editorUI.mapObjectWindow.content.GetComponentsInChildren<Text>().Where(t => t.text == label).FirstOrDefault();
			return boxText.gameObject.GetComponentInParent<Button>();
		}

		private IEnumerator MoveSelectedWithMouse(Vector3 delta)
		{
			var go = this.editor.selectedObjects.First();
			this.inputSource.SetMousePosition(MainCam.instance.cam.WorldToScreenPoint(go.transform.position));
			yield return this.DragMouse(go.transform.position + delta);
		}

		private IEnumerator DragMouse(Vector3 pos)
		{
			this.inputSource.SetMouseButtonDown(0);
			yield return null;
			this.inputSource.SetMousePosition(MainCam.instance.cam.WorldToScreenPoint(pos));
			yield return null;
			this.inputSource.SetMouseButtonUp(0);
			yield return null;
		}
	}
}

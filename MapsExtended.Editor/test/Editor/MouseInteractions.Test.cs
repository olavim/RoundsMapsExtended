using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using FluentAssertions;
using MapsExt.Editor.ActionHandlers;
using MapsExt.MapObjects;
using UnityEngine;
using UnityEngine.UI;
using Surity;
using System;
using MapsExt.MapObjects.Properties;

namespace MapsExt.Editor.Tests
{
	[TestClass]
	public class MouseInteractionTests
	{
		private MapEditor editor;
		private MapEditorUI editorUI;
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
			this.editorUI = rootGo.GetComponentInChildren<MapEditorUI>();
			this.utils = new EditorTestUtils(this.editor, this.inputSource);
		}

		[AfterEach]
		public IEnumerator CloseEditor()
		{
			yield return MapsExtendedEditor.instance.CloseEditorCoroutine();
			this.editor = null;
			this.editorUI = null;
		}

		[Test]
		public IEnumerator Test_SpawnBoxFromMapObjectWindow()
		{
			yield return this.SpawnFromMapObjectWindow("Box");

			this.editor.content.transform.childCount.Should().Be(1);

			var box = this.editor.ActiveObject;
			box.GetComponent<MapObjectInstance>().dataType.Should().Be(typeof(BoxData));
			box.GetHandlerValue<PositionProperty>().Should().Be((PositionProperty) Vector2.zero);
		}

		[Test]
		public IEnumerator Test_MoveBox()
		{
			yield return this.SpawnFromMapObjectWindow("Box");

			var box = this.editor.ActiveObject;
			var delta = new PositionProperty(-5, 0);
			yield return this.utils.MoveSelectedWithMouse(delta);
			box.GetHandlerValue<PositionProperty>().Should().Be(delta);
		}

		[Test]
		public IEnumerator Test_MoveBoxSnapToGrid()
		{
			yield return this.SpawnFromMapObjectWindow("Box");

			var box = this.editor.ActiveObject;
			var delta = new PositionProperty(-5.2f, 0);
			yield return this.utils.MoveSelectedWithMouse(delta);
			box.GetHandlerValue<PositionProperty>().Should().Be(new PositionProperty(-5.25f, 0));
		}

		[Test]
		public IEnumerator Test_SelectUnderlyingBox()
		{
			yield return this.SpawnFromMapObjectWindow("Box");
			var box1 = this.editor.ActiveObject;
			yield return this.SpawnFromMapObjectWindow("Box");
			var box2 = this.editor.ActiveObject;

			this.inputSource.SetMousePosition(MainCam.instance.cam.WorldToScreenPoint(box1.transform.position));
			this.inputSource.SetMouseButtonDown(0);
			this.inputSource.SetMouseButtonUp(0);
			yield return null;

			this.editor.ActiveObject.Should().BeSameAs(box1);
			this.editor.ActiveObject.Should().NotBeSameAs(box2);
		}

		[Test]
		public IEnumerator Test_SelectGroup()
		{
			yield return this.SpawnFromMapObjectWindow("Box");
			yield return this.SpawnFromMapObjectWindow("Box");

			this.inputSource.SetMousePosition(MainCam.instance.cam.WorldToScreenPoint(new Vector3(-5, -5, 0)));
			this.inputSource.SetMouseButtonDown(0);
			yield return null;
			this.inputSource.SetMousePosition(MainCam.instance.cam.WorldToScreenPoint(new Vector3(5, 5, 0)));
			this.inputSource.SetMouseButtonUp(0);
			yield return null;

			this.editor.SelectedObjects.Count.Should().Be(2);
			this.editor.ActiveObject.GetComponent<IGroupMapObjectActionHandler>().Should().NotBeNull();
		}

		[Test]
		public IEnumerator Test_MoveGroup()
		{
			yield return this.SpawnFromMapObjectWindow("Box");
			var box1 = this.editor.ActiveObject;
			yield return this.SpawnFromMapObjectWindow("Box");
			var box2 = this.editor.ActiveObject;

			this.editor.SelectAll();

			var delta = new PositionProperty(-5, 0);
			yield return this.utils.MoveSelectedWithMouse(delta);
			box1.GetHandlerValue<PositionProperty>().Should().Be(delta);
			box2.GetHandlerValue<PositionProperty>().Should().Be(delta);
		}

		[Test]
		public IEnumerator Test_ResizeBox()
		{
			yield return this.SpawnFromMapObjectWindow("Box");
			var box = this.editor.ActiveObject;

			box.GetHandlerValue<ScaleProperty>().Should().Be(new ScaleProperty(2, 2));
			yield return this.utils.ResizeSelectedWithMouse(Vector3.one, AnchorPosition.TopRight);
			box.GetHandlerValue<ScaleProperty>().Should().Be(new ScaleProperty(3, 3));
		}

		[Test]
		public IEnumerator Test_RotateBox()
		{
			yield return this.SpawnFromMapObjectWindow("Box");
			var box = this.editor.ActiveObject;

			box.GetHandlerValue<RotationProperty>().Should().Be(new RotationProperty(0));
			yield return this.utils.RotateSelectedWithMouse(45);
			box.GetHandlerValue<RotationProperty>().Should().Be(new RotationProperty(45));
		}

		private IEnumerator SpawnFromMapObjectWindow(string objectName)
		{
			bool collectionChanged = false;

			void OnEditorSelectionChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				if (this.editor.ActiveObject != null)
				{
					this.editor.SelectedObjects.CollectionChanged -= OnEditorSelectionChanged;
					collectionChanged = true;
				}
			}

			this.editor.SelectedObjects.CollectionChanged += OnEditorSelectionChanged;

			var btn = this.GetMapObjectWindowButton(objectName);
			btn.Should().NotBeNull();
			btn.onClick.Invoke();

			while (!collectionChanged)
			{
				yield return null;
			}
		}

		private Button GetMapObjectWindowButton(string label)
		{
			var boxText = Array.Find(this.editorUI.mapObjectWindow.content.GetComponentsInChildren<Text>(), t => t.text == label);
			return boxText.gameObject.GetComponentInParent<Button>();
		}
	}
}

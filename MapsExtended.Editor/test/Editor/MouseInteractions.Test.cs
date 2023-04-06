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
using MapsExt.Properties;

namespace MapsExt.Editor.Tests
{
	[TestClass]
	internal class MouseInteractionTests : EditorTestBase
	{
		[Test]
		public IEnumerator Test_SpawnBoxFromMapObjectWindow()
		{
			yield return this.SpawnFromMapObjectWindow("Box");

			this.Editor.Content.transform.childCount.Should().Be(1);

			var box = this.Editor.ActiveObject;
			box.GetComponent<MapObjectInstance>().DataType.Should().Be(typeof(BoxData));
			box.GetHandlerValue<PositionProperty>().Should().Be((PositionProperty) Vector2.zero);
		}

		[Test]
		public IEnumerator Test_MoveBox()
		{
			yield return this.SpawnFromMapObjectWindow("Box");

			var box = this.Editor.ActiveObject;
			var delta = new PositionProperty(-5, 0);
			yield return this.Utils.MoveSelectedWithMouse(delta);
			box.GetHandlerValue<PositionProperty>().Should().Be(delta);
		}

		[Test]
		public IEnumerator Test_MoveBoxSnapToGrid()
		{
			yield return this.SpawnFromMapObjectWindow("Box");

			var box = this.Editor.ActiveObject;
			var delta = new PositionProperty(-5.2f, 0);
			yield return this.Utils.MoveSelectedWithMouse(delta);
			box.GetHandlerValue<PositionProperty>().Should().Be(new PositionProperty(-5.25f, 0));
		}

		[Test]
		public IEnumerator Test_SelectUnderlyingBox()
		{
			yield return this.SpawnFromMapObjectWindow("Box");
			var box1 = this.Editor.ActiveObject;
			yield return this.SpawnFromMapObjectWindow("Box");
			var box2 = this.Editor.ActiveObject;

			this.InputSource.SetMousePosition(MainCam.instance.cam.WorldToScreenPoint(box1.transform.position));
			this.InputSource.SetMouseButtonDown(0);
			this.InputSource.SetMouseButtonUp(0);
			yield return null;

			this.Editor.ActiveObject.Should().BeSameAs(box1);
			this.Editor.ActiveObject.Should().NotBeSameAs(box2);
		}

		[Test]
		public IEnumerator Test_SelectGroup()
		{
			yield return this.SpawnFromMapObjectWindow("Box");
			yield return this.SpawnFromMapObjectWindow("Box");

			this.InputSource.SetMousePosition(MainCam.instance.cam.WorldToScreenPoint(new Vector3(-5, -5, 0)));
			this.InputSource.SetMouseButtonDown(0);
			yield return null;
			this.InputSource.SetMousePosition(MainCam.instance.cam.WorldToScreenPoint(new Vector3(5, 5, 0)));
			this.InputSource.SetMouseButtonUp(0);
			yield return null;

			this.Editor.SelectedObjects.Count.Should().Be(2);
			this.Editor.ActiveObject.GetComponent<IGroupMapObjectActionHandler>().Should().NotBeNull();
		}

		[Test]
		public IEnumerator Test_MoveGroup()
		{
			yield return this.SpawnFromMapObjectWindow("Box");
			var box1 = this.Editor.ActiveObject;
			yield return this.SpawnFromMapObjectWindow("Box");
			var box2 = this.Editor.ActiveObject;

			this.Editor.SelectAll();

			var delta = new PositionProperty(-5, 0);
			yield return this.Utils.MoveSelectedWithMouse(delta);
			box1.GetHandlerValue<PositionProperty>().Should().Be(delta);
			box2.GetHandlerValue<PositionProperty>().Should().Be(delta);
		}

		[Test]
		public IEnumerator Test_ResizeBox()
		{
			yield return this.SpawnFromMapObjectWindow("Box");
			var box = this.Editor.ActiveObject;

			box.GetHandlerValue<ScaleProperty>().Should().Be(new ScaleProperty(2, 2));
			yield return this.Utils.ResizeSelectedWithMouse(Vector3.one, AnchorPosition.TopRight);
			box.GetHandlerValue<ScaleProperty>().Should().Be(new ScaleProperty(3, 3));
		}

		[Test]
		public IEnumerator Test_RotateBox()
		{
			yield return this.SpawnFromMapObjectWindow("Box");
			var box = this.Editor.ActiveObject;

			box.GetHandlerValue<RotationProperty>().Should().Be(new RotationProperty(0));
			yield return this.Utils.RotateSelectedWithMouse(45);
			box.GetHandlerValue<RotationProperty>().Should().Be(new RotationProperty(45));
		}

		private IEnumerator SpawnFromMapObjectWindow(string objectName)
		{
			bool collectionChanged = false;

			void OnEditorSelectionChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				if (this.Editor.ActiveObject != null)
				{
					this.Editor.SelectedObjects.CollectionChanged -= OnEditorSelectionChanged;
					collectionChanged = true;
				}
			}

			this.Editor.SelectedObjects.CollectionChanged += OnEditorSelectionChanged;

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
			var boxText = Array.Find(this.EditorUI.MapObjectWindow.Content.GetComponentsInChildren<Text>(), t => t.text == label);
			return boxText.gameObject.GetComponentInParent<Button>();
		}
	}
}

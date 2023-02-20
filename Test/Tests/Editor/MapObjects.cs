using System;
using System.Collections;
using System.Linq;
using FluentAssertions;
using MapsExt.Editor;
using MapsExt.MapObjects;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MapsExt.Test.Tests.Editor
{
	public class MapObjectTests
	{
		private MapEditor editor;
		private MapEditorUI editorUI;

		[BeforeEach]
		public IEnumerator ResetScene()
		{
			bool sceneLoaded = false;

			void LoadScene(Scene scene, LoadSceneMode mode)
			{
				SceneManager.sceneLoaded -= LoadScene;
				sceneLoaded = true;
			};

			SceneManager.sceneLoaded += LoadScene;
			yield return MapsExtendedEditor.instance.OpenEditorCoroutine();

			while (!sceneLoaded)
			{
				yield return null;
			}

			var rootGo = GameObject.Find("/Map");
			this.editor = rootGo.GetComponentInChildren<MapEditor>();
			this.editorUI = rootGo.GetComponentInChildren<MapEditorUI>();
		}

		[Test]
		public IEnumerator Test_BoxSpawnsInTheMiddle()
		{
			this.SpawnBox();
			yield return null;

			this.editor.content.transform.childCount.Should().Be(1);

			var box = this.editor.content.transform.GetChild(0).gameObject;
			box.GetComponent<MapObjectInstance>().dataType.Should().Be(typeof(BoxData));
			box.transform.position.x.Should().Be(0);
			box.transform.position.y.Should().Be(0);
		}

		[Test]
		public IEnumerator Test_MoveBoxWithMouse()
		{
			this.SpawnBox();
			yield return null;

			var box = this.editor.content.transform.GetChild(0).gameObject;

			MouseUtils.SetCursorPosition(WorldToMonitorPoint(box.transform.position));
			yield return DragMouse(box.transform.position + new Vector3(-5, 0, 0));

			box.transform.position.x.Should().Be(-5);
			box.transform.position.y.Should().Be(0);
		}

		private void SpawnBox()
		{
			var btn = this.GetMapObjectWindowButton("Box");
			btn.Should().NotBeNull();
			btn.onClick.Invoke();
		}

		private Button GetMapObjectWindowButton(string label)
		{
			var boxText = this.editorUI.mapObjectWindow.content.GetComponentsInChildren<Text>().Where(t => t.text == label).FirstOrDefault();
			return boxText.gameObject.GetComponentInParent<Button>();
		}

		private IEnumerator DragMouse(Vector3 pos)
		{
			MouseUtils.MouseDown();
			yield return null;
			MouseUtils.SetCursorPosition(WorldToMonitorPoint(pos));
			yield return null;
			MouseUtils.MouseUp();
		}

		private Vector2Int WorldToMonitorPoint(Vector3 point)
		{
			var screenPoint = MainCam.instance.cam.WorldToScreenPoint(point);
			return MonitorUtils.ScreenToMonitorPoint(new Vector2Int((int) screenPoint.x, (int) screenPoint.y));
		}
	}
}

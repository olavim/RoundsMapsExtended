using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using MapsExt.Editor;
using MapsExt.Editor.ActionHandlers;
using MapsExt.MapObjects;
using UnityEngine;

namespace MapsExt.Test.Tests.Editor
{
	public class EditorTestUtils
	{
		private MapEditor editor;
		private SimulatedInputSource inputSource;

		public EditorTestUtils(MapEditor editor) : this(editor, null) { }

		public EditorTestUtils(MapEditor editor, SimulatedInputSource inputSource)
		{
			this.editor = editor;
			this.inputSource = inputSource;
		}

		public IEnumerator SpawnMapObject<T>() where T : MapObjectData
		{
			bool spawned = false;

			void OnChange(object sender, NotifyCollectionChangedEventArgs e)
			{
				if (this.editor.selectedObjects.Count > 0)
				{
					this.editor.selectedObjects.CollectionChanged -= OnChange;
					spawned = true;
				}
			}

			this.editor.selectedObjects.CollectionChanged += OnChange;
			this.editor.CreateMapObject(typeof(T));

			while (!spawned)
			{
				yield return null;
			}
		}

		public IEnumerator MoveSelectedWithMouse(Vector3 delta)
		{
			var go = this.editor.selectedObjects.First();
			yield return this.DragMouse(go.transform.position, delta);
		}

		public IEnumerator ResizeSelectedWithMouse(Vector3 delta, int anchorPosition)
		{
			var go = this.editor.selectedObjects.First();
			var resizeInteractionContent = go.GetComponent<SizeHandler>().content;
			var resizeHandle = resizeInteractionContent.transform.Find("Resize Handle " + anchorPosition).gameObject;
			yield return this.DragMouse(resizeHandle.transform.position, delta);
		}

		public IEnumerator RotateSelectedWithMouse(float degrees)
		{
			var go = this.editor.selectedObjects.First();
			var resizeInteractionContent = go.GetComponent<MapsExt.Editor.ActionHandlers.RotationHandler>().content;
			var handle = resizeInteractionContent.transform.Find("Rotation Handle").gameObject;

			var from = handle.transform.position;
			var rotated = Quaternion.Euler(0, 0, degrees) * from;

			yield return this.DragMouse(from, rotated - from);
		}

		public IEnumerator DragMouse(Vector3 worldPosition, Vector3 delta)
		{
			this.inputSource.SetMousePosition(MainCam.instance.cam.WorldToScreenPoint(worldPosition));
			this.inputSource.SetMouseButtonDown(0);
			yield return null;
			this.inputSource.SetMousePosition(MainCam.instance.cam.WorldToScreenPoint(worldPosition + delta));
			yield return null;
			this.inputSource.SetMouseButtonUp(0);
			yield return null;
		}
	}
}

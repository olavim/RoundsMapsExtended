using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using MapsExt.Editor.Events;
using MapsExt.Editor.MapObjects;
using MapsExt.MapObjects;
using MapsExt.Properties;
using UnityEngine;

namespace MapsExt.Editor.Tests
{
	public class EditorTestUtils
	{
		private readonly MapEditor editor;
		private readonly SimulatedInputSource inputSource;

		public EditorTestUtils(MapEditor editor) : this(editor, null) { }

		public EditorTestUtils(MapEditor editor, SimulatedInputSource inputSource)
		{
			this.editor = editor;
			this.inputSource = inputSource;
		}

		public IEnumerator SpawnMapObject<T>() where T : MapObjectData
		{
			return this.SpawnMapObject(typeof(T));
		}

		public IEnumerator SpawnMapObject(Type type)
		{
			if (!typeof(MapObjectData).IsAssignableFrom(type))
			{
				throw new ArgumentException("Type must be assignable to MapObjectData");
			}

			IEnumerator Run()
			{
				this.editor.CreateMapObject(type);

				while (this.editor.ActiveMapObjectPart == null)
				{
					yield return null;
				}

				yield return null;
			}

			return Run();
		}

		public IEnumerable<Type> GetMapObjects()
		{
			return typeof(MapsExtendedEditor).Assembly.GetTypes().Where(t => t.GetCustomAttribute<EditorMapObjectAttribute>() != null);
		}

		public IEnumerable<Type> GetMapObjectsWithProperty<T>() where T : IProperty
		{
			return this.GetMapObjects().Where(t => this.HasProperty<T>(t.GetCustomAttribute<EditorMapObjectAttribute>().DataType));
		}

		public bool HasProperty<T>(Type type) where T : IProperty
		{
			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
			return
				type.GetProperties(flags).Any(p => typeof(T).IsAssignableFrom(p.PropertyType)) ||
				type.GetFields(flags).Any(p => typeof(T).IsAssignableFrom(p.FieldType));
		}

		public IEnumerator MoveSelectedWithMouse(Vector2 delta)
		{
			var go = this.editor.SelectedMapObjectParts.First();
			yield return this.DragMouse(go.transform.position, delta);
		}

		public IEnumerator ResizeSelectedWithMouse(Vector2 delta, Direction2D anchorPosition)
		{
			if (anchorPosition == Direction2D.Middle)
			{
				throw new ArgumentException("anchorPosition cannot be Direction2D.Middle");
			}

			IEnumerator DoResizeSelectedWithMouse()
			{
				var resizeInteractionContent = this.editor.ActiveMapObjectPart.GetComponent<SizeHandler>().Content;
				var resizeHandle = resizeInteractionContent.transform.Find("Resize Handle " + anchorPosition).gameObject;
				yield return this.DragMouse(resizeHandle.transform.position, delta);
			}

			return DoResizeSelectedWithMouse();
		}

		public IEnumerator RotateSelectedWithMouse(float degrees)
		{
			var go = this.editor.ActiveMapObjectPart;
			var resizeInteractionContent = go.GetComponent<Events.RotationHandler>().Content;
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

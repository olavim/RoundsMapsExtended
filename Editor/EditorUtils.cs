using UnityEngine;
using System.Collections.Generic;

namespace MapsExt.Editor
{
	public static class EditorUtils
	{
		public static List<GameObject> GetHoveredMapObjects()
		{
			var mousePos = Input.mousePosition;
			var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));
			var colliders = Physics2D.OverlapPointAll(mouseWorldPos);
			return EditorUtils.GetDraggableGameObjects(colliders);
		}

		public static List<GameObject> GetContainedMapObjects(Rect rect)
		{
			var colliders = Physics2D.OverlapAreaAll(rect.min, rect.max);
			return EditorUtils.GetDraggableGameObjects(colliders);
		}

		private static List<GameObject> GetDraggableGameObjects(IEnumerable<Collider2D> colliders)
		{
			var list = new List<GameObject>();

			foreach (var collider in colliders)
			{
				var go = collider.gameObject;

				while (go && !EditorUtils.IsDraggableGameObject(go))
				{
					go = go.transform.parent?.gameObject;
				}

				if (go && !list.Contains(go))
				{
					list.Add(go);
				}
			}

			return list;
		}

		public static bool IsDraggableGameObject(GameObject go)
		{
			return go.GetComponent<EditorActionHandler>() != null;
		}

		public static Vector3 SnapToGrid(Vector3 pos, float gridSize)
		{
			float gridX = Snap(pos.x, gridSize);
			float gridY = Snap(pos.y, gridSize);
			return new Vector3(gridX, gridY, pos.z);
		}

		public static float Snap(float num, float step)
		{
			return Mathf.Round(num / step) * Mathf.Abs(step);
		}

		public static Rect GetMapObjectBounds(GameObject go)
		{
			var colliders = new List<Collider2D>();

			if (go.GetComponent<Collider2D>())
			{
				colliders.Add(go.GetComponent<Collider2D>());
			}

			colliders.AddRange(go.GetComponentsInChildren<Collider2D>());

			float minX = 9999f;
			float maxX = -9999f;
			float minY = 9999f;
			float maxY = -9999f;

			foreach (var collider in colliders)
			{
				minX = Mathf.Min(minX, collider.bounds.min.x);
				maxX = Mathf.Max(maxX, collider.bounds.max.x);
				minY = Mathf.Min(minY, collider.bounds.min.y);
				maxY = Mathf.Max(maxY, collider.bounds.max.y);
			}

			return new Rect(minX, minY, maxX - minX, maxY - minY);
		}
	}
}

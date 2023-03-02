using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using MapsExt.Editor.ActionHandlers;

namespace MapsExt.Editor
{
	public static class EditorUtils
	{
		public static List<MapObjectActionHandler> GetActionHandlersAt(Vector3 position)
		{
			var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(position.x, position.y));
			var colliders = Physics2D.OverlapPointAll(mouseWorldPos);
			return colliders.SelectMany(c => c.GetComponentsInChildren<MapObjectActionHandler>()).ToList();
		}

		public static List<MapObjectActionHandler> GetContainedActionHandlers(Rect rect)
		{
			var colliders = Physics2D.OverlapAreaAll(rect.min, rect.max);
			return colliders.SelectMany(c => c.GetComponentsInChildren<MapObjectActionHandler>()).ToList();
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

		public static Bounds GetMapObjectBounds(GameObject go)
		{
			var colliders = go.GetComponentsInChildren<Collider2D>();
			var bounds = new Bounds(go.transform.position, Vector3.zero);

			foreach (var collider in colliders)
			{
				bounds.Encapsulate(collider.bounds);
			}

			return bounds;
		}
	}
}

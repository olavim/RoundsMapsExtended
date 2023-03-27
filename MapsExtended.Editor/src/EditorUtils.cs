﻿using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using MapsExt.Editor.ActionHandlers;

namespace MapsExt.Editor
{
	public static class EditorUtils
	{
		public static List<ActionHandler> GetActionHandlersAt(Vector2 position)
		{
			var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(position.x, position.y));
			var colliders = Physics2D.OverlapPointAll(mouseWorldPos);
			return colliders.SelectMany(c => c.GetComponentsInChildren<ActionHandler>()).ToList();
		}

		public static List<ActionHandler> GetContainedActionHandlers(Rect rect)
		{
			var colliders = Physics2D.OverlapAreaAll(rect.min, rect.max);
			return colliders.SelectMany(c => c.GetComponentsInChildren<ActionHandler>()).ToList();
		}

		public static Vector2 SnapToGrid(Vector2 pos, float gridSize)
		{
			float gridX = Snap(pos.x, gridSize);
			float gridY = Snap(pos.y, gridSize);
			return new Vector2(gridX, gridY);
		}

		public static float Snap(float num, float step)
		{
			return Mathf.Round(num / step) * Mathf.Abs(step);
		}
	}
}
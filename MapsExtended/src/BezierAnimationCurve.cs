using UnityEngine;

namespace MapsExt
{
	public class BezierAnimationCurve
	{
		private Vector2 _c1;
		private Vector2 _c2;

		public BezierAnimationCurve(float x1, float y1, float x2, float y2)
		{
			this._c1 = new(x1, y1);
			this._c2 = new(x2, y2);
		}

		public float Evaluate(float time)
		{
			/* Bezier curves can be kind of "grouped up" at certain points. What this means is that the
			 * visual half-way point of the curve may not be at t = 0.5, and the visual one-third of the
			 * way -point may not be at t = 0.33, etc. Instead we need to search for the t-value that
			 * corresponds to the time-value.
			 */
			float t = this.BinarySearchT(time, 0, 1);
			return this.CalcBezier(t).y;
		}

		private float BinarySearchT(float x, float min, float max)
		{
			float diff = 1;
			float currentT = 0;

			for (int i = 0; i < 10 && Mathf.Abs(diff) > 0.0001f; i++)
			{
				currentT = min + ((max - min) / 2.0f);
				diff = this.CalcBezier(currentT).x - x;

				if (diff > 0)
				{
					max = currentT;
				}
				else
				{
					min = currentT;
				}
			}

			return currentT;
		}

		private Vector2 CalcBezier(float t)
		{
			float t2 = t * t;
			float t3 = t2 * t;
			return
					(this._c1 * ((3f * t3) - (6f * t2) + (3f * t))) +
					(this._c2 * ((-3f * t3) + (3f * t2))) +
					(Vector2.one * t3);
		}
	}
}

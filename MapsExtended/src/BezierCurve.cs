using System.Linq;
using UnityEngine;

namespace MapsExt
{
	public class BezierCurve
	{
		private class Segment
		{
			public float Distance { get; set; }
			public Vector2 Point { get; set; }

			public Segment(float distance, Vector2 point)
			{
				this.Distance = distance;
				this.Point = point;
			}
		}

		private const int SegmentCount = 20;

		private readonly Segment[] _segments;
		private readonly Vector2 _p0;
		private readonly Vector2 _p1;
		private readonly Vector2 _p2;
		private readonly Vector2 _p3;
		private readonly float _totalLength;

		public Vector2[] Points => this._segments.Select(s => s.Point).ToArray();

		public BezierCurve(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
		{
			this._p0 = p0;
			this._p1 = p1;
			this._p2 = p2;
			this._p3 = p3;

			var segments = new Segment[SegmentCount + 1];
			segments[0] = new Segment(0, p0);

			for (int i = 1; i <= SegmentCount; i++)
			{
				float t = i / (float) SegmentCount;
				var point = this.EvaluatePosition(t);
				this._totalLength += Vector2.Distance(point, segments[i - 1].Point);
				segments[i] = new Segment(this._totalLength, point);
			}

			this._segments = new Segment[SegmentCount + 1];
			this._segments[0] = new Segment(0, p0);

			float step = this._totalLength / SegmentCount;

			for (int i = 1; i <= SegmentCount; i++)
			{
				float distance = i * step;
				this._segments[i] = new Segment(distance, this.FindPositionForDistance(distance, segments));
			}
		}

		public float Evaluate(float t)
		{
			return this.EvaluatePosition(t).y;
		}

		public Vector2 EvaluatePosition(float t)
		{
			float t2 = t * t;
			float t3 = t2 * t;
			float u = 1 - t;
			float u2 = u * u;
			float u3 = u2 * u;
			return (u3 * this._p0) + (3 * u2 * t * this._p1) + (3 * u * t2 * this._p2) + (t3 * this._p3);
		}

		public float EvaluateForDistance(float distance)
		{
			return this.EvaluatePositionForDistance(distance).y;
		}

		public Vector2 EvaluatePositionForDistance(float distance)
		{
			if (distance <= 0)
			{
				return this._p0;
			}

			if (distance >= 1)
			{
				return this._p3;
			}

			float targetLength = distance * this._totalLength;
			int segmentIndex = (int) (distance * (this._segments.Length - 1));

			var segment = this._segments[segmentIndex];
			var nextSegment = this._segments[segmentIndex + 1];

			float t = (targetLength - segment.Distance) / (nextSegment.Distance - segment.Distance);
			return Vector2.Lerp(segment.Point, nextSegment.Point, t);
		}

		private Vector2 FindPositionForDistance(float distance, Segment[] segments)
		{
			int low = 0;
			int high = segments.Length - 1;
			int index = 0;

			while (low < high)
			{
				index = (low + high) / 2;
				if (segments[index].Distance < distance)
				{
					low = index + 1;
				}
				else
				{
					high = index;
				}
			}

			if (segments[index].Distance > distance)
			{
				index--;
			}

			var segment = segments[index];
			var nextSegment = segments[index + 1];

			float t = (distance - segment.Distance) / (nextSegment.Distance - segment.Distance);
			return Vector2.Lerp(segment.Point, nextSegment.Point, t);
		}
	}
}

using System;
using System.Linq;
using UnityEngine;

namespace MapsExt
{
	public class BezierCurve
	{
		private class DistanceToPoint : IComparable<DistanceToPoint>, IComparable
		{
			public float Distance { get; set; }
			public Vector2 Point { get; set; }

			public DistanceToPoint(float distance, Vector2 point)
			{
				this.Distance = distance;
				this.Point = point;
			}

			public int CompareTo(DistanceToPoint obj)
			{
				return this.Distance.CompareTo(obj.Distance);
			}

			public int CompareTo(object obj)
			{
				return this.CompareTo((DistanceToPoint) obj);
			}
		}

		private static Vector2 FindPositionForDistance(float distance, DistanceToPoint[] segments)
		{
			int index = Array.BinarySearch(segments, new DistanceToPoint(distance, Vector2.zero));
			if (index < 0)
			{
				index = ~index - 1;
			}

			if (index >= segments.Length - 1)
			{
				return segments.Last().Point;
			}

			var segment = segments[index];
			var nextSegment = segments[index + 1];

			float t = (distance - segment.Distance) / (nextSegment.Distance - segment.Distance);
			return Vector2.Lerp(segment.Point, nextSegment.Point, t);
		}

		private const int DefaultSegmentCount = 40;

		private readonly DistanceToPoint[] _distancesToPoints;

		public Vector2 P0 { get; }
		public Vector2 P1 { get; }
		public Vector2 P2 { get; }
		public Vector2 P3 { get; }

		public Vector2[] Points => this._distancesToPoints.Select(s => s.Point).ToArray();
		public float Length => this._distancesToPoints.Last().Distance;

		public BezierCurve(Vector2 p1, Vector2 p2, int segmentCount = DefaultSegmentCount) : this(Vector2.zero, p1, p2, Vector2.one, segmentCount) { }

		public BezierCurve(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, int segmentCount = DefaultSegmentCount)
		{
			if (segmentCount < 2)
			{
				throw new ArgumentOutOfRangeException(nameof(segmentCount), "Must be at least 2");
			}

			this.P0 = p0;
			this.P1 = p1;
			this.P2 = p2;
			this.P3 = p3;
			this._distancesToPoints = this.GetDistancesToPoints(segmentCount);
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
			return (u3 * this.P0) + (3 * u2 * t * this.P1) + (3 * u * t2 * this.P2) + (t3 * this.P3);
		}

		public float EvaluateForDistance(float distance)
		{
			return this.EvaluatePositionForDistance(distance).y;
		}

		public Vector2 EvaluatePositionForDistance(float distance)
		{
			if (distance <= 0)
			{
				return this.P0;
			}

			if (distance >= 1)
			{
				return this.P3;
			}

			float targetLength = distance * this.Length;
			int segmentIndex = (int) (distance * (this._distancesToPoints.Length - 1));

			var segment = this._distancesToPoints[segmentIndex];
			var nextSegment = this._distancesToPoints[segmentIndex + 1];

			float t = (targetLength - segment.Distance) / (nextSegment.Distance - segment.Distance);
			return Vector2.Lerp(segment.Point, nextSegment.Point, t);
		}

		private DistanceToPoint[] GetDistancesToPoints(int count)
		{
			var initialSegments = new DistanceToPoint[count + 1];
			initialSegments[0] = new DistanceToPoint(0, this.P0);

			for (int i = 1; i <= count; i++)
			{
				float t = i / (float) count;
				var point = this.EvaluatePosition(t);
				float totalLength = initialSegments[i - 1].Distance + Vector2.Distance(point, initialSegments[i - 1].Point);
				initialSegments[i] = new DistanceToPoint(totalLength, point);
			}

			float step = initialSegments[count].Distance / count;
			var spacedSegments = new DistanceToPoint[count + 1];

			for (int i = 0; i <= count; i++)
			{
				float distance = i * step;
				spacedSegments[i] = new DistanceToPoint(distance, FindPositionForDistance(distance, initialSegments));
			}

			return spacedSegments;
		}
	}
}

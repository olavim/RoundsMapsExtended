using FluentAssertions;
using Surity;
using UnityEngine;

namespace MapsExt.Tests
{
	[TestClass]
	public class BezierCurveTests
	{
		[Test]
		public void Test_LinearBezierCurveValues()
		{
			var curve1 = new BezierCurve(
				new(0, 0),
				new(1, 1),
				new(2, 2),
				new(3, 3)
			);
			var curve2 = new BezierCurve(
				new(0, 0),
				new(0.1f, 0.1f),
				new(2.9f, 2.9f),
				new(3, 3)
			);

			const float precision = 0.000001f;

			curve1.Evaluate(0).Should().BeApproximately(0, precision);
			curve1.Evaluate(0.1f).Should().BeApproximately(0.3f, precision);
			curve1.Evaluate(0.5f).Should().BeApproximately(1.5f, precision);
			curve1.Evaluate(0.9f).Should().BeApproximately(2.7f, precision);
			curve1.Evaluate(1).Should().BeApproximately(3, precision);

			curve2.Evaluate(0).Should().BeApproximately(0, precision);
			curve2.Evaluate(0.1f).Should().NotBeApproximately(0.3f, precision);
			curve2.Evaluate(0.5f).Should().BeApproximately(1.5f, precision);
			curve2.Evaluate(0.9f).Should().NotBeApproximately(2.7f, precision);
			curve2.Evaluate(1).Should().BeApproximately(3, precision);
		}

		[Test]
		public void Test_LinearBezierCurveDistances()
		{
			var curve1 = new BezierCurve(
				new(0, 0),
				new(1, 1),
				new(2, 2),
				new(3, 3)
			);

			var curve2 = new BezierCurve(
				new(0, 0),
				new(0.1f, 0.1f),
				new(2.9f, 2.9f),
				new(3, 3)
			);

			const float precision = 0.000001f;

			curve1.EvaluateForDistance(0).Should().BeApproximately(0, precision);
			curve1.EvaluateForDistance(0.1f).Should().BeApproximately(0.3f, precision);
			curve1.EvaluateForDistance(0.5f).Should().BeApproximately(1.5f, precision);
			curve1.EvaluateForDistance(0.9f).Should().BeApproximately(2.7f, precision);
			curve1.EvaluateForDistance(1).Should().BeApproximately(3, precision);

			curve2.EvaluateForDistance(0).Should().BeApproximately(0, precision);
			curve2.EvaluateForDistance(0.1f).Should().BeApproximately(0.3f, precision);
			curve2.EvaluateForDistance(0.5f).Should().BeApproximately(1.5f, precision);
			curve2.EvaluateForDistance(0.9f).Should().BeApproximately(2.7f, precision);
			curve2.EvaluateForDistance(1).Should().BeApproximately(3, precision);
		}

		[Test]
		public void Test_BezierCurvePointSpacing()
		{
			var curves = new BezierCurve[] {
				new(new(0.33f, 0.33f), new(0.66f, 0.66f)),
				new(new(0.1f, 0.1f), new(0.9f, 0.9f)),
				new(new(0.12f, 0), new(0.39f, 0)),
				new(new(0.61f, 1), new(0.88f, 1)),
				new(new(0.33f, 0), new(0.66f, 1))
			};

			const float precision = 0.001f;

			foreach (var curve in curves)
			{
				var points = curve.Points;
				float step = curve.Length / points.Length;

				for (var i = 1; i < points.Length - 1; i++)
				{
					float distance = Vector2.Distance(points[i], points[i - 1]);
					distance.Should().BeApproximately(step, precision);
				}
			}
		}
	}
}

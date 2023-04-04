using FluentAssertions;
using Surity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapsExt.Tests
{
	[TestClass(Only = true)]
	public class BezierCurveTests
	{
		[Test]
		public void Test_LinearBezierCurveValues()
		{
			var curve1 = new BezierCurve(
				new Vector2(0, 0),
				new Vector2(1, 1),
				new Vector2(2, 2),
				new Vector2(3, 3)
			);
			var curve2 = new BezierCurve(
				new Vector2(0, 0),
				new Vector2(0.1f, 0.1f),
				new Vector2(2.9f, 2.9f),
				new Vector2(3, 3)
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
				new Vector2(0, 0),
				new Vector2(1, 1),
				new Vector2(2, 2),
				new Vector2(3, 3)
			);

			var curve2 = new BezierCurve(
				new Vector2(0, 0),
				new Vector2(0.1f, 0.1f),
				new Vector2(2.9f, 2.9f),
				new Vector2(3, 3)
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
	}
}

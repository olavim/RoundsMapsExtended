using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using UnityEngine;

namespace MapsExt.Tests
{
	public static class Vector3Extensions
	{
		public static Vector3Assertions Should(this Vector3 instance)
		{
			return new Vector3Assertions(instance);
		}
	}

	public class Vector3Assertions : ReferenceTypeAssertions<Vector3, Vector3Assertions>
	{
		public Vector3Assertions(Vector3 subject) : base(subject) { }

		protected override string Identifier => "Vector3";

		public AndConstraint<Vector3Assertions> Be(Vector3 expected, string because = "", params object[] becauseArgs)
		{
			Execute.Assertion
					.BecauseOf(because, becauseArgs)
					.ForCondition(this.Subject == expected)
					.WithDefaultIdentifier(this.Identifier)
					.FailWith("Expected {context} to be {0}{reason}, but found {1}.", expected, this.Subject);

			return new AndConstraint<Vector3Assertions>(this);
		}

		public AndConstraint<Vector3Assertions> Be(Vector2 expected, string because = "", params object[] becauseArgs)
		{
			Execute.Assertion
					.BecauseOf(because, becauseArgs)
					.ForCondition((Vector2) this.Subject == expected)
					.WithDefaultIdentifier(this.Identifier)
					.FailWith("Expected {context} to be {0}{reason}, but found {1}.", expected, (Vector2) this.Subject);

			return new AndConstraint<Vector3Assertions>(this);
		}

		public AndConstraint<Vector3Assertions> BeApproximately(Vector3 expected, int precision = 6, string because = "", params object[] becauseArgs)
		{
			Execute.Assertion
					.BecauseOf(because, becauseArgs)
					.ForCondition(this.Subject.Round(precision) == expected.Round(precision))
					.WithDefaultIdentifier(this.Identifier)
					.FailWith("Expected {context:Vector3} to be approximately {0}, but found {1}.", expected, this.Subject);

			return new AndConstraint<Vector3Assertions>(this);
		}

		public AndConstraint<Vector3Assertions> BeApproximately(Vector2 expected, int precision = 6, string because = "", params object[] becauseArgs)
		{
			var v2Subject = (Vector2) this.Subject;

			Execute.Assertion
					.BecauseOf(because, becauseArgs)
					.ForCondition(v2Subject.Round(precision) == expected.Round(precision))
					.WithDefaultIdentifier(this.Identifier)
					.FailWith("Expected {context} to be approximately {0}{reason}, but found {1}.", expected, (Vector2) this.Subject);

			return new AndConstraint<Vector3Assertions>(this);
		}
	}
}

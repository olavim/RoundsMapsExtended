using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using UnityEngine;

namespace MapsExt.Tests
{
	public static class Vector2Extensions
	{
		public static Vector2Assertions Should(this Vector2 instance)
		{
			return new Vector2Assertions(instance);
		}
	}

	public class Vector2Assertions : ReferenceTypeAssertions<Vector2, Vector2Assertions>
	{
		public Vector2Assertions(Vector2 subject) : base(subject) { }

		protected override string Identifier => "Vector2";

		public AndConstraint<Vector2Assertions> Be(Vector2 expected, string because = "", params object[] becauseArgs)
		{
			Execute.Assertion
					.BecauseOf(because, becauseArgs)
					.ForCondition(this.Subject == expected)
					.WithDefaultIdentifier(this.Identifier)
					.FailWith("Expected {context} to be {0}{reason}, but found {1}.", expected, this.Subject);

			return new AndConstraint<Vector2Assertions>(this);
		}

		public AndConstraint<Vector2Assertions> Be(Vector3 expected, string because = "", params object[] becauseArgs)
		{
			Execute.Assertion
					.BecauseOf(because, becauseArgs)
					.ForCondition(this.Subject == (Vector2) expected)
					.WithDefaultIdentifier(this.Identifier)
					.FailWith("Expected {context} to be {0}{reason}, but found {1}.", (Vector2) expected, this.Subject);

			return new AndConstraint<Vector2Assertions>(this);
		}

		public AndConstraint<Vector2Assertions> BeApproximately(Vector2 expected, int precision = 6, string because = "", params object[] becauseArgs)
		{
			Execute.Assertion
					.BecauseOf(because, becauseArgs)
					.ForCondition(this.Subject.Round(precision) == expected.Round(precision))
					.WithDefaultIdentifier(this.Identifier)
					.FailWith("Expected {context:Vector3} to be approximately {0}, but found {1}.", expected, this.Subject);

			return new AndConstraint<Vector2Assertions>(this);
		}

		public AndConstraint<Vector2Assertions> BeApproximately(Vector3 expected, int precision = 6, string because = "", params object[] becauseArgs)
		{
			Execute.Assertion
					.BecauseOf(because, becauseArgs)
					.ForCondition(this.Subject.Round(precision) == (Vector2) expected.Round(precision))
					.WithDefaultIdentifier(this.Identifier)
					.FailWith("Expected {context} to be approximately {0}{reason}, but found {1}.", (Vector2) expected, this.Subject);

			return new AndConstraint<Vector2Assertions>(this);
		}
	}
}

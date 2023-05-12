using System;

namespace MapsExt.Properties
{
	public abstract class ValueProperty<T> : IProperty, IEquatable<ValueProperty<T>> where T : IEquatable<T>
	{
		public abstract T Value { get; }

		protected ValueProperty() { }

		public override string ToString() => this.Value.ToString();

		public virtual bool Equals(ValueProperty<T> other)
		{
			if (this.Value is null || other.Value is null)
			{
				return this.Value is null && other.Value is null;
			}

			return this.Value.Equals(other.Value);
		}

		public override bool Equals(object other) => other is ValueProperty<T> prop && this == prop;
		public override int GetHashCode() => this.Value.GetHashCode();

		public static bool operator ==(ValueProperty<T> a, ValueProperty<T> b)
		{
			if (a is null || b is null)
			{
				return a is null && b is null;
			}

			return a.Equals(b);
		}
		public static bool operator !=(ValueProperty<T> a, ValueProperty<T> b) => !(a == b);
	}
}
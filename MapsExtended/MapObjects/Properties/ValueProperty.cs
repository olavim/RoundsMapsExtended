using System;

namespace MapsExt.MapObjects.Properties
{
	public abstract class ValueProperty<T> : IProperty, IEquatable<ValueProperty<T>> where T : IEquatable<T>
	{
		public T Value { get; set; }

		protected ValueProperty() { }

		protected ValueProperty(T value)
		{
			this.Value = value;
		}

		public override string ToString() => this.Value.ToString();

		public virtual bool Equals(ValueProperty<T> other) => this.Value.Equals(other.Value);
		public override bool Equals(object other) => other is ValueProperty<T> prop && this.Equals(prop);
		public override int GetHashCode() => this.Value.GetHashCode();

		public static bool operator ==(ValueProperty<T> a, ValueProperty<T> b) => a.Equals(b);
		public static bool operator !=(ValueProperty<T> a, ValueProperty<T> b) => !a.Equals(b);
	}
}
using System;

namespace MapsExt.MapObjects.Properties
{
	public abstract class ValueProperty<T> : IProperty, IEquatable<ValueProperty<T>> where T : IEquatable<T>
	{
		public T value;

		protected ValueProperty() { }

		protected ValueProperty(T value)
		{
			this.value = value;
		}

		public override string ToString() => this.value.ToString();

		public virtual bool Equals(ValueProperty<T> other) => this.value.Equals(other.value);
		public override bool Equals(object other) => other is ValueProperty<T> prop && this.Equals(prop);
		public override int GetHashCode() => this.value.GetHashCode();

		public static bool operator ==(ValueProperty<T> a, ValueProperty<T> b) => a.Equals(b);
		public static bool operator !=(ValueProperty<T> a, ValueProperty<T> b) => !a.Equals(b);
	}
}
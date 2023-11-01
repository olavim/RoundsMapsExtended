using System;

namespace MapsExt.Compatibility
{
	public interface IPredicateDisabler<T>
	{
		void AddDisableCase(Predicate<T> predicate);
	}
}

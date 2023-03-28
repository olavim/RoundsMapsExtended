using UnityEngine;

namespace MapsExt.MapObjects.Properties
{
	public interface ILinearProperty : IProperty
	{
		IProperty Lerp(IProperty end, float t);
	}

	public interface ILinearProperty<T> : ILinearProperty where T : IProperty
	{
		T Lerp(T end, float t);
	}
}

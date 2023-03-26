using UnityEngine;

namespace MapsExt.MapObjects.Properties
{
	public interface ILinearProperty : IMapObjectProperty
	{
		IMapObjectProperty Lerp(IMapObjectProperty end, float t);
	}

	public interface ILinearProperty<T> : ILinearProperty where T : IMapObjectProperty
	{
		T Lerp(T end, float t);
	}
}

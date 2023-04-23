using UnityEngine;

namespace MapsExt.Properties
{
	public interface IPropertyReader<T> where T : IProperty
	{
		T ReadProperty(GameObject instance);
	}
}

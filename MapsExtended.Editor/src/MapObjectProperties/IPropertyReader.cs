using MapsExt.Properties;
using UnityEngine;

namespace MapsExt.Editor.Properties
{
	public interface IPropertyReader<T> where T : IProperty
	{
		T ReadProperty(GameObject instance);
	}
}

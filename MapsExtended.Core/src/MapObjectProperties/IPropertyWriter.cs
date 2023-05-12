using UnityEngine;

namespace MapsExt.Properties
{
	public interface IPropertyWriter<T> where T : IProperty
	{
		void WriteProperty(T property, GameObject target);
	}
}

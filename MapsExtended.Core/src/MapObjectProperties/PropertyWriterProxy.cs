using Sirenix.Utilities;
using System;
using System.Reflection;
using UnityEngine;

namespace MapsExt.Properties
{
	public class PropertyWriterProxy : IPropertyWriter<IProperty>
	{
		private readonly object _methodTarget;
		private readonly MethodInfo _method;

		public PropertyWriterProxy(object serializer, Type propertyType)
		{
			var serializerType = serializer.GetType();

			if (!serializerType.ImplementsOpenGenericInterface(typeof(IPropertyWriter<>)))
			{
				throw new ArgumentException($"Serializer must implement {typeof(IPropertyWriter<>)}", nameof(serializer));
			}

			if (serializerType.GetArgumentsOfInheritedOpenGenericInterface(typeof(IPropertyWriter<>))[0] != propertyType)
			{
				throw new ArgumentException($"Serializer must implement {typeof(IPropertyWriter<>)}<{propertyType}>", nameof(serializer));
			}

			this._methodTarget = serializer;
			this._method = serializer.GetType().GetMethod(
				nameof(IPropertyWriter<IProperty>.WriteProperty),
				new[] { propertyType, typeof(GameObject) }
			);
		}

		public void WriteProperty(IProperty property, GameObject target)
		{
			this._method.Invoke(this._methodTarget, new object[] { property, target });
		}
	}
}

using MapsExt.Properties;
using Sirenix.Utilities;
using System;
using System.Reflection;
using UnityEngine;

namespace MapsExt.Editor.Properties
{
	internal class PropertyReaderProxy : IPropertyReader<IProperty>
	{
		private readonly object _methodTarget;
		private readonly MethodInfo _method;

		public PropertyReaderProxy(object serializer, Type propertyType)
		{
			var serializerType = serializer.GetType();

			if (!serializerType.ImplementsOpenGenericInterface(typeof(IPropertyReader<>)))
			{
				throw new ArgumentException($"Serializer must implement {typeof(IPropertyReader<>)}", nameof(serializer));
			}

			if (serializerType.GetArgumentsOfInheritedOpenGenericInterface(typeof(IPropertyReader<>))[0] != propertyType)
			{
				throw new ArgumentException($"Serializer must implement {typeof(IPropertyReader<>)}<{propertyType}>", nameof(serializer));
			}

			this._methodTarget = serializer;
			this._method = serializer.GetType().GetMethod(
				nameof(IPropertyReader<IProperty>.ReadProperty),
				new[] { typeof(GameObject) }
			);
		}

		public IProperty ReadProperty(GameObject instance)
		{
			return (IProperty) this._method.Invoke(this._methodTarget, new object[] { instance });
		}
	}
}

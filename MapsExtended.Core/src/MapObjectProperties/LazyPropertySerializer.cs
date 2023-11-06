using MapsExt.Utils;
using Sirenix.Utilities;
using System;
using UnityEngine;

namespace MapsExt.Properties
{
	public sealed class LazyPropertySerializer : IPropertySerializer<IProperty>
	{
		private delegate void WritePropertyDelegate(IProperty property, GameObject target);
		private delegate IProperty ReadPropertyDelegate(GameObject instance);

		private readonly object _methodTarget;
		private readonly WritePropertyDelegate _writeDelegate;
		private readonly ReadPropertyDelegate _readDelegate;
		private readonly Type _propertyType;

		public LazyPropertySerializer(object serializer, Type propertyType)
		{
			this._propertyType = propertyType;
			this._methodTarget = serializer;

			var serializerType = serializer.GetType();

			if (
				serializerType.ImplementsOpenGenericInterface(typeof(IPropertyWriter<>)) &&
				serializerType.GetArgumentsOfInheritedOpenGenericInterface(typeof(IPropertyWriter<>))[0] == propertyType
			)
			{
				var methodInfo = serializer.GetType().GetMethod(
					nameof(IPropertySerializer<IProperty>.WriteProperty),
					new[] { propertyType, typeof(GameObject) }
				);
				this._writeDelegate = ReflectionUtils.ConvertMethod<WritePropertyDelegate>(serializer, methodInfo);
			}

			if (
				serializerType.ImplementsOpenGenericInterface(typeof(IPropertyReader<>)) &&
				serializerType.GetArgumentsOfInheritedOpenGenericInterface(typeof(IPropertyReader<>))[0] == propertyType
			)
			{
				var methodInfo = serializer.GetType().GetMethod(
					nameof(IPropertyReader<IProperty>.ReadProperty),
					new[] { typeof(GameObject) }
				);
				this._readDelegate = ReflectionUtils.ConvertMethod<ReadPropertyDelegate>(serializer, methodInfo);
			}
		}

		public IProperty ReadProperty(GameObject instance)
		{
			if (this._readDelegate == null)
			{
				throw new InvalidOperationException($"Serializer {this._methodTarget.GetType()} does not implement {typeof(IPropertyReader<>).Name}<{this._propertyType.Name}>");
			}

			return this._readDelegate(instance);
		}

		public void WriteProperty(IProperty property, GameObject target)
		{
			if (this._writeDelegate == null)
			{
				throw new InvalidOperationException($"Serializer {this._methodTarget.GetType()} does not implement {typeof(IPropertyWriter<>).Name}<{this._propertyType.Name}>");
			}

			this._writeDelegate(property, target);
		}
	}
}

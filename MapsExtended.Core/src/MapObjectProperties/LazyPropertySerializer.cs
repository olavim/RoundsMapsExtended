using Sirenix.Utilities;
using System;
using System.Reflection;
using UnityEngine;

namespace MapsExt.Properties
{
	public sealed class LazyPropertySerializer : IPropertySerializer<IProperty>
	{
		private readonly object _methodTarget;
		private readonly MethodInfo _writeMethod;
		private readonly MethodInfo _readMethod;
		private readonly Type _propertyType;
		private readonly bool _serializerImplementsWriter;
		private readonly bool _serializerImplementsReader;

		public LazyPropertySerializer(object serializer, Type propertyType)
		{
			var serializerType = serializer.GetType();

			if (
				serializerType.ImplementsOpenGenericInterface(typeof(IPropertyWriter<>)) &&
				serializerType.GetArgumentsOfInheritedOpenGenericInterface(typeof(IPropertyWriter<>))[0] == propertyType
			)
			{
				this._serializerImplementsWriter = true;
			}

			if (
				serializerType.ImplementsOpenGenericInterface(typeof(IPropertyReader<>)) &&
				serializerType.GetArgumentsOfInheritedOpenGenericInterface(typeof(IPropertyReader<>))[0] == propertyType
			)
			{
				this._serializerImplementsReader = true;
			}

			this._propertyType = propertyType;
			this._methodTarget = serializer;

			this._writeMethod = serializer.GetType().GetMethod(
				nameof(IPropertySerializer<IProperty>.WriteProperty),
				new[] { propertyType, typeof(GameObject) }
			);

			this._readMethod = serializer.GetType().GetMethod(
				nameof(IPropertyReader<IProperty>.ReadProperty),
				new[] { typeof(GameObject) }
			);
		}

		public IProperty ReadProperty(GameObject instance)
		{
			if (!this._serializerImplementsReader)
			{
				throw new InvalidOperationException($"Serializer {this._methodTarget.GetType()} does not implement {typeof(IPropertyReader<>).Name}<{this._propertyType.Name}>");
			}

			return (IProperty) this._readMethod.Invoke(this._methodTarget, new object[] { instance });
		}

		public void WriteProperty(IProperty property, GameObject target)
		{
			if (!this._serializerImplementsWriter)
			{
				throw new InvalidOperationException($"Serializer {this._methodTarget.GetType()} does not implement {typeof(IPropertyWriter<>).Name}<{this._propertyType.Name}>");
			}

			this._writeMethod.Invoke(this._methodTarget, new object[] { property, target });
		}
	}
}

using System;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace MapsExt
{
	public static class ReflectionUtils
	{
		static TDelegate ConvertMethod<TDelegate>(object instance, MethodInfo methodInfo) where TDelegate : Delegate
		{
			var requestedMethodInfo = typeof(TDelegate).GetMethod("Invoke");
			var requestedParameterTypes = requestedMethodInfo.GetParameters().Select(p => p.ParameterType).ToList();

			var parameterTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToList();
			var parameterNames = methodInfo.GetParameters().Select(p => p.Name).ToList();

			var methodParameters = new List<ParameterExpression>();
			var convertedMethodParameters = new List<Expression>();

			for (int i = 0; i < requestedParameterTypes.Count; i++)
			{
				var requestedType = requestedParameterTypes[i];
				var currentType = parameterTypes[i];
				var name = parameterNames[i];

				var param = Expression.Parameter(requestedType, name);
				methodParameters.Add(param);

				var convertedParam = currentType == requestedType ? (Expression) param : Expression.Convert(param, currentType);
				convertedMethodParameters.Add(convertedParam);
			}

			var lambda = Expression.Lambda<TDelegate>(
					Expression.Call(
						instance == null ? null : Expression.Constant(instance),
						methodInfo,
						convertedMethodParameters
					),
					methodParameters.ToArray()
				);

			return lambda.Compile();
		}

		static TDelegate ConvertDelegate<TDelegate>(Delegate del) where TDelegate : Delegate
		{
			var delegates = del.GetInvocationList().Select(inv => ConvertMethod<TDelegate>(inv.Target, inv.Method)).ToArray();
			return (TDelegate) Delegate.Combine(delegates);
		}

		/// <summary>
		/// Finds and converts a static method from 'type', with the specified attribute of 'attributeType', to TDelegate.
		/// If such a method is not found, also looks for attributed static properties and converts their values to TDelegate.
		/// </summary>
		/// <typeparam name="TDelegate">Type to convert the attributed method to.</typeparam>
		/// <param name="type">Type from where methods are searched from.</param>
		/// <param name="attributeType">Type of the attribute to look for.</param>
		public static TDelegate GetAttributedMethod<TDelegate>(Type type, Type attributeType) where TDelegate : Delegate
		{
			var deserializerMethod = type
				.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.FirstOrDefault(m => m.GetCustomAttribute(attributeType) != null);

			if (deserializerMethod != null)
			{
				return ReflectionUtils.ConvertMethod<TDelegate>(null, deserializerMethod);
			}

			var deserializer = ReflectionUtils.GetAttributedProperty<Delegate>(type, attributeType);

			if (deserializer != null)
			{
				return ReflectionUtils.ConvertDelegate<TDelegate>(deserializer);
			}

			return null;
		}

		public static T GetAttributedProperty<T>(Type type, Type attributeType) where T : class
		{
			return type
				.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.FirstOrDefault(m => m.GetCustomAttribute(attributeType) != null)
				?.GetValue(null) as T;
		}

		public static IEnumerable<Type> GetParentTypes(this Type type)
		{
			if (type == null)
			{
				yield break;
			}

			foreach (var i in type.GetInterfaces())
			{
				yield return i;
			}

			var currentBaseType = type.BaseType;
			while (currentBaseType != null)
			{
				yield return currentBaseType;
				currentBaseType = currentBaseType.BaseType;
			}
		}
	}
}

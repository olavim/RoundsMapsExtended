using System;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace MapsExt.Utils
{
	public static class ReflectionUtils
	{
		public static TDelegate ConvertMethod<TDelegate>(object instance, MethodInfo methodInfo) where TDelegate : Delegate
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

		/// <summary>
		/// Finds and converts a static method from 'type', with the specified attribute of 'attributeType', to TDelegate.
		/// If such a method is not found, also looks for attributed static properties and converts their values to TDelegate.
		/// </summary>
		/// <typeparam name="TDelegate">Type to convert the attributed method to.</typeparam>
		/// <param name="type">Type from where methods are searched from.</param>
		/// <param name="attributeType">Type of the attribute to look for.</param>
		[Obsolete("Deprecated")]
		public static TDelegate GetAttributedMethod<TDelegate>(Type type, Type attributeType) where TDelegate : Delegate
		{
			var deserializerMethod = Array.Find(
				type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic),
				m => m.GetCustomAttribute(attributeType) != null
			);

			if (deserializerMethod != null)
			{
				return ConvertMethod<TDelegate>(null, deserializerMethod);
			}

			var deserializer = GetAttributedProperty<Delegate>(type, attributeType);

			if (deserializer != null)
			{
				return ConvertDelegate<TDelegate>(deserializer);
			}

			return null;
		}

		[Obsolete("Deprecated")]
		public static T GetAttributedProperty<T>(Type type, Type attributeType) where T : class
		{
			var prop = Array.Find(
				type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic),
				m => m.GetCustomAttribute(attributeType) != null
			);
			try
			{
				return prop?.GetValue(null) as T;
			}
			catch (TargetInvocationException ex)
			{
				throw new Exception($"Could not get value of property {prop?.Name} from {type.Name}", ex.GetBaseException());
			}
		}

		private static TDelegate ConvertDelegate<TDelegate>(Delegate del) where TDelegate : Delegate
		{
			var delegates = del.GetInvocationList().Select(inv => ConvertMethod<TDelegate>(inv.Target, inv.Method)).ToArray();
			return (TDelegate) Delegate.Combine(delegates);
		}

		public static Type[] GetAssemblyTypes(Assembly assembly)
		{
			try
			{
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				return e.Types.Where(t => t != null).ToArray();
			}
		}
	}
}

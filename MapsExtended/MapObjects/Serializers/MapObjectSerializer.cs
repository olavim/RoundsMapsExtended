using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnboundLib;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MapsExt.MapObjects
{
	public class MapObject
	{
		public bool active = true;
	}

	public delegate void SerializerAction<T>(GameObject instance, T target) where T : MapObject;
	public delegate void DeserializerAction<T>(T data, GameObject target) where T : MapObject;

	public static class BaseMapObjectSerializer
	{
		public static void Serialize(GameObject instance, MapObject target)
		{
			target.active = instance.activeSelf;
		}

		public static void Deserialize(MapObject data, GameObject target)
		{
			var c = target.GetOrAddComponent<MapObjectInstance>();
			c.dataType = data.GetType();
			target.SetActive(data.active);
		}
	}

	public static class MapObjectSerializerUtils
	{
		static DeserializerAction<MapObject> CreateDeserializerDelegate(Delegate del)
		{
			DeserializerAction<MapObject> res = null;

			foreach (var invocation in del.GetInvocationList())
			{
				var methodInfo = invocation.Method;
				var data = Expression.Parameter(typeof(MapObject), "data");
				var target = Expression.Parameter(typeof(GameObject), "target");

				var lambda = Expression.Lambda<DeserializerAction<MapObject>>(
						Expression.Call(
							Expression.Constant(invocation.Target),
							methodInfo,
							Expression.Convert(data, methodInfo.GetParameters()[0].ParameterType),
							target
						),
						data,
						target
					);

				res += lambda.Compile();
			}

			return res;
		}

		static SerializerAction<MapObject> CreateSerializerDelegate(Delegate del)
		{
			SerializerAction<MapObject> res = null;

			foreach (var invocation in del.GetInvocationList())
			{
				var methodInfo = invocation.Method;
				var instance = Expression.Parameter(typeof(GameObject), "instance");
				var target = Expression.Parameter(typeof(MapObject), "target");

				var lambda = Expression.Lambda<SerializerAction<MapObject>>(
						Expression.Call(
							Expression.Constant(invocation.Target),
							methodInfo,
							instance,
							Expression.Convert(target, methodInfo.GetParameters()[1].ParameterType)
						),
						instance,
						target
					);

				res += lambda.Compile();
			}

			return res;
		}

		public static SerializerAction<MapObject> GetTypeSerializer<T>(Type type) where T : Attribute
		{
			var serializerMethod = type
				.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.FirstOrDefault(m => m.GetCustomAttribute<T>() != null);

			if (serializerMethod != null)
			{
				return (instance, target) => serializerMethod.Invoke(null, new object[] { instance, (dynamic)target });
			}

			var serializerProp = type
				.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.FirstOrDefault(m => m.GetCustomAttribute<T>() != null);

			if (serializerProp != null)
			{
				var value = serializerProp.GetValue(null);
				Delegate method = (Delegate)value;
				SerializerAction<MapObject> serialize = MapObjectSerializerUtils.CreateSerializerDelegate(method);
				return (data, target) => serialize(data, target);
			}

			return null;
		}

		public static DeserializerAction<MapObject> GetTypeDeserializer<T>(Type type) where T : Attribute
		{
			var deserializerMethod = type
				.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.FirstOrDefault(m => m.GetCustomAttribute<T>() != null);

			if (deserializerMethod != null)
			{
				return (data, target) => deserializerMethod.Invoke(null, new object[] { (dynamic)data, target });
			}

			var deserializerProp = type
				.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.FirstOrDefault(m => m.GetCustomAttribute<T>() != null);

			if (deserializerProp != null)
			{
				var value = deserializerProp.GetValue(null);
				Delegate method = (Delegate)value;
				DeserializerAction<MapObject> deserialize = MapObjectSerializerUtils.CreateDeserializerDelegate(method);
				return (instance, target) => deserialize(instance, target);
			}

			return null;
		}
	}

	public class MapObjectInstance : MonoBehaviour
	{
		public Type dataType;

		public void Start()
		{
			this.FixShadow();
		}

		private void FixShadow()
		{
			var collider = this.gameObject.GetComponent<Collider2D>();

			if (collider == null)
			{
				return;
			}

			var sf = this.gameObject.GetOrAddComponent<SFPolygon>();
			sf.opacity = 0.5f;

			if (collider is PolygonCollider2D || collider is BoxCollider2D)
			{
				sf.CopyFromCollider(collider);
			}

			if (collider is CircleCollider2D circleCollider)
			{
				int numVertices = 24;
				float anglePerVertex = 360f / numVertices;
				float radius = circleCollider.radius;

				var identity = new Vector3(0, radius, 0);
				var vertices = new List<Vector2>();

				for (int i = 0; i < numVertices; i++)
				{
					var rotation = Quaternion.Euler(0, 0, i * anglePerVertex);
					var point = rotation * identity;
					vertices.Add(new Vector2(point.x, point.y));
				}

				sf.SetPath(0, vertices.ToArray());
			}
		}
	}
}

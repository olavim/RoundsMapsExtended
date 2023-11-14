using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MapsExt.Compatibility
{
	public abstract class MapLoader
	{
		class MapLoaderException : Exception
		{
			public MapLoaderException() { }

			public MapLoaderException(string message) : base(message) { }

			public MapLoaderException(string message, Exception innerException) : base(message, innerException) { }
		}

		public static MapLoader ForVersion(string version, DeserializationContext context = null)
		{
			if (version.StartsWith("0."))
			{
				return new V0MapLoader(context);
			}

			if (version.StartsWith("1."))
			{
				return new V1MapLoader(context);
			}

			throw new ArgumentException($"Unsupported map version: {version}");
		}

		public static CustomMap Load(Stream stream, DeserializationContext context = null)
		{
			string version = ReadValue(stream, "_version", context) ?? "0.0.0";
			return ForVersion(version, context).Load(stream) ?? throw new MapLoaderException("Map load failed");
		}

		public static CustomMapInfo LoadInfo(Stream stream, DeserializationContext context = null)
		{
			using var memoryStream = new MemoryStream();
			long pos = stream.Position;
			stream.CopyTo(memoryStream);
			stream.Seek(pos, SeekOrigin.Begin);

			string id = ReadValue(stream, "_id", context) ?? ReadValue(stream, "id", context) ?? throw new MapLoaderException("Map info load failed");
			string name = ReadValue(stream, "_name", context) ?? ReadValue(stream, "name", context) ?? id;
			string version = ReadValue(stream, "_version", context) ?? "0.0.0";

			return new CustomMapInfo(id, name, version, memoryStream.ToArray());
		}

		public static CustomMap LoadResource(string resourceName, DeserializationContext context = null)
		{
			try
			{
				var assembly = Assembly.GetCallingAssembly();
				using var stream = assembly.GetManifestResourceStream(resourceName);
				return Load(stream, context);
			}
			catch (Exception ex)
			{
				throw new MapLoaderException($"Map load failed for resource: {resourceName}", ex);
			}
		}

		public static CustomMap LoadPath(string path, DeserializationContext context = null)
		{
			try
			{
				var bytes = File.ReadAllBytes(path);
				using var stream = new MemoryStream(bytes);
				return Load(stream, context);
			}
			catch (Exception ex)
			{
				throw new MapLoaderException($"Map load failed for path: {path}", ex);
			}
		}

		public static CustomMapInfo LoadInfoFromPath(string path, DeserializationContext context = null)
		{
			try
			{
				var bytes = File.ReadAllBytes(path);
				using var stream = new MemoryStream(bytes);
				return LoadInfo(stream, context);
			}
			catch (Exception ex)
			{
				throw new MapLoaderException($"Map info load failed for path: {path}", ex);
			}
		}

		public static string ReadValue(Stream stream, string key, DeserializationContext context = null)
		{
			long pos = stream.Position;

			try
			{
				int depth = -1;

				using var reader = new JsonTextReader(stream, context ?? new());
				EntryType entry = 0;
				string currentKey = null;
				string value = null;

				while (entry != EntryType.EndOfStream)
				{
					reader.ReadToNextEntry(out currentKey, out value, out entry);

					if (entry == EntryType.StartOfNode)
					{
						depth++;
					}

					if (entry == EntryType.EndOfNode)
					{
						depth--;
					}

					if (currentKey == key && depth == 0)
					{
						return value.Trim('"');
					}
				}

				return null;
			}
			finally
			{
				stream.Seek(pos, SeekOrigin.Begin);
			}
		}

		public static string[] ReadValues(Stream stream, string key, DeserializationContext context = null)
		{
			long pos = stream.Position;
			var values = new List<string>();

			try
			{
				using var reader = new JsonTextReader(stream, context ?? new());
				EntryType entry = 0;
				string currentKey = null;
				string value = null;

				while (entry != EntryType.EndOfStream)
				{
					reader.ReadToNextEntry(out currentKey, out value, out entry);

					if (currentKey == key)
					{
						values.Add(value);
					}
				}

				return values.ToArray();
			}
			finally
			{
				stream.Seek(pos, SeekOrigin.Begin);
			}
		}

		public DeserializationContext Context { get; private set; }

		protected MapLoader(DeserializationContext context = null)
		{
			this.Context = context ?? new();
		}

		public virtual CustomMap LoadResource(string resourceName)
		{
			var assembly = Assembly.GetCallingAssembly();
			using var stream = assembly.GetManifestResourceStream(resourceName);
			return this.Load(stream);
		}

		public abstract CustomMap Load(Stream stream);
	}

	public class V0MapLoader : MapLoader
	{
		public V0MapLoader(DeserializationContext context = null) : base(context)
		{
			this.Context.Binder = new V0.MapObjects.V0MapObjectBinder();
		}

		public override CustomMap Load(Stream stream)
		{
#pragma warning disable CS0618
			return SerializationUtility.DeserializeValue<V0.CustomMap>(stream, DataFormat.JSON, this.Context).Upgrade();
#pragma warning restore CS0618
		}
	}

	public class V1MapLoader : MapLoader
	{
		public V1MapLoader(DeserializationContext context = null) : base(context)
		{
			this.Context.Binder = new V1.MapObjects.V1MapObjectBinder();
		}

		public override CustomMap Load(Stream stream)
		{
			return SerializationUtility.DeserializeValue<CustomMap>(stream, DataFormat.JSON, this.Context);
		}
	}
}

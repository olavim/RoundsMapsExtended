using Sirenix.Serialization;
using System;
using System.IO;
using System.Reflection;

namespace MapsExt.Compatibility
{
	public abstract class MapLoader
	{
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
			long pos = stream.Position;
			string version = ReadVersion(stream, context);
			stream.Seek(pos, SeekOrigin.Begin);
			return ForVersion(version, context).Load(stream);
		}

		public static CustomMap LoadResource(string resourceName, DeserializationContext context = null)
		{
			try
			{
				var assembly = Assembly.GetCallingAssembly();
				using (var stream = assembly.GetManifestResourceStream(resourceName))
				{
					return Load(stream, context);
				}
			}
			catch (Exception)
			{
				UnityEngine.Debug.Log($"Failed to load resource {resourceName}");
				throw;
			}
		}

		public static CustomMap LoadPath(string path, DeserializationContext context = null)
		{
			try
			{
				var bytes = File.ReadAllBytes(path);
				using (var stream = new MemoryStream(bytes))
				{
					return Load(stream, context);
				}
			}
			catch (Exception)
			{
				UnityEngine.Debug.Log($"Failed to load map {path}");
				throw;
			}
		}

		public static string ReadVersion(Stream stream, DeserializationContext context = null)
		{
			int depth = -1;

			using (var reader = new JsonTextReader(stream, context ?? new DeserializationContext()))
			{
				EntryType entry = 0;
				string key = null;
				string value = null;

				while (entry != EntryType.EndOfStream)
				{
					reader.ReadToNextEntry(out key, out value, out entry);

					if (entry == EntryType.StartOfNode)
					{
						depth++;
					}

					if (entry == EntryType.EndOfNode)
					{
						depth--;
					}
				}

				if (key == "version" && depth == 0)
				{
					return value;
				}
			}

			return "0.0.0";
		}

		protected DeserializationContext context;

		protected MapLoader(DeserializationContext context = null)
		{
			this.context = context ?? new DeserializationContext();
		}

		public virtual CustomMap LoadResource(string resourceName)
		{
			var assembly = Assembly.GetCallingAssembly();
			using (var stream = assembly.GetManifestResourceStream(resourceName))
			{
				return this.Load(stream);
			}
		}

		public abstract CustomMap Load(Stream stream);
	}

	public class V0MapLoader : MapLoader
	{
		public V0MapLoader(DeserializationContext context = null) : base(context)
		{
			this.context.Binder = new V0.MapObjects.V0MapObjectBinder();
		}

		public override CustomMap Load(Stream stream)
		{
			object map = SerializationUtility.DeserializeValue<V0.CustomMap>(stream, DataFormat.JSON, this.context);

			while (map is IUpgradable upgradeable)
			{
				map = upgradeable.Upgrade();
			}

			return (CustomMap) map;
		}
	}

	public class V1MapLoader : MapLoader
	{
		public V1MapLoader(DeserializationContext context = null) : base(context) { }

		public override CustomMap Load(Stream stream)
		{
			return SerializationUtility.DeserializeValue<CustomMap>(stream, DataFormat.JSON, this.context);
		}
	}
}

using System;

namespace MapsExt.Compatibility
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class CompatibilityPatchAttribute : Attribute
	{
		public CompatibilityPatchAttribute() { }
	}
}

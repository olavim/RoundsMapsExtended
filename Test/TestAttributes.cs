using System;

namespace MapsExt.Test
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class Test : Attribute { }

	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class BeforeEach : Attribute { }
}

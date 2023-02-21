using System;

namespace MapsExt.Test
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class Test : Attribute { }

	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class BeforeEach : Attribute { }

	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class AfterEach : Attribute { }

	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class BeforeAll : Attribute { }

	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class AfterAll : Attribute { }
}

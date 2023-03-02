using System;
using System.Reflection;

namespace MapsExt.Testing
{
	public class TestStepInfo
	{
		public string Name { get; }
		public Type Type { get; }
		public MethodInfo MethodInfo { get; }

		public TestStepInfo(string name, Type testClassType, MethodInfo methodInfo)
		{
			this.Name = name;
			this.Type = testClassType;
			this.MethodInfo = methodInfo;
		}
	}
}

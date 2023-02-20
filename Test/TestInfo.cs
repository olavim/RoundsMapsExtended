using System;
using System.Reflection;

namespace MapsExt.Test
{
	public class TestInfo
	{
		public string Name { get; }
		public Type ClassType { get; }
		public MethodInfo MethodInfo { get; }
		public TestMethodType MethodType { get; }
		public TestResult Result { get; set; }

		public TestInfo(string name, Type classType, MethodInfo methodInfo, TestMethodType methodType)
		{
			this.Name = name;
			this.ClassType = classType;
			this.MethodInfo = methodInfo;
			this.MethodType = methodType;
		}
	}
}

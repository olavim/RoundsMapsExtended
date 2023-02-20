using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MapsExt.Test
{
	public class TestDiscoverer
	{
		public TestInfo[] FindTests()
		{
			var list = new List<TestInfo>();
			var types = typeof(MapsExtendedTest).Assembly.GetTypes();

			foreach (var type in types)
			{
				var tests = type
					.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
					.Where(m => m.GetCustomAttributes<Test>().Count() > 0)
					.Select(m => new TestInfo(m.Name, type, m, TestMethodType.Test));

				list.AddRange(tests);
			}

			return list.ToArray();
		}

		public TestInfo[] FindBeforeEach(Type type)
		{
			return type
				.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(m => m.GetCustomAttributes<BeforeEach>().Count() > 0)
				.Select(m => new TestInfo(m.Name, type, m, TestMethodType.BeforeEach))
				.ToArray();
		}
	}
}

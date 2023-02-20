using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MapsExt.Test
{
	public class TestRunner
	{
		public IEnumerator DiscoverAndRun()
		{
			var discoverer = new TestDiscoverer();
			var tests = discoverer.FindTests();

			var testsByClass = new Dictionary<Type, List<TestInfo>>();
			foreach (var test in tests)
			{
				if (!testsByClass.ContainsKey(test.ClassType))
				{
					testsByClass.Add(test.ClassType, new List<TestInfo>());
				}

				testsByClass[test.ClassType].Add(test);
			}

			int testsPassed = 0;
			int testsFailed = 0;

			foreach (var type in testsByClass.Keys)
			{
				var instance = AccessTools.CreateInstance(type);
				var beforeEach = discoverer.FindBeforeEach(type);

				foreach (var testInfo in testsByClass[type])
				{
					foreach (var beforeEachInfo in beforeEach)
					{
						yield return this.RunTest(instance, beforeEachInfo);
						if (!beforeEachInfo.Result.pass)
						{
							testInfo.Result = beforeEachInfo.Result;
							break;
						}
					}

					if (testInfo.Result == null)
					{
						yield return this.RunTest(instance, testInfo);
					}

					if (testInfo.Result.pass)
					{
						testsPassed++;
						MapsExtendedTest.Logger.LogInfo($"  [PASS] [{type.Name}] {testInfo.Name}");
					}
					else
					{
						testsFailed++;
						MapsExtendedTest.Logger.LogError($"  [FAIL] [{type.Name}] {testInfo.Name}: {testInfo.Result.failReason}");
					}
				}
			}

			MapsExtendedTest.Logger.LogInfo($"Tests total: {tests.Length}");
			MapsExtendedTest.Logger.LogInfo($"Tests passed: {testsPassed}");
			MapsExtendedTest.Logger.LogInfo($"Tests failed: {testsFailed}");
		}

		public IEnumerator RunTest(object instance, TestInfo test)
		{
			if (test.MethodInfo.ReturnType == typeof(IEnumerator))
			{
				var enumerator = (IEnumerator) test.MethodInfo.Invoke(instance, new object[] { });
				bool moveNext = true;

				while (moveNext)
				{
					try
					{
						moveNext = enumerator.MoveNext();
					}
					catch (Exception e)
					{
						test.Result = TestResult.Fail(e.Message);
						yield break;
					}

					if (moveNext)
					{
						yield return enumerator.Current;
					}
				}
			}
			else
			{
				try
				{
					test.MethodInfo.Invoke(instance, new object[] { });
				}
				catch (TargetInvocationException e)
				{
					test.Result = TestResult.Fail(e.GetBaseException().Message);
					yield break;
				}
			}

			test.Result = TestResult.Pass();
		}
	}
}

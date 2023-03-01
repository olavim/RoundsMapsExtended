using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MapsExt.Test
{
	[TestClass(skip: true)]
	public class TestRunner
	{
		public IEnumerator DiscoverAndRun()
		{
			int testsPassed = 0;
			int testsFailed = 0;

			var testClasses = typeof(MapsExtendedTest).Assembly.GetTypes()
				.Where(t => t.GetCustomAttribute<TestClass>() != null && !t.GetCustomAttribute<TestClass>().skip);

			var testClassesWithOnly = testClasses.Where(t => t.GetCustomAttribute<TestClass>().only);

			if (testClassesWithOnly.Count() > 0)
			{
				testClasses = testClassesWithOnly;
			}

			foreach (var testClass in testClasses)
			{
				var executions = this.GetExecutionGroups(testClass).ToList();

				if (executions.Count == 0)
				{
					continue;
				}

				var instance = AccessTools.CreateInstance(testClass);

				for (int i = 0; i < executions.Count; i++)
				{
					var exec = executions[i];
					yield return this.Run(instance, exec);

					if (exec.Result.pass && i != 0 && i != executions.Count - 1)
					{
						MapsExtendedTest.Logger.LogInfo($"  [PASS] [{testClass.Name}] {exec.Name}");
						testsPassed++;
					}

					if (!exec.Result.pass)
					{
						MapsExtendedTest.Logger.LogError($"  [FAIL] [{testClass.Name}] {exec.Name}: {exec.Result.failReason}");
						testsFailed++;
					}
				}
			}

			MapsExtendedTest.Logger.LogInfo($"Tests total: {testsPassed + testsFailed}");
			MapsExtendedTest.Logger.LogInfo($"Tests passed: {testsPassed}");
			MapsExtendedTest.Logger.LogInfo($"Tests failed: {testsFailed}");
		}

		public IEnumerable<TestExecutionGroup> GetExecutionGroups(Type type)
		{
			var executions = new List<TestExecutionGroup>();

			var testSteps = this.FindSteps<Test>(type);

			if (testSteps.Length == 0)
			{
				return executions;
			}

			var beforeEachSteps = this.FindSteps<BeforeEach>(type);
			var afterEachSteps = this.FindSteps<AfterEach>(type);
			var beforeAllSteps = this.FindSteps<BeforeAll>(type);
			var afterAllSteps = this.FindSteps<AfterAll>(type);

			executions.Add(new TestExecutionGroup("Before All", beforeAllSteps));

			foreach (var testStep in testSteps)
			{
				var steps = new List<TestStepInfo>();
				steps.AddRange(beforeEachSteps);
				steps.Add(testStep);
				steps.AddRange(afterEachSteps);
				executions.Add(new TestExecutionGroup(testStep.Name, steps));
			}

			executions.Add(new TestExecutionGroup("After All", afterAllSteps));

			return executions;
		}

		public IEnumerator Run(object instance, TestExecutionGroup execution)
		{
			foreach (var step in execution.Steps)
			{
				if (step.MethodInfo.ReturnType == typeof(IEnumerator))
				{
					var enumerator = (IEnumerator) step.MethodInfo.Invoke(instance, new object[] { });
					bool moveNext = true;

					while (moveNext)
					{
						try
						{
							moveNext = enumerator.MoveNext();
						}
						catch (Exception e)
						{
							execution.Result = ExecutionResult.Fail(e.Message);
							MapsExtendedTest.Logger.LogError(e);
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
						step.MethodInfo.Invoke(instance, new object[] { });
					}
					catch (TargetInvocationException e)
					{
						execution.Result = ExecutionResult.Fail(e.GetBaseException().Message);
						yield break;
					}
				}
			}

			execution.Result = ExecutionResult.Pass();
		}

		private TestStepInfo[] FindSteps<T>(Type testClassType) where T : Attribute
		{
			return testClassType
				.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(m => m.GetCustomAttributes<T>().Count() > 0)
				.Select(m => new TestStepInfo(m.Name, testClassType, m))
				.ToArray();
		}
	}
}

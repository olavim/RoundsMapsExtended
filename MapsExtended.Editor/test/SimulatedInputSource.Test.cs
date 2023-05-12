using FluentAssertions;
using System.Collections;
using UnityEngine;
using Surity;

namespace MapsExt.Editor.Tests
{
	class MouseDownLifeCounter : MonoBehaviour
	{
		public IInputSource inputSource;
		public int numFramesActive = 0;

		protected virtual void Update()
		{
			if (this.inputSource?.GetMouseButtonDown(0) ?? false)
			{
				this.numFramesActive++;
			}
		}
	}

	[TestClass]
	internal class SimulatedInputSourceTests
	{
		private GameObject _container;

		[BeforeAll]
		private void CreateContainer()
		{
			this._container = new GameObject("SimulatedInputSourceTests");
			GameObject.DontDestroyOnLoad(this._container);
		}

		[AfterAll]
		private void DestroyContainer()
		{
			GameObject.Destroy(this._container);
		}

		[Test]
		private IEnumerator Test_MouseDown_ActiveForOneFrame()
		{
			var input = this._container.AddComponent<SimulatedInputSource>();
			var counter = this._container.AddComponent<MouseDownLifeCounter>();

			counter.inputSource = input;
			input.SetMouseButtonDown(0);

			counter.numFramesActive.Should().Be(0);
			input.GetMouseButtonDown(0).Should().BeTrue();
			yield return null;
			counter.numFramesActive.Should().Be(1);
			input.GetMouseButtonDown(0).Should().BeTrue();
			yield return null;
			counter.numFramesActive.Should().Be(1);
			input.GetMouseButtonDown(0).Should().BeFalse();

			GameObject.Destroy(input);
			GameObject.Destroy(counter);
		}
	}
}

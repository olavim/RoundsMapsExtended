using FluentAssertions;
using System.Collections;
using UnityEngine;

namespace MapsExt.Test
{
	class MouseDownLifeCounter : MonoBehaviour
	{
		public Editor.IInputSource inputSource;
		public int numFramesActive = 0;

		public void Update()
		{
			if (this.inputSource?.GetMouseButtonDown(0) ?? false)
			{
				this.numFramesActive++;
			}
		}
	}

	public class SimulatedInputSourceTests
	{
		[Test]
		public IEnumerator Test_MouseDown_ActiveForOneFrame()
		{
			var input = MapsExtendedTest.instance.gameObject.AddComponent<SimulatedInputSource>();
			var counter = MapsExtendedTest.instance.gameObject.AddComponent<MouseDownLifeCounter>();

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

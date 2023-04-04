using System.Collections;
using FluentAssertions;
using MapsExt.MapObjects;
using Surity;

namespace MapsExt.Editor.Tests
{
	[TestClass]
	internal class InspectorTests : EditorTestBase
	{
		[Test]
		public IEnumerator Test_EditorActiveObjectIsInspectorTarget()
		{
			yield return this.Utils.SpawnMapObject<BoxData>();
			this.Editor.ActiveObject.Should().BeSameAs(this.Inspector.Target);
		}
	}
}

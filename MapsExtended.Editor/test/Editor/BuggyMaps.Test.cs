using System.Collections;
using Surity;
using MapsExt.Properties;
using MapsExt.Compatibility;

namespace MapsExt.Editor.Tests
{
	[TestClass]
	internal class BuggyMapsTests : EditorTestBase
	{
		[Test]
		public IEnumerator Test_DuplicateIdObjects_History()
		{
			this.Editor.LoadMap(MapLoader.LoadResource("MapsExt.Editor.Fixtures.1.2.7.duplicateids.map"));
			yield return null;

			this.Editor.AddSelected(this.Editor.Content.transform.GetChild(0).gameObject);
			yield return this.Utils.MoveSelectedWithMouse(new PositionProperty(0, 1));

			this.Editor.ClearSelected();
			this.Editor.AddSelected(this.Editor.Content.transform.GetChild(1).gameObject);
			yield return this.Utils.MoveSelectedWithMouse(new PositionProperty(0, 1));

			this.Editor.Undo();
			this.Editor.Undo();
		}
	}
}

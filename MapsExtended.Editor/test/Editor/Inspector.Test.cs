using System.Collections;
using FluentAssertions;
using MapsExt.Editor.ActionHandlers;
using MapsExt.Editor.MapObjects;
using MapsExt.Editor.Properties;
using MapsExt.MapObjects;
using MapsExt.Properties;
using Surity;
using UnityEngine;

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

		[Test]
		public IEnumerator Test_Position()
		{
			yield return this.Utils.SpawnMapObject<BoxData>();

			var element = (PositionElement) this.Inspector.GetElement<PositionProperty>();

			var val1 = new PositionProperty(0, 0);
			var val2 = new PositionProperty(1, 1);

			element.Value.Should().Be(val1.Value);
			this.Inspector.Target.GetHandlerValue<PositionProperty>().Should().Be(val1);

			element.Value = val2.Value;

			element.Value.Should().Be(val2.Value);
			this.Inspector.Target.GetHandlerValue<PositionProperty>().Should().Be(val2);
		}

		[Test]
		public IEnumerator Test_Scale()
		{
			yield return this.Utils.SpawnMapObject<BoxData>();

			var element = (ScaleElement) this.Inspector.GetElement<ScaleProperty>();

			var val1 = new ScaleProperty(2, 2);
			var val2 = new ScaleProperty(3, 3);

			element.Value.Should().Be(val1.Value);
			this.Inspector.Target.GetHandlerValue<ScaleProperty>().Should().Be(val1);

			element.Value = val2.Value;

			element.Value.Should().Be(val2.Value);
			this.Inspector.Target.GetHandlerValue<ScaleProperty>().Should().Be(val2);
		}

		[Test]
		public IEnumerator Test_Rotation()
		{
			yield return this.Utils.SpawnMapObject<BoxData>();

			var element = (RotationElement) this.Inspector.GetElement<RotationProperty>();

			var val1 = new RotationProperty(Quaternion.identity);
			var val2 = new RotationProperty(45);

			element.Value.Should().Be(val1.Value);
			this.Inspector.Target.GetHandlerValue<RotationProperty>().Should().Be(val1);

			element.Value = val2.Value;

			element.Value.Should().Be(val2.Value);
			this.Inspector.Target.GetHandlerValue<RotationProperty>().Should().Be(val2);
		}

		[Test]
		public IEnumerator Test_Damageable()
		{
			yield return this.Utils.SpawnMapObject<BoxDestructibleData>();

			var element = (DamageableElement) this.Inspector.GetElement<DamageableProperty>();

			var val1 = new DamageableProperty(true);
			var val2 = new DamageableProperty(false);

			element.Value.Should().Be(val1.Value);
			this.Inspector.Target.GetComponent<DamageableMapObjectInstance>().damageableByEnvironment.Should().Be(val1);

			element.Value = val2.Value;

			element.Value.Should().Be(val2.Value);
			this.Inspector.Target.GetComponent<DamageableMapObjectInstance>().damageableByEnvironment.Should().Be(val2);
		}

		[Test]
		public IEnumerator Test_Rope()
		{
			yield return this.Utils.SpawnMapObject<RopeData>();

			this.Inspector.Target.Should().BeSameAs(this.Editor.SelectedObjects[0].GetComponentInParent<MapObjectInstance>().gameObject);

			var element = (RopePositionElement) this.Inspector.GetElement<RopePositionProperty>();

			var val1 = new RopePositionProperty(new(0, 1), new(0, -1));
			var val2 = new RopePositionProperty(new(0, 2), new(0, -2));

			element.AnchorPosition1.Should().Be(val1.StartPosition);
			element.AnchorPosition2.Should().Be(val1.EndPosition);
			this.Inspector.Target.GetComponent<EditorRope.RopeInstance>().GetAnchor(0).GetAnchoredPosition().Should().Be(val1.StartPosition);
			this.Inspector.Target.GetComponent<EditorRope.RopeInstance>().GetAnchor(1).GetAnchoredPosition().Should().Be(val1.EndPosition);

			element.AnchorPosition1 = val2.StartPosition;
			element.AnchorPosition2 = val2.EndPosition;

			element.AnchorPosition1.Should().Be(val2.StartPosition);
			element.AnchorPosition2.Should().Be(val2.EndPosition);
			this.Inspector.Target.GetComponent<EditorRope.RopeInstance>().GetAnchor(0).GetAnchoredPosition().Should().Be(val2.StartPosition);
			this.Inspector.Target.GetComponent<EditorRope.RopeInstance>().GetAnchor(1).GetAnchoredPosition().Should().Be(val2.EndPosition);
		}
	}
}

using System.Collections;
using UnityEngine;
using Surity;
using MapsExt.Editor.UI;

namespace MapsExt.Editor.Tests
{
	internal abstract class EditorTestBase
	{
		protected MapEditor Editor { get; private set; }
		protected MapEditorUI EditorUI { get; private set; }
		protected MapObjectInspector Inspector { get; private set; }
		protected SimulatedInputSource InputSource { get; private set; }
		protected EditorTestUtils Utils { get; private set; }
		protected MapEditorAnimationHandler AnimationHandler { get; private set; }

		[BeforeAll]
		public void SetInputSource()
		{
			var rootGo = MapsExtendedEditor.instance.gameObject;
			this.InputSource = rootGo.AddComponent<SimulatedInputSource>();
			EditorInput.SetInputSource(this.InputSource);
		}

		[AfterAll]
		public void ResetInputSource()
		{
			GameObject.Destroy(this.InputSource);
			this.InputSource = null;
			EditorInput.SetInputSource(EditorInput.DefaultInputSource);
		}

		[AfterAll]
		[BeforeEach]
		public IEnumerator OpenEditor()
		{
			yield return MapsExtendedEditor.instance.OpenEditorCoroutine();
			var rootGo = GameObject.Find("/Map");
			this.Editor = rootGo.GetComponentInChildren<MapEditor>();
			this.EditorUI = rootGo.GetComponentInChildren<MapEditorUI>();
			this.Utils = new EditorTestUtils(this.Editor, this.InputSource);
			this.AnimationHandler = this.Editor.AnimationHandler;
			this.Inspector = this.EditorUI.Inspector;
		}

		[AfterEach]
		public IEnumerator CloseEditor()
		{
			yield return MapsExtendedEditor.instance.CloseEditorCoroutine();
		}
	}
}

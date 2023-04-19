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

		private GameObject _content;

		[BeforeAll]
		public void SetInputSource()
		{
			this._content = new GameObject("EditorTestBaseContent");
			this.InputSource = this._content.AddComponent<SimulatedInputSource>();
			EditorInput.SetInputSource(this.InputSource);
		}

		[BeforeAll]
		public IEnumerator OpenEditor()
		{
			yield return MapsExtendedEditor.OpenEditorCoroutine();
			var rootGo = GameObject.Find("/Map");
			this.Editor = rootGo.GetComponentInChildren<MapEditor>();
			this.EditorUI = rootGo.GetComponentInChildren<MapEditorUI>();
			this.Utils = new EditorTestUtils(this.Editor, this.InputSource);
			this.AnimationHandler = this.Editor.AnimationHandler;
			this.Inspector = this.EditorUI.Inspector;
		}

		[AfterAll]
		public void ResetInputSource()
		{
			EditorInput.SetInputSource(EditorInput.DefaultInputSource);
			this.InputSource = null;
			GameObject.Destroy(this._content);
		}

		[AfterAll]
		public IEnumerator CloseEditor()
		{
			yield return MapsExtendedEditor.CloseEditorCoroutine();
		}

		[AfterEach]
		public void Cleanup()
		{
			this.Editor.AnimationHandler.SetAnimation(null);
			this.Editor.SelectAll();
			this.Editor.OnDeleteSelectedMapObjects();
			this.Editor.ClearHistory();
		}
	}
}

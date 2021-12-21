using UnityEngine;
using MapsExt.Editor.UI;

namespace MapsExt.Editor.MapObjects
{
	public abstract class InspectorSpec : MonoBehaviour
	{
		public abstract void OnInspectorLayout(InspectorLayoutBuilder builder, MapEditor editor, MapEditorUI editorUI);
	}
}

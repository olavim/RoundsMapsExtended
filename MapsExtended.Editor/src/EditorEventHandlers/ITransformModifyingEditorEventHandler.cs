using System;

namespace MapsExt.Editor.Events
{
	public delegate void TransformChangedEventHandler();

	public interface ITransformModifyingEditorEventHandler
	{
		event TransformChangedEventHandler OnTransformChanged;
	}
}

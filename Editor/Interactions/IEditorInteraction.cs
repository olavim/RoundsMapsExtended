namespace MapsExt.Editor.Interactions
{
	public interface IEditorInteraction
	{
		/// <summary>
		/// Called when the mouse is pressed on selected map objects
		/// </summary>
		void OnPointerDown();

		/// <summary>
		/// Called when the mouse is released from selected map objects
		/// </summary>
		void OnPointerUp();
	}
}

using System;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// Force Odin to draw this value as an IMGUI-embedded UI Toolkit Visual Element.
	/// </summary>
	public class DrawWithVisualElementsAttribute : Attribute
	{
		public bool DrawCollectionWithImGUI;
	}
}

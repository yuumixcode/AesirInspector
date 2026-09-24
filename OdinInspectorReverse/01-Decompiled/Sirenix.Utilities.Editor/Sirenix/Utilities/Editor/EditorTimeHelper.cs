using System;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// A utility class for getting delta time for the GUI editor.
	/// </summary>
	[Obsolete("Use GUITimeHelper.LayoutDeltaTime or GUITimeHelper.RepaintDeltaTime instead depending on which event you're tracking delta time in.", false)]
	public class EditorTimeHelper
	{
		public static readonly EditorTimeHelper Time = new EditorTimeHelper();

		public float DeltaTime => GUITimeHelper.LayoutDeltaTime;

		public void Update()
		{
		}
	}
}

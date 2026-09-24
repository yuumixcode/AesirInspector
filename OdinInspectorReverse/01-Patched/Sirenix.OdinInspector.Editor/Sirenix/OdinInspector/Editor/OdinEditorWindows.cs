using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor
{
	internal static class OdinEditorWindows
	{
		public static List<OdinEditorWindow> ActiveWindows = new List<OdinEditorWindow>(32);

		public static void SetActive(OdinEditorWindow window)
		{
			if (!(window == null) && !ActiveWindows.Contains(window))
			{
				ActiveWindows.Add(window);
			}
		}

		public static void SetInactive(OdinEditorWindow window)
		{
			ActiveWindows.Remove(window);
		}
	}
}

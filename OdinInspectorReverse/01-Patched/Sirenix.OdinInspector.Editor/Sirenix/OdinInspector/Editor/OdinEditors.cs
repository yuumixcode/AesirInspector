using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor
{
	internal static class OdinEditors
	{
		public static List<OdinEditor> ActiveEditors = new List<OdinEditor>(32);

		public static void SetActive(OdinEditor editor)
		{
			if (!(editor == null) && !ActiveEditors.Contains(editor))
			{
				ActiveEditors.Add(editor);
			}
		}

		public static void SetInactive(OdinEditor editor)
		{
			ActiveEditors.Remove(editor);
		}
	}
}

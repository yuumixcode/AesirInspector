using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	[InitializeOnLoad]
	internal static class DesignerTransientWindowHandler
	{
		static DesignerTransientWindowHandler()
		{
			AssemblyReloadEvents.beforeAssemblyReload += CloseAllTransientWindows;
		}

		private static void CloseAllTransientWindows()
		{
			DesignerAttributePopup[] attributePopups = Resources.FindObjectsOfTypeAll<DesignerAttributePopup>();
			DesignerAttributeExampleWindow[] exampleWindows = Resources.FindObjectsOfTypeAll<DesignerAttributeExampleWindow>();
			for (int i = 0; i < attributePopups.Length; i++)
			{
				attributePopups[i].Close();
			}
			for (int j = 0; j < exampleWindows.Length; j++)
			{
				exampleWindows[j].Close();
			}
		}
	}
}

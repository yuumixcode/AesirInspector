using Sirenix.Utilities;
using UnityEditor.Callbacks;

namespace Sirenix.OdinInspector.Editor
{
	internal static class CustomEditorLoader
	{
		[DidReloadScripts]
		private static void LoadCustomEditors()
		{
			GlobalConfig<InspectorConfig>.Instance.UpdateOdinEditors();
		}
	}
}

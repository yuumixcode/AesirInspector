using System;
using UnityEditor;

namespace Sirenix.OdinValidator.Editor
{
	public class AutomationConfigFallbackPostProcessor : AssetPostprocessor
	{
		public static Action InvokeOnNextPostProcessSomeAsset;

		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			if (InvokeOnNextPostProcessSomeAsset != null)
			{
				Action toInvoke = InvokeOnNextPostProcessSomeAsset;
				InvokeOnNextPostProcessSomeAsset = null;
				toInvoke();
			}
		}
	}
}

using System;
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor.Modules
{
	public class UnityLocalizationModuleDefinition : ModuleDefinition
	{
		public override string ID => "Unity.Localization";

		public override string NiceName => "Unity.Localization support";

		public override Version LatestVersion => new Version(2, 0, 0, 9);

		public override Version RequiresOdinVersion => new Version(4, 0, 2, 2);

		public override string Description => "This massive module contains a total overhaul of and new workflow for the Unity.Localization package.";

		public override string DependenciesDescription => "com.unity.localization package v1.1.0 or above";

		public override string BuildFromPath => "Assets/Plugins/Sirenix/Odin Inspector/Modules/Unity.Localization/";

		public override bool CheckSupportsCurrentEnvironment()
		{
			return UnityPackageUtility.HasPackageInstalled("com.unity.localization", new Version(1, 1, 0));
		}
	}
}

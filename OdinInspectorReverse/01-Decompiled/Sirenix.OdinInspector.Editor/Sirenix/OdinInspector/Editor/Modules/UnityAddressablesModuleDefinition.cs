using System;
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor.Modules
{
	public class UnityAddressablesModuleDefinition : ModuleDefinition
	{
		public override string ID => "Unity.Addressables";

		public override string NiceName => "Unity.Addressables support";

		public override Version LatestVersion => new Version(1, 1, 0, 13);

		public override Version RequiresOdinVersion => new Version(4, 0, 2, 2);

		public override string Description => "This small module contains a set of custom drawer and validator implementations to bring Odin support to Unity.Addressables in the inspector.";

		public override string DependenciesDescription => "com.unity.addressables package v1.20+";

		public override string BuildFromPath => "Assets/Plugins/Sirenix/Odin Inspector/Modules/Unity.Addressables/";

		public override bool UnstableExperimental => false;

		public override bool CheckSupportsCurrentEnvironment()
		{
			return UnityPackageUtility.HasPackageInstalled("com.unity.addressables", new Version(1, 20));
		}
	}
}

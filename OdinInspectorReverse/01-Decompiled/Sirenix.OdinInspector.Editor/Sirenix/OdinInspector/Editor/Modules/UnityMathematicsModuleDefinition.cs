using System;
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor.Modules
{
	public class UnityMathematicsModuleDefinition : ModuleDefinition
	{
		public override string ID => "Unity.Mathematics";

		public override string NiceName => "Unity.Mathematics support";

		public override Version LatestVersion => new Version(1, 0, 1, 0);

		public override string Description => "This small module contains a set of custom drawers to improve the performance, look and functionality of drawing Unity.Mathematics structs in the inspector.";

		public override string DependenciesDescription => "com.unity.mathematics package v1.0+";

		public override string BuildFromPath => "../Sirenix Solution/Sirenix.OdinInspector.SmallModules/Packages/com.unity.mathematics/";

		public override bool CheckSupportsCurrentEnvironment()
		{
			return UnityPackageUtility.HasPackageInstalled("com.unity.mathematics", new Version(1, 0));
		}
	}
}

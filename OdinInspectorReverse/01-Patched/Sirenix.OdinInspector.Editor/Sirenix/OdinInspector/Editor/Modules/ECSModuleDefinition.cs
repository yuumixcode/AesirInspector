using System;
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor.Modules
{
	public class ECSModuleDefinition : ModuleDefinition
	{
		public override string ID => "Unity.Entities";

		public override string BuildFromPath => "../Sirenix Solution/Sirenix.OdinInspector.SmallModules/Packages/com.unity.entities/";

		public override Version LatestVersion => new Version(1, 0, 1, 0);

		public override bool UnstableExperimental => true;

		public override string NiceName => "Unity.Entities support";

		public override string Description => "This module adds an Entity Component System inspector integration to Odin.\r\n\r\nPLEASE NOTE that since Unity's ECS systems are still unstable and under development, this module is currently considered EXPERIMENTAL, and is *KNOWN* to be unstable, particularly in cases where entities are added/removed every frame.\r\n\r\nPlease report issues with (along with reproduction projects) at https://bitbucket.org/sirenix/odin-inspector/issues";

		public override string DependenciesDescription => "com.unity.entities package v0.1.1+";

		public override bool CheckSupportsCurrentEnvironment()
		{
			return UnityPackageUtility.HasPackageInstalled("com.unity.entities", new Version(0, 1, 1));
		}
	}
}

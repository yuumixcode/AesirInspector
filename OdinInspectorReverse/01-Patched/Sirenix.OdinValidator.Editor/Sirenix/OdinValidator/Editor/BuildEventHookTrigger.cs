using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Sirenix.OdinValidator.Editor
{
	public class BuildEventHookTrigger : IPreprocessBuildWithReport, IOrderedCallback
	{
		public int callbackOrder => -2000;

		public void OnPreprocessBuild(BuildReport report)
		{
			AutomationConfig.TriggerOnBuild();
		}
	}
}

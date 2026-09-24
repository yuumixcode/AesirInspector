using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Sirenix.Serialization.Internal
{
	public class PreBuildAOTAutomation : IPreprocessBuildWithReport, IOrderedCallback
	{
		public int callbackOrder => -1000;

		public void OnPreprocessBuild(BuildTarget target, string path)
		{
			if (GlobalConfig<AOTGenerationConfig>.Instance.ShouldAutomationGeneration(target))
			{
				GlobalConfig<AOTGenerationConfig>.Instance.ScanProject();
				GlobalConfig<AOTGenerationConfig>.Instance.GenerateDLL();
			}
		}

		public void OnPreprocessBuild(BuildReport report)
		{
			OnPreprocessBuild(report.summary.platform, report.summary.outputPath);
		}
	}
}

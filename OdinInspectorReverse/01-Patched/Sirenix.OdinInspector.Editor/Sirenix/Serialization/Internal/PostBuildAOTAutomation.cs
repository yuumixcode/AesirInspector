using System.IO;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Sirenix.Serialization.Internal
{
	public class PostBuildAOTAutomation : IPostprocessBuildWithReport, IOrderedCallback
	{
		public int callbackOrder => -1000;

		public void OnPostprocessBuild(BuildTarget target, string path)
		{
			if (GlobalConfig<AOTGenerationConfig>.Instance.DeleteDllAfterBuilds && GlobalConfig<AOTGenerationConfig>.Instance.ShouldAutomationGeneration(target))
			{
				if (!AssetDatabase.IsValidFolder(GlobalConfig<AOTGenerationConfig>.Instance.AOTFolderPath))
				{
					Debug.LogError("Attempted to delete the Odin AOT DLL using an invalid folder path: " + GlobalConfig<AOTGenerationConfig>.Instance.AOTFolderPath + ". Please report this issue and include the full error message along with the invalid folder path.");
					return;
				}
				Directory.Delete(GlobalConfig<AOTGenerationConfig>.Instance.AOTFolderPath, recursive: true);
				File.Delete(GlobalConfig<AOTGenerationConfig>.Instance.AOTFolderPath.TrimEnd('/', '\\') + ".meta");
				AssetDatabase.Refresh();
			}
		}

		public void OnPostprocessBuild(BuildReport report)
		{
			OnPostprocessBuild(report.summary.platform, report.summary.outputPath);
		}
	}
}

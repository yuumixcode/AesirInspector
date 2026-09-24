using System.Collections.Generic;
using System.IO;
using Sirenix.Serialization.Utilities.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Sirenix.OdinInspector.Editor
{
	public class AssemblyImportSettingsAutomation : IPreprocessBuildWithReport, IOrderedCallback
	{
		public int callbackOrder => -1500;

		private static void ConfigureImportSettings()
		{
			if (EditorOnlyModeConfig.Instance.IsEditorOnlyModeEnabled() || !GlobalConfig<ImportSettingsConfig>.Instance.AutomateBeforeBuild || EditorOnlyModeConfig.Instance.IsInSourceCode())
			{
				return;
			}
			string assemblyDir = new DirectoryInfo(SirenixAssetPaths.SirenixAssembliesPath).FullName;
			string projectAssetsPath = Directory.GetCurrentDirectory().TrimEnd('\\', '/');
			bool isPackage = !new DirectoryInfo(projectAssetsPath).HasSubDirectory(new DirectoryInfo(assemblyDir));
			string aotDirPath = assemblyDir + "NoEmitAndNoEditor/";
			string jitDirPath = assemblyDir + "NoEditor/";
			DirectoryInfo aotDir = new DirectoryInfo(aotDirPath);
			DirectoryInfo jitDir = new DirectoryInfo(jitDirPath);
			List<string> aotAssemblies = new List<string>();
			List<string> jitAssemblies = new List<string>();
			FileInfo[] files = aotDir.GetFiles("*.dll");
			foreach (FileInfo file in files)
			{
				string path = file.FullName;
				path = ((!isPackage) ? path.Substring(projectAssetsPath.Length + 1) : (SirenixAssetPaths.SirenixAssembliesPath.TrimEnd('\\', '/') + "/" + path.Substring(assemblyDir.Length)));
				aotAssemblies.Add(path);
			}
			FileInfo[] files2 = jitDir.GetFiles("*.dll");
			foreach (FileInfo file2 in files2)
			{
				string path2 = file2.FullName;
				path2 = ((!isPackage) ? path2.Substring(projectAssetsPath.Length + 1) : (SirenixAssetPaths.SirenixAssembliesPath.TrimEnd('\\', '/') + "/" + path2.Substring(assemblyDir.Length)));
				jitAssemblies.Add(path2);
			}
			AssetDatabase.StartAssetEditing();
			try
			{
				BuildTarget platform = EditorUserBuildSettings.activeBuildTarget;
				if (AssemblyImportSettingsUtilities.IsJITSupported(platform, AssemblyImportSettingsUtilities.GetCurrentScriptingBackend(), AssemblyImportSettingsUtilities.GetCurrentApiCompatibilityLevel()))
				{
					ApplyImportSettings(platform, aotAssemblies.ToArray(), OdinAssemblyImportSettings.ExcludeFromAll);
					ApplyImportSettings(platform, jitAssemblies.ToArray(), OdinAssemblyImportSettings.IncludeInBuildOnly);
				}
				else
				{
					ApplyImportSettings(platform, aotAssemblies.ToArray(), OdinAssemblyImportSettings.IncludeInBuildOnly);
					ApplyImportSettings(platform, jitAssemblies.ToArray(), OdinAssemblyImportSettings.ExcludeFromAll);
				}
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}
		}

		private static void ApplyImportSettings(BuildTarget platform, string[] assemblyPaths, OdinAssemblyImportSettings importSettings)
		{
			for (int i = 0; i < assemblyPaths.Length; i++)
			{
				AssemblyImportSettingsUtilities.SetAssemblyImportSettings(platform, assemblyPaths[i], importSettings);
			}
		}

		void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
		{
			ConfigureImportSettings();
		}
	}
}

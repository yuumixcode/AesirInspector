using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Sirenix.Serialization.Utilities.Editor
{
	/// <summary>
	/// Utility for correctly setting import on OdinSerializer assemblies based on platform and scripting backend.
	/// </summary>
	public static class AssemblyImportSettingsUtilities
	{
		private static MethodInfo getPropertyIntMethod;

		private static MethodInfo getScriptingBackendMethod;

		private static MethodInfo getApiCompatibilityLevelMethod;

		private static MethodInfo apiCompatibilityLevelProperty;

		/// <summary>
		/// All valid Unity BuildTarget platforms.
		/// </summary>
		public static readonly ImmutableList<BuildTarget> Platforms;

		/// <summary>
		/// All valid Unity BuildTarget platforms that support Just In Time compilation.
		/// </summary>
		public static readonly ImmutableList<BuildTarget> JITPlatforms;

		/// <summary>
		/// All scripting backends that support JIT.
		/// </summary>
		public static readonly ImmutableList<ScriptingImplementation> JITScriptingBackends;

		/// <summary>
		/// All API compatibility levels that support JIT.
		/// </summary>
		public static readonly ImmutableList<ApiCompatibilityLevel> JITApiCompatibilityLevels;

		static AssemblyImportSettingsUtilities()
		{
			getPropertyIntMethod = typeof(PlayerSettings).GetMethod("GetPropertyInt", BindingFlags.Static | BindingFlags.Public, null, new Type[2]
			{
				typeof(string),
				typeof(BuildTargetGroup)
			}, null);
			getScriptingBackendMethod = typeof(PlayerSettings).GetMethod("GetScriptingBackend", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(BuildTargetGroup) }, null);
			getApiCompatibilityLevelMethod = typeof(PlayerSettings).GetMethod("GetApiCompatibilityLevel", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(BuildTargetGroup) }, null);
			PropertyInfo apiLevelProperty = typeof(PlayerSettings).GetProperty("apiCompatibilityLevel", BindingFlags.Static | BindingFlags.Public);
			apiCompatibilityLevelProperty = ((apiLevelProperty != null) ? apiLevelProperty.GetGetMethod() : null);
			Platforms = new ImmutableList<BuildTarget>((from BuildTarget t in Enum.GetValues(typeof(BuildTarget))
				where t >= (BuildTarget)0 && !typeof(BuildTarget).GetMember(t.ToString())[0].IsDefined(typeof(ObsoleteAttribute), inherit: false)
				select t).ToArray());
			JITPlatforms = new ImmutableList<BuildTarget>(Platforms.Where((BuildTarget i) => i.ToString().StartsWith("StandaloneOSX")).Append(new BuildTarget[4]
			{
				BuildTarget.StandaloneWindows,
				BuildTarget.StandaloneWindows64,
				BuildTarget.StandaloneLinux64,
				BuildTarget.Android
			}).ToArray());
			JITScriptingBackends = new ImmutableList<ScriptingImplementation>(new ScriptingImplementation[1]);
			string[] jitApiNames = new string[5] { "NET_2_0", "NET_2_0_Subset", "NET_4_6", "NET_Web", "NET_Micro" };
			string[] apiLevelNames = Enum.GetNames(typeof(ApiCompatibilityLevel));
			JITApiCompatibilityLevels = new ImmutableList<ApiCompatibilityLevel>((from x in jitApiNames
				where apiLevelNames.Contains(x)
				select (ApiCompatibilityLevel)Enum.Parse(typeof(ApiCompatibilityLevel), x)).ToArray());
		}

		/// <summary>
		/// Set the import settings on the assembly.
		/// </summary>
		/// <param name="assemblyFilePath">The path to the assembly to configure import settings from.</param>
		/// <param name="importSettings">The import settings to configure for the assembly at the path.</param>
		public static void SetAssemblyImportSettings(BuildTarget platform, string assemblyFilePath, OdinAssemblyImportSettings importSettings)
		{
			bool includeInBuild = false;
			bool includeInEditor = false;
			switch (importSettings)
			{
			case OdinAssemblyImportSettings.IncludeInAll:
				includeInBuild = true;
				includeInEditor = true;
				break;
			case OdinAssemblyImportSettings.IncludeInBuildOnly:
				includeInBuild = true;
				break;
			case OdinAssemblyImportSettings.IncludeInEditorOnly:
				includeInEditor = true;
				break;
			}
			SetAssemblyImportSettings(platform, assemblyFilePath, includeInBuild, includeInEditor);
		}

		/// <summary>
		/// Set the import settings on the assembly.
		/// </summary>
		/// <param name="assemblyFilePath">The path to the assembly to configure import settings from.</param>
		/// <param name="includeInBuild">Indicates if the assembly should be included in the build.</param>
		/// <param name="includeInEditor">Indicates if the assembly should be included in the Unity editor.</param>
		public static void SetAssemblyImportSettings(BuildTarget platform, string assemblyFilePath, bool includeInBuild, bool includeInEditor)
		{
			if (!File.Exists(assemblyFilePath))
			{
				throw new FileNotFoundException(assemblyFilePath);
			}
			PluginImporter importer = (PluginImporter)AssetImporter.GetAtPath(assemblyFilePath);
			if (importer == null)
			{
				throw new InvalidOperationException("Failed to get PluginImporter for " + assemblyFilePath);
			}
			if (importer.GetCompatibleWithAnyPlatform() || importer.GetCompatibleWithPlatform(platform) != includeInBuild || importer.GetCompatibleWithEditor() != includeInEditor)
			{
				importer.SetCompatibleWithAnyPlatform(enable: false);
				importer.SetCompatibleWithPlatform(platform, includeInBuild);
				importer.SetCompatibleWithEditor(includeInEditor);
				importer.SaveAndReimport();
			}
		}

		/// <summary>
		/// Gets the current scripting backend for the build from the Unity editor. This method is Unity version independent.
		/// </summary>
		/// <returns></returns>
		public static ScriptingImplementation GetCurrentScriptingBackend()
		{
			BuildTargetGroup buildGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
			if (getScriptingBackendMethod != null)
			{
				return (ScriptingImplementation)getScriptingBackendMethod.Invoke(null, new object[1] { buildGroup });
			}
			if (getPropertyIntMethod != null)
			{
				return (ScriptingImplementation)getPropertyIntMethod.Invoke(null, new object[2] { "ScriptingBackend", buildGroup });
			}
			throw new InvalidOperationException("Was unable to get the current scripting backend!");
		}

		/// <summary>
		/// Gets the current API compatibility level from the Unity Editor. This method is Unity version independent.
		/// </summary>
		/// <returns></returns>
		public static ApiCompatibilityLevel GetCurrentApiCompatibilityLevel()
		{
			if (getApiCompatibilityLevelMethod != null)
			{
				BuildTargetGroup buildGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
				return (ApiCompatibilityLevel)getApiCompatibilityLevelMethod.Invoke(null, new object[1] { buildGroup });
			}
			if (apiCompatibilityLevelProperty != null)
			{
				return (ApiCompatibilityLevel)apiCompatibilityLevelProperty.Invoke(null, null);
			}
			throw new InvalidOperationException("Was unable to get the current api compatibility level!");
		}

		/// <summary>
		/// Gets a value that indicates if the specified platform supports JIT.
		/// </summary>
		/// <param name="platform">The platform to test.</param>
		/// <returns><c>true</c> if the platform supports JIT; otherwise <c>false</c>.</returns>
		public static bool PlatformSupportsJIT(BuildTarget platform)
		{
			return JITPlatforms.Contains(platform);
		}

		/// <summary>
		/// Gets a value that indicates if the specified scripting backend supports JIT.
		/// </summary>
		/// <param name="backend">The backend to test.</param>
		/// <returns><c>true</c> if the backend supports JIT; otherwise <c>false</c>.</returns>
		public static bool ScriptingBackendSupportsJIT(ScriptingImplementation backend)
		{
			return JITScriptingBackends.Contains(backend);
		}

		/// <summary>
		/// Gets a value that indicates if the specified api level supports JIT.
		/// </summary>
		/// <param name="apiLevel">The api level to test.</param>
		/// <returns><c>true</c> if the api level supports JIT; otherwise <c>false</c>.</returns>
		public static bool ApiCompatibilityLevelSupportsJIT(ApiCompatibilityLevel apiLevel)
		{
			return JITApiCompatibilityLevels.Contains(apiLevel);
		}

		/// <summary>
		/// Gets a value that indicates if the specified build settings supports JIT.
		/// </summary>
		/// <param name="platform">The platform build setting.</param>
		/// <param name="backend">The scripting backend build settting.</param>
		/// <param name="apiLevel">The api level build setting.</param>
		/// <returns><c>true</c> if the build settings supports JIT; otherwise <c>false</c>.</returns>
		public static bool IsJITSupported(BuildTarget platform, ScriptingImplementation backend, ApiCompatibilityLevel apiLevel)
		{
			if (PlatformSupportsJIT(platform) && ScriptingBackendSupportsJIT(backend))
			{
				return ApiCompatibilityLevelSupportsJIT(apiLevel);
			}
			return false;
		}
	}
}

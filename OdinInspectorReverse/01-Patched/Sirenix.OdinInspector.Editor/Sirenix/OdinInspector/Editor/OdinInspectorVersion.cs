using System;
using System.IO;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Installed Odin Inspector Version Info.
	/// </summary>
	public static class OdinInspectorVersion
	{
		private static string version;

		private static string buildName;

		private static string licensee;

		private static bool isOnDiskVersionInitialized;

		private static Version onDiskVersion;

		/// <summary>
		/// Gets the name of the current running version of Odin Inspector.
		/// </summary>
		public static string BuildName
		{
			get
			{
				if (buildName == null)
				{
					SirenixBuildNameAttribute attribute = typeof(InspectorConfig).Assembly.GetAttribute<SirenixBuildNameAttribute>(inherit: true);
					buildName = ((attribute != null) ? attribute.BuildName : "Source Code");
				}
				return buildName;
			}
		}

		public static bool HasLicensee => !string.IsNullOrEmpty(Licensee);

		public static string Licensee
		{
			get
			{
				if (licensee == null && !BakedValues.TryGetBakedValue<string>("Licensee", out licensee))
				{
					licensee = "";
				}
				return licensee;
			}
		}

		/// <summary>
		/// Gets the current running version of Odin Inspector.
		/// </summary>
		public static string Version
		{
			get
			{
				if (version == null)
				{
					SirenixBuildVersionAttribute attribute = typeof(InspectorConfig).Assembly.GetAttribute<SirenixBuildVersionAttribute>(inherit: true);
					version = ((attribute != null) ? attribute.Version : "Source Code Mode");
				}
				return version;
			}
		}

		/// <summary>
		/// Whether the current version of Odin is an enterprise version.
		/// </summary>
		public static bool IsEnterprise => false;

		/// <summary>
		/// Gets the Odin Inspector version as written in the "Version.txt" file inside the Odin Inspector directory.
		/// Note that this is the version on disk, which is not necessarily the same as the runtime version determined from the loaded assembly.
		/// </summary>
		/// <returns>
		/// The version read from Version.txt, or null if the file is missing or its contents can't be parsed.
		/// </returns>
		public static Version GetOnDiskVersion()
		{
			if (!isOnDiskVersionInitialized)
			{
				string versionFile = Path.Combine(SirenixAssetPaths.OdinPath, "Version.txt");
				if (File.Exists(versionFile))
				{
					string versionFileText = File.ReadAllText(versionFile);
					if (System.Version.TryParse(versionFileText, out Version parsedVersion))
					{
						onDiskVersion = parsedVersion;
					}
					else
					{
						onDiskVersion = null;
					}
				}
				isOnDiskVersionInitialized = true;
			}
			return onDiskVersion;
		}
	}
}

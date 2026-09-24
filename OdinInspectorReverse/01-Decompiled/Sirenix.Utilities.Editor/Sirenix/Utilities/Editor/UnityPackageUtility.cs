using System;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	public static class UnityPackageUtility
	{
		[Serializable]
		private struct PackageInfo
		{
			public string name;

			public string version;
		}

		public static bool HasPackageInstalled(string requiredPackage, Version minimumVersion)
		{
			string path = "Packages/" + requiredPackage + "/package.json";
			TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
			if (asset == null)
			{
				return false;
			}
			PackageInfo info = JsonUtility.FromJson<PackageInfo>(asset.text);
			if (info.name != requiredPackage)
			{
				return false;
			}
			int dashIndex = info.version.IndexOf('-');
			if (dashIndex > -1)
			{
				info.version = info.version.Substring(0, dashIndex);
			}
			Version parsedVersion;
			try
			{
				parsedVersion = new Version(info.version);
			}
			catch
			{
				Debug.LogError("Failed to parse package version string '" + info.version + "' into System.Version");
				return false;
			}
			return parsedVersion >= minimumVersion;
		}
	}
}

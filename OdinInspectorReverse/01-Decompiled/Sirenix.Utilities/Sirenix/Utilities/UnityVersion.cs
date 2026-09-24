using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities
{
	/// <summary>
	/// Utility class indicating current Unity version.
	/// </summary>
	[InitializeOnLoad]
	public static class UnityVersion
	{
		/// <summary>
		/// The current Unity version major.
		/// </summary>
		public static readonly int Major;

		/// <summary>
		/// The current Unity version minor.
		/// </summary>
		public static readonly int Minor;

		static UnityVersion()
		{
			string[] version = Application.unityVersion.Split(new char[1] { '.' });
			if (version.Length < 2)
			{
				Debug.LogError("Could not parse current Unity version '" + Application.unityVersion + "'; not enough version elements.");
				return;
			}
			if (!int.TryParse(version[0], out Major))
			{
				Debug.LogError("Could not parse major part '" + version[0] + "' of Unity version '" + Application.unityVersion + "'.");
			}
			if (!int.TryParse(version[1], out Minor))
			{
				Debug.LogError("Could not parse minor part '" + version[1] + "' of Unity version '" + Application.unityVersion + "'.");
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void EnsureLoaded()
		{
		}

		/// <summary>
		/// Tests current Unity version is equal or greater.
		/// </summary>
		/// <param name="major">Minimum major version.</param>
		/// <param name="minor">Minimum minor version.</param>
		/// <returns><c>true</c> if the current Unity version is greater. Otherwise <c>false</c>.</returns>
		public static bool IsVersionOrGreater(int major, int minor)
		{
			if (Major <= major)
			{
				if (Major == major)
				{
					return Minor >= minor;
				}
				return false;
			}
			return true;
		}
	}
}

using System;
using System.Reflection;
using UnityEditor;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>This class fixes Unity's about window, by invoking "UnityEditor.VisualStudioIntegration.UnityVSSupport.GetAboutWindowLabel" before any dynamic assemblies have been defined.</para>
	/// <para>This is because dynamic assemblies in the current AppDomain break that method, and Unity's about window depends on it.</para>
	/// </summary>
	[InitializeOnLoad]
	internal static class FixUnityAboutWindowBeforeEmit
	{
		private static bool isFixed;

		static FixUnityAboutWindowBeforeEmit()
		{
			Fix();
		}

		public static void Fix()
		{
			if (isFixed)
			{
				return;
			}
			isFixed = true;
			Type unityVSSupport = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.VisualStudioIntegration.UnityVSSupport");
			if (!(unityVSSupport != null))
			{
				return;
			}
			MethodInfo getAboutWindowLabel = unityVSSupport.GetMethod("GetAboutWindowLabel", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (getAboutWindowLabel != null)
			{
				try
				{
					getAboutWindowLabel.Invoke(null, null);
				}
				catch
				{
				}
			}
		}
	}
}

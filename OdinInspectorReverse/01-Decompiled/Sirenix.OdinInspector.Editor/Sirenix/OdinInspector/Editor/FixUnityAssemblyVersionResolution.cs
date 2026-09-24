using System;
using System.IO;
using System.Reflection;
using UnityEditor;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>
	/// If you mark any of Unity's assemblies with the [AssemblyVersion] attribute to get a rolling assembly
	/// version that changes sometimes (or all the time), Odin's hardcoded assembly references to user types
	/// will break.
	/// </para>
	/// <para>
	/// To fix this case, and all other cases of references to wrongly versioned Unity types not being resolved,
	/// we overload the app domain's type resolution and resolve Unity user assemblies properly regardless of
	/// version.
	/// </para>
	/// </summary>
	[InitializeOnLoad]
	internal static class FixUnityAssemblyVersionResolution
	{
		static FixUnityAssemblyVersionResolution()
		{
			Fix();
		}

		private static void Fix()
		{
			string[] unityAssemblyPrefixes = new string[12]
			{
				"Assembly-CSharp-Editor-firstpass", "Assembly-CSharp-firstpass", "Assembly-CSharp-Editor", "Assembly-CSharp", "Assembly-UnityScript-Editor-firstpass", "Assembly-UnityScript-firstpass", "Assembly-UnityScript-Editor", "Assembly-UnityScript", "Assembly-Boo-Editor-firstpass", "Assembly-Boo-firstpass",
				"Assembly-Boo-Editor", "Assembly-Boo"
			};
			AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs args)
			{
				string text = args.Name;
				foreach (string value in unityAssemblyPrefixes)
				{
					if (text.StartsWith(value))
					{
						int num = text.IndexOf(',');
						if (num >= 0)
						{
							text = text.Substring(0, num);
						}
						try
						{
							return Assembly.Load(text);
						}
						catch (FileNotFoundException)
						{
							return (Assembly)null;
						}
						catch (ReflectionTypeLoadException)
						{
							return (Assembly)null;
						}
						catch (TypeLoadException)
						{
							return (Assembly)null;
						}
					}
				}
				return (Assembly)null;
			};
		}
	}
}

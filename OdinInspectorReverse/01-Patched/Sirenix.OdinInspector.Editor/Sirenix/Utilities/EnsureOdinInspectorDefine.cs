using System;
using System.Linq;
using UnityEditor;

namespace Sirenix.Utilities
{
	/// <summary>
	/// Defines the ODIN_INSPECTOR symbol.
	/// </summary>
	internal static class EnsureOdinInspectorDefine
	{
		internal static readonly string[] DEFINES = new string[5] { "ODIN_INSPECTOR", "ODIN_INSPECTOR_3", "ODIN_INSPECTOR_3_1", "ODIN_INSPECTOR_3_2", "ODIN_INSPECTOR_3_3" };

		[InitializeOnLoadMethod]
		private static void EnsureScriptingDefineSymbol()
		{
			BuildTargetGroup currentTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
			if (currentTarget == BuildTargetGroup.Unknown)
			{
				return;
			}
			string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentTarget).Trim();
			string[] defines = definesString.Split(new char[1] { ';' });
			bool changed = false;
			string[] dEFINES = DEFINES;
			foreach (string define in dEFINES)
			{
				if (!defines.Contains(define))
				{
					if (!definesString.EndsWith(";", StringComparison.InvariantCulture))
					{
						definesString += ";";
					}
					definesString += define;
					changed = true;
				}
			}
			if (changed)
			{
				PlayerSettings.SetScriptingDefineSymbolsForGroup(currentTarget, definesString);
			}
		}
	}
}

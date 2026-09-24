using System;
using System.Linq;
using UnityEditor;

namespace Sirenix.OdinValidator.Editor
{
	/// <summary>
	/// Defines the ODIN_VALIDATOR symbol.
	/// </summary>
	internal static class EnsureOdinValidatorDefine
	{
		internal static readonly string[] DEFINES = new string[4] { "ODIN_VALIDATOR", "ODIN_VALIDATOR_3_1", "ODIN_VALIDATOR_3_2", "ODIN_VALIDATOR_3_3" };

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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Sirenix.OdinInspector.Editor
{
	internal static class EnsureEditorOnlyModeDefine
	{
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
			string define = "ODIN_INSPECTOR_EDITOR_ONLY";
			if (EditorOnlyModeConfig.Instance.IsEditorOnlyModeEnabled())
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
			else
			{
				for (int i = 0; i < defines.Length; i++)
				{
					if (defines[i] == define)
					{
						List<string> list = defines.ToList();
						list.RemoveAt(i);
						definesString = string.Join(";", list.ToArray());
						changed = true;
						break;
					}
				}
			}
			if (changed)
			{
				PlayerSettings.SetScriptingDefineSymbolsForGroup(currentTarget, definesString);
			}
		}
	}
}

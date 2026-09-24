using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	[CustomEditor(typeof(GlobalValidationConfig))]
	public class GlobalValidationConfigEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), SirenixGUIStyles.BorderColor);
			GUIHelper.PushHierarchyMode(hierarchyMode: false);
			ValidationSessionEditor.DrawConfig();
			GUIHelper.PopHierarchyMode();
		}

		public override bool UseDefaultMargins()
		{
			return false;
		}
	}
}

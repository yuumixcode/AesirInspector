using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalStringArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, string> where TArray : IList
	{
		private static GUIStyle style;

		protected override string DrawElement(Rect rect, string value)
		{
			if (style == null)
			{
				style = new GUIStyle(EditorStyles.textField);
				style.alignment = TextAnchor.MiddleCenter;
			}
			return EditorGUI.TextField(new Rect(rect.x, rect.y, rect.width + 1f, rect.height + 1f), value, style);
		}
	}
}

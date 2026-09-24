using System.Collections;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalBoolArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, bool> where TArray : IList
	{
		protected override bool DrawElement(Rect rect, bool value)
		{
			if (Event.current.type == EventType.Repaint)
			{
				return EditorGUI.Toggle(rect.AlignCenter(16f, 16f), value);
			}
			return EditorGUI.Toggle(rect, value);
		}
	}
}

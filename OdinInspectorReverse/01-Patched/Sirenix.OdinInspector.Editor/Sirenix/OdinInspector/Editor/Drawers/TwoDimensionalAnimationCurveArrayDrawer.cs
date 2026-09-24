using System.Collections;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalAnimationCurveArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, AnimationCurve> where TArray : IList
	{
		protected override AnimationCurve DrawElement(Rect rect, AnimationCurve value)
		{
			if (value == null)
			{
				if (GUI.Button(rect.Padding(2f), "Null - Create Animation Curve", EditorStyles.objectField))
				{
					value = new AnimationCurve();
				}
				return value;
			}
			return EditorGUI.CurveField(rect.Padding(2f), value);
		}
	}
}

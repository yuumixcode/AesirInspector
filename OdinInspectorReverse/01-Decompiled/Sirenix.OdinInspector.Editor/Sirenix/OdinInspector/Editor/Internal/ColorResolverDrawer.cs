using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class ColorResolverDrawer : OdinAttributeDrawer<ColorResolverAttribute, string>
	{
		protected override void DrawPropertyLayout(GUIContent label)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight);
			base.ValueEntry.SmartValue = ColorResolverSelector.Draw(rect, label, base.ValueEntry.SmartValue, base.Property);
		}
	}
}

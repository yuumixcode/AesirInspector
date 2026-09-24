using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class ImageResolverDrawer : OdinAttributeDrawer<ImageResolverAttribute, string>
	{
		protected override void DrawPropertyLayout(GUIContent label)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight);
			string value = ImageResolverSelector.Draw(rect, label, base.ValueEntry.SmartValue);
			value = (string.IsNullOrWhiteSpace(value) ? null : value);
			if (value != base.ValueEntry.SmartValue)
			{
				base.ValueEntry.SmartValue = value;
			}
		}
	}
}

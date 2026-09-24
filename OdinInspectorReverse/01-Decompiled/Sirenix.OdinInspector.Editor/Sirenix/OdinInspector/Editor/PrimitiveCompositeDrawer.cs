using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Property drawer for primitive composite properties.
	/// </summary>
	public abstract class PrimitiveCompositeDrawer<T> : OdinValueDrawer<T>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<T> entry = base.ValueEntry;
			bool conflict = false;
			int childCount = entry.Property.Children.Count;
			for (int i = 0; i < childCount; i++)
			{
				InspectorProperty child = entry.Property.Children[i];
				if (child.ValueEntry != null && child.ValueEntry.ValueState == PropertyValueState.PrimitiveValueConflict)
				{
					conflict = true;
					break;
				}
			}
			if (conflict)
			{
				EditorGUI.showMixedValue = true;
				GUI.changed = false;
			}
			DrawPropertyField(entry, label);
			if (!conflict)
			{
				return;
			}
			EditorGUI.showMixedValue = false;
			if (GUI.changed)
			{
				T value = entry.SmartValue;
				for (int j = 0; j < entry.ValueCount; j++)
				{
					entry.Values[j] = value;
				}
			}
		}

		/// <summary>
		/// Draws the property field.
		/// </summary>
		protected abstract void DrawPropertyField(IPropertyValueEntry<T> entry, GUIContent label);
	}
}

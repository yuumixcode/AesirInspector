using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Vector2Int proprety drawer.
	/// </summary>
	public sealed class Vector2IntDrawer : OdinValueDrawer<Vector2Int>, IDefinesGenericMenuItems
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			Rect labelRect;
			Rect contentRect = SirenixEditorGUI.BeginHorizontalPropertyLayout(label, out labelRect);
			EditorGUI.BeginChangeCheck();
			Vector4 val = SirenixEditorFields.VectorPrefixSlideRect(labelRect, UnityShims.Vector4.op_Implicit(UnityShims.Vector2Int.op_Implicit(base.ValueEntry.SmartValue)));
			if (EditorGUI.EndChangeCheck())
			{
				base.ValueEntry.SmartValue = new Vector2Int((int)val.x, (int)val.y);
			}
			bool showLabels = SirenixEditorFields.ResponsiveVectorComponentFields && contentRect.width >= 185f;
			GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
			base.ValueEntry.Property.Children[0].Draw(showLabels ? GUIHelper.TempContent("X") : null);
			base.ValueEntry.Property.Children[1].Draw(showLabels ? GUIHelper.TempContent("Y") : null);
			GUIHelper.PopLabelWidth();
			SirenixEditorGUI.EndHorizontalPropertyLayout();
		}

		/// <summary>
		/// Populates the generic menu for the property.
		/// </summary>
		public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			Vector2Int value = (Vector2Int)property.ValueEntry.WeakSmartValue;
			if (genericMenu.GetItemCount() > 0)
			{
				genericMenu.AddSeparator("");
			}
			genericMenu.AddItem(new GUIContent("Zero", "Set the vector to (0, 0)"), value == Vector2Int.zero, delegate
			{
				SetVector(property, Vector2Int.zero);
			});
			genericMenu.AddItem(new GUIContent("One", "Set the vector to (1, 1)"), value == Vector2Int.one, delegate
			{
				SetVector(property, Vector2Int.one);
			});
			genericMenu.AddSeparator("");
			genericMenu.AddItem(new GUIContent("Right", "Set the vector to (1, 0)"), value == Vector2Int.right, delegate
			{
				SetVector(property, Vector2Int.right);
			});
			genericMenu.AddItem(new GUIContent("Left", "Set the vector to (-1, 0)"), value == Vector2Int.left, delegate
			{
				SetVector(property, Vector2Int.left);
			});
			genericMenu.AddItem(new GUIContent("Up", "Set the vector to (0, 1)"), value == Vector2Int.up, delegate
			{
				SetVector(property, Vector2Int.up);
			});
			genericMenu.AddItem(new GUIContent("Down", "Set the vector to (0, -1)"), value == Vector2Int.down, delegate
			{
				SetVector(property, Vector2Int.down);
			});
		}

		private void SetVector(InspectorProperty property, Vector2Int value)
		{
			property.Tree.DelayActionUntilRepaint(delegate
			{
				for (int i = 0; i < property.ValueEntry.ValueCount; i++)
				{
					property.ValueEntry.WeakValues[i] = value;
				}
			});
		}
	}
}

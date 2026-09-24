using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Vector3Int property drawer.
	/// </summary>
	public sealed class Vector3IntDrawer : OdinValueDrawer<Vector3Int>, IDefinesGenericMenuItems
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			Rect labelRect;
			Rect contentRect = SirenixEditorGUI.BeginHorizontalPropertyLayout(label, out labelRect);
			EditorGUI.BeginChangeCheck();
			Vector4 val = SirenixEditorFields.VectorPrefixSlideRect(labelRect, UnityShims.Vector4.op_Implicit(UnityShims.Vector3Int.op_Implicit(base.ValueEntry.SmartValue)));
			if (EditorGUI.EndChangeCheck())
			{
				base.ValueEntry.SmartValue = new Vector3Int((int)val.x, (int)val.y, (int)val.z);
			}
			bool showLabels = SirenixEditorFields.ResponsiveVectorComponentFields && contentRect.width >= 185f;
			GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
			base.ValueEntry.Property.Children[0].Draw(showLabels ? GUIHelper.TempContent("X") : null);
			base.ValueEntry.Property.Children[1].Draw(showLabels ? GUIHelper.TempContent("Y") : null);
			base.ValueEntry.Property.Children[2].Draw(showLabels ? GUIHelper.TempContent("Z") : null);
			GUIHelper.PopLabelWidth();
			SirenixEditorGUI.EndHorizontalPropertyLayout();
		}

		/// <summary>
		/// Populates the generic menu for the property.
		/// </summary>
		public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			Vector3Int value = (Vector3Int)property.ValueEntry.WeakSmartValue;
			if (genericMenu.GetItemCount() > 0)
			{
				genericMenu.AddSeparator("");
			}
			genericMenu.AddItem(new GUIContent("Zero", "Set the vector to (0, 0, 0)"), value == Vector3Int.zero, delegate
			{
				SetVector(property, Vector3Int.zero);
			});
			genericMenu.AddItem(new GUIContent("One", "Set the vector to (1, 1, 1)"), value == Vector3Int.one, delegate
			{
				SetVector(property, Vector3Int.one);
			});
			genericMenu.AddSeparator("");
			genericMenu.AddItem(new GUIContent("Right", "Set the vector to (1, 0, 0)"), value == Vector3Int.right, delegate
			{
				SetVector(property, Vector3Int.right);
			});
			genericMenu.AddItem(new GUIContent("Left", "Set the vector to (-1, 0, 0)"), value == Vector3Int.left, delegate
			{
				SetVector(property, Vector3Int.left);
			});
			genericMenu.AddItem(new GUIContent("Up", "Set the vector to (0, 1, 0)"), value == Vector3Int.up, delegate
			{
				SetVector(property, Vector3Int.up);
			});
			genericMenu.AddItem(new GUIContent("Down", "Set the vector to (0, -1, 0)"), value == Vector3Int.down, delegate
			{
				SetVector(property, Vector3Int.down);
			});
			genericMenu.AddItem(new GUIContent("Forward", "Set the vector property to (0, 0, 1)"), value == new Vector3Int(0, 0, 1), delegate
			{
				SetVector(property, new Vector3Int(0, 0, 1));
			});
			genericMenu.AddItem(new GUIContent("Back", "Set the vector property to (0, 0, -1)"), value == new Vector3Int(0, 0, -1), delegate
			{
				SetVector(property, new Vector3Int(0, 0, -1));
			});
		}

		private void SetVector(InspectorProperty property, Vector3Int value)
		{
			property.Tree.DelayActionUntilRepaint(delegate
			{
				property.ValueEntry.WeakSmartValue = value;
			});
		}
	}
}

using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Vector3 property drawer.
	/// </summary>
	public sealed class Vector3Drawer : OdinValueDrawer<Vector3>, IDefinesGenericMenuItems
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			Rect labelRect;
			Rect contentRect = SirenixEditorGUI.BeginHorizontalPropertyLayout(label, out labelRect);
			EditorGUI.BeginChangeCheck();
			Vector4 val = SirenixEditorFields.VectorPrefixSlideRect(labelRect, UnityShims.Vector4.op_Implicit(base.ValueEntry.SmartValue));
			if (EditorGUI.EndChangeCheck())
			{
				base.ValueEntry.SmartValue = UnityShims.Vector4.op_ImplicitVec3(val);
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
			Vector3 value = (Vector3)property.ValueEntry.WeakSmartValue;
			if (genericMenu.GetItemCount() > 0)
			{
				genericMenu.AddSeparator("");
			}
			genericMenu.AddItem(new GUIContent("Normalize"), Mathf.Approximately(value.magnitude, 1f), delegate
			{
				NormalizeEntries(property);
			});
			genericMenu.AddItem(new GUIContent("Zero", "Set the vector to (0, 0, 0)"), value == Vector3.zero, delegate
			{
				SetVector(property, Vector3.zero);
			});
			genericMenu.AddItem(new GUIContent("One", "Set the vector to (1, 1, 1)"), value == Vector3.one, delegate
			{
				SetVector(property, Vector3.one);
			});
			genericMenu.AddSeparator("");
			genericMenu.AddItem(new GUIContent("Right", "Set the vector to (1, 0, 0)"), value == Vector3.right, delegate
			{
				SetVector(property, Vector3.right);
			});
			genericMenu.AddItem(new GUIContent("Left", "Set the vector to (-1, 0, 0)"), value == Vector3.left, delegate
			{
				SetVector(property, Vector3.left);
			});
			genericMenu.AddItem(new GUIContent("Up", "Set the vector to (0, 1, 0)"), value == Vector3.up, delegate
			{
				SetVector(property, Vector3.up);
			});
			genericMenu.AddItem(new GUIContent("Down", "Set the vector to (0, -1, 0)"), value == Vector3.down, delegate
			{
				SetVector(property, Vector3.down);
			});
			genericMenu.AddItem(new GUIContent("Forward", "Set the vector property to (0, 0, 1)"), value == Vector3.forward, delegate
			{
				SetVector(property, Vector3.forward);
			});
			genericMenu.AddItem(new GUIContent("Back", "Set the vector property to (0, 0, -1)"), value == Vector3.back, delegate
			{
				SetVector(property, Vector3.back);
			});
		}

		private void SetVector(InspectorProperty property, Vector3 value)
		{
			property.Tree.DelayActionUntilRepaint(delegate
			{
				property.ValueEntry.WeakSmartValue = value;
			});
		}

		private void NormalizeEntries(InspectorProperty property)
		{
			property.Tree.DelayActionUntilRepaint(delegate
			{
				property.ValueEntry.WeakSmartValue = ((Vector3)property.ValueEntry.WeakSmartValue).normalized;
			});
		}
	}
}

using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Quaternion property drawer.
	/// </summary>
	public sealed class QuaternionDrawer : OdinValueDrawer<Quaternion>, IDefinesGenericMenuItems
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<Quaternion> entry = base.ValueEntry;
			entry.SmartValue = SirenixEditorFields.RotationField(label, entry.SmartValue, GlobalConfig<GeneralDrawerConfig>.Instance.QuaternionDrawMode);
		}

		/// <summary>
		/// Populates the generic menu for the property.
		/// </summary>
		public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			if (property.ValueEntry.WeakSmartValue == null)
			{
				return;
			}
			Quaternion value = (Quaternion)property.ValueEntry.WeakSmartValue;
			value.ToAngleAxis(out var angle, out var axis);
			genericMenu.AddSeparator("");
			QuaternionDrawMode drawMode = GlobalConfig<GeneralDrawerConfig>.Instance.QuaternionDrawMode;
			genericMenu.AddItem(new GUIContent("Euler"), drawMode == QuaternionDrawMode.Eulers, delegate
			{
				SetDrawMode(property, QuaternionDrawMode.Eulers);
			});
			genericMenu.AddItem(new GUIContent("Angle axis"), drawMode == QuaternionDrawMode.AngleAxis, delegate
			{
				SetDrawMode(property, QuaternionDrawMode.AngleAxis);
			});
			genericMenu.AddItem(new GUIContent("Raw"), drawMode == QuaternionDrawMode.Raw, delegate
			{
				SetDrawMode(property, QuaternionDrawMode.Raw);
			});
			genericMenu.AddSeparator("");
			genericMenu.AddItem(new GUIContent("Zero"), value == Quaternion.identity, delegate
			{
				for (int i = 0; i < property.ValueEntry.ValueCount; i++)
				{
					property.ValueEntry.WeakValues[i] = Quaternion.identity;
				}
			});
			genericMenu.AddSeparator("");
			genericMenu.AddItem(new GUIContent("Right", "Set the axis to (1, 0, 0)"), axis == Vector3.right && angle != 0f, delegate
			{
				SetAxis(property, Vector3.right);
			});
			genericMenu.AddItem(new GUIContent("Left", "Set the axis to (-1, 0, 0)"), axis == Vector3.left, delegate
			{
				SetAxis(property, Vector3.left);
			});
			genericMenu.AddItem(new GUIContent("Up", "Set the axis to (0, 1, 0)"), axis == Vector3.up, delegate
			{
				SetAxis(property, Vector3.up);
			});
			genericMenu.AddItem(new GUIContent("Down", "Set the axis to (0, -1, 0)"), axis == Vector3.down, delegate
			{
				SetAxis(property, Vector3.down);
			});
			genericMenu.AddItem(new GUIContent("Forward", "Set the axis property to (0, 0, 1)"), axis == Vector3.forward, delegate
			{
				SetAxis(property, Vector3.forward);
			});
			genericMenu.AddItem(new GUIContent("Back", "Set the axis property to (0, 0, -1)"), axis == Vector3.back, delegate
			{
				SetAxis(property, Vector3.back);
			});
		}

		private void SetAxis(InspectorProperty property, Vector3 axis)
		{
			property.Tree.DelayActionUntilRepaint(delegate
			{
				((Quaternion)property.ValueEntry.WeakSmartValue).ToAngleAxis(out var angle, out var _);
				Quaternion quaternion = Quaternion.AngleAxis(angle, axis);
				for (int i = 0; i < property.ValueEntry.ValueCount; i++)
				{
					property.ValueEntry.WeakValues[i] = quaternion;
				}
			});
		}

		private void SetDrawMode(InspectorProperty property, QuaternionDrawMode mode)
		{
			if (GlobalConfig<GeneralDrawerConfig>.Instance.QuaternionDrawMode != mode)
			{
				GlobalConfig<GeneralDrawerConfig>.Instance.QuaternionDrawMode = mode;
			}
		}
	}
}

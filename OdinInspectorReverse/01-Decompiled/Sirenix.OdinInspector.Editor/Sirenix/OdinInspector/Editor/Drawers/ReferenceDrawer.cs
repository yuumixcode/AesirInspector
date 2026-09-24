using System;
using System.Reflection;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws all reference type properties, which has already been drawn elsewhere. This drawer adds an additional foldout to prevent infinite draw depth.
	/// </summary>
	[DrawerPriority(90.0, 0.0, 0.0)]
	[AllowGUIEnabledForReadonly]
	public sealed class ReferenceDrawer<T> : OdinValueDrawer<T> where T : class
	{
		private LocalPersistentContext<bool> isToggled;

		private InspectorProperty referencedProperty;

		private bool hideReferenceBox;

		private string error;

		/// <summary>
		/// Prevents the drawer from being applied to UnityEngine.Object references since they are shown as an object field, and is not drawn in-line.
		/// </summary>
		public override bool CanDrawTypeFilter(Type type)
		{
			if (!typeof(MemberInfo).IsAssignableFrom(type))
			{
				return !typeof(UnityEngine.Object).IsAssignableFrom(type);
			}
			return false;
		}

		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			if (base.CanDrawValueProperty(property))
			{
				return !property.Attributes.HasAttribute<DoNotDrawAsReferenceAttribute>();
			}
			return false;
		}

		protected override void Initialize()
		{
			isToggled = this.GetPersistentValue("is_Toggled", defaultValue: false);
			hideReferenceBox = base.Property.Attributes.HasAttribute<HideDuplicateReferenceBoxAttribute>();
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<T> entry = base.ValueEntry;
			if (Event.current.type == EventType.Layout)
			{
				if (entry.ValueState == PropertyValueState.Reference)
				{
					referencedProperty = entry.Property.Tree.GetPropertyAtPath(entry.TargetReferencePath);
					if (referencedProperty == null)
					{
						error = "Reference to " + entry.TargetReferencePath + ". But no property was found at path, which is a problem.";
					}
					else
					{
						error = null;
					}
				}
				else
				{
					error = null;
					referencedProperty = null;
				}
			}
			if (error != null)
			{
				SirenixEditorGUI.MessageBox(error, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
			}
			if (referencedProperty != null)
			{
				PropertyContext<bool> isInReference = referencedProperty.Context.GetGlobal("is_in_reference", defaultValue: false);
				bool drawReferenceBox = true;
				if (!isInReference.Value)
				{
					drawReferenceBox = !hideReferenceBox;
				}
				if (drawReferenceBox)
				{
					SirenixEditorGUI.BeginToolbarBox();
					SirenixEditorGUI.BeginToolbarBoxHeader();
					isToggled.Value = SirenixEditorGUI.Foldout(isToggled.Value, label, out var valueRect);
					GUI.Label(valueRect, "Reference to " + referencedProperty.Path, SirenixGUIStyles.LeftAlignedGreyMiniLabel);
					SirenixEditorGUI.EndToolbarBoxHeader();
					if (SirenixEditorGUI.BeginFadeGroup(UniqueDrawerKey.Create(base.Property, this), isToggled.Value))
					{
						bool previous = isInReference.Value;
						isInReference.Value = true;
						referencedProperty.Draw(label);
						isInReference.Value = previous;
					}
					SirenixEditorGUI.EndFadeGroup();
					SirenixEditorGUI.EndToolbarBox();
				}
				else
				{
					bool previous2 = isInReference.Value;
					isInReference.Value = true;
					referencedProperty.Draw(label);
					isInReference.Value = previous2;
				}
			}
			else
			{
				CallNextDrawer(label);
			}
		}
	}
}

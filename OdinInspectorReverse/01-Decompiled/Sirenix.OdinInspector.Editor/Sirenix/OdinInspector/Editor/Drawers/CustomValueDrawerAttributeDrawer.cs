using System;
using System.Collections;
using Sirenix.OdinInspector.Editor.ActionResolvers;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.ValidateInputAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.ValidateInputAttribute" />
	[DrawerPriority(0.0, 0.0, double.MaxValue)]
	public class CustomValueDrawerAttributeDrawer<T> : OdinAttributeDrawer<CustomValueDrawerAttribute, T>
	{
		private ActionResolver customAction;

		private ValueResolver customValue;

		private static readonly Sirenix.OdinInspector.Editor.ValueResolvers.NamedValue[] customDrawerArgsValue = new Sirenix.OdinInspector.Editor.ValueResolvers.NamedValue[2]
		{
			new Sirenix.OdinInspector.Editor.ValueResolvers.NamedValue("label", typeof(GUIContent)),
			new Sirenix.OdinInspector.Editor.ValueResolvers.NamedValue("callNextDrawer", typeof(Func<GUIContent, bool>))
		};

		private static readonly Sirenix.OdinInspector.Editor.ActionResolvers.NamedValue[] customDrawerArgsAction = new Sirenix.OdinInspector.Editor.ActionResolvers.NamedValue[2]
		{
			new Sirenix.OdinInspector.Editor.ActionResolvers.NamedValue("label", typeof(GUIContent)),
			new Sirenix.OdinInspector.Editor.ActionResolvers.NamedValue("callNextDrawer", typeof(Func<GUIContent, bool>))
		};

		public override bool CanDrawTypeFilter(Type type)
		{
			return !typeof(IList).IsAssignableFrom(type);
		}

		protected override void Initialize()
		{
			customValue = ValueResolver.Get(base.ValueEntry.BaseValueType, base.Property, base.Attribute.Action, customDrawerArgsValue);
			if (!customValue.HasError)
			{
				customValue.Context.NamedValues.Set("callNextDrawer", new Func<GUIContent, bool>(base.CallNextDrawer));
			}
			else if (customValue.ErrorMessage.Contains("void"))
			{
				customAction = ActionResolver.Get(base.Property, base.Attribute.Action, customDrawerArgsAction);
				if (!customAction.HasError)
				{
					customAction.Context.NamedValues.Set("callNextDrawer", new Func<GUIContent, bool>(base.CallNextDrawer));
				}
			}
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (customAction != null)
			{
				if (customAction.HasError)
				{
					SirenixEditorGUI.MessageBox(customAction.ErrorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
					CallNextDrawer(label);
				}
				else
				{
					customAction.Context.NamedValues.Set("label", label);
					customAction.DoAction();
				}
			}
			else if (customValue.ErrorMessage != null)
			{
				SirenixEditorGUI.MessageBox(customValue.ErrorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				CallNextDrawer(label);
			}
			else
			{
				customValue.Context.NamedValues.Set("label", label);
				base.ValueEntry.SmartValue = (T)customValue.GetWeakValue();
			}
		}
	}
}

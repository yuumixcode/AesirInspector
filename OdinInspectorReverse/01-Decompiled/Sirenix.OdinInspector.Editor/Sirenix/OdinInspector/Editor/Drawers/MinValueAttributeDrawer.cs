using System;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	[DrawerPriority(0.0, 9000.0, 0.0)]
	public sealed class MinValueAttributeDrawer<T> : OdinAttributeDrawer<MinValueAttribute, T> where T : struct
	{
		private static readonly bool IsNumber = GenericNumberUtility.IsNumber(typeof(T));

		private static readonly bool IsVector = GenericNumberUtility.IsVector(typeof(T));

		private ValueResolver<double> minValueGetter;

		public override bool CanDrawTypeFilter(Type type)
		{
			if (!IsNumber)
			{
				return IsVector;
			}
			return true;
		}

		protected override void Initialize()
		{
			minValueGetter = ValueResolver.Get(base.Property, base.Attribute.Expression, base.Attribute.MinValue);
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (minValueGetter.HasError)
			{
				SirenixEditorGUI.MessageBox(minValueGetter.ErrorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				CallNextDrawer(label);
				return;
			}
			EditorGUI.BeginChangeCheck();
			CallNextDrawer(label);
			if (EditorGUI.EndChangeCheck())
			{
				T value = base.ValueEntry.SmartValue;
				double min = minValueGetter.GetValue();
				if (!GenericNumberUtility.NumberIsInRange(value, min, double.MaxValue))
				{
					base.ValueEntry.SmartValue = GenericNumberUtility.Clamp(value, min, double.MaxValue);
				}
			}
		}
	}
}

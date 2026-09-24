using System;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	[DrawerPriority(0.0, 9000.0, 0.0)]
	public sealed class MaxValueAttributeDrawer<T> : OdinAttributeDrawer<MaxValueAttribute, T> where T : struct
	{
		private static readonly bool IsNumber = GenericNumberUtility.IsNumber(typeof(T));

		private static readonly bool IsVector = GenericNumberUtility.IsVector(typeof(T));

		private ValueResolver<double> maxValueGetter;

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
			maxValueGetter = ValueResolver.Get(base.Property, base.Attribute.Expression, base.Attribute.MaxValue);
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (maxValueGetter.HasError)
			{
				SirenixEditorGUI.MessageBox(maxValueGetter.ErrorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				CallNextDrawer(label);
				return;
			}
			EditorGUI.BeginChangeCheck();
			CallNextDrawer(label);
			if (EditorGUI.EndChangeCheck())
			{
				T value = base.ValueEntry.SmartValue;
				double max = maxValueGetter.GetValue();
				if (!GenericNumberUtility.NumberIsInRange(value, double.MinValue, max))
				{
					base.ValueEntry.SmartValue = GenericNumberUtility.Clamp(value, double.MinValue, max);
				}
			}
		}
	}
}

using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Common base implementation for progress bar attribute drawers.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class BaseProgressBarAttributeDrawer<T> : OdinAttributeDrawer<ProgressBarAttribute, T>
	{
		private ValueResolver<T> minResolver;

		private ValueResolver<T> maxResolver;

		private ValueResolver<Color> foregroundColorResolver;

		private ValueResolver<Color> backgroundColorResolver;

		private ValueResolver<string> labelResolver;

		/// <summary>
		/// Initialized the drawer.
		/// </summary>
		protected override void Initialize()
		{
			minResolver = ValueResolver.Get(base.Property, base.Attribute.MinGetter, ConvertUtility.Convert<double, T>(base.Attribute.Min));
			maxResolver = ValueResolver.Get(base.Property, base.Attribute.MaxGetter, ConvertUtility.Convert<double, T>(base.Attribute.Max));
			foregroundColorResolver = ValueResolver.Get(base.Property, base.Attribute.ColorGetter, base.Attribute.Color);
			backgroundColorResolver = ValueResolver.Get(base.Property, base.Attribute.BackgroundColorGetter, ProgressBarConfig.Default.BackgroundColor);
			labelResolver = ValueResolver.GetForString(base.Property, base.Attribute.CustomValueStringGetter);
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			ValueResolver.DrawErrors(minResolver, maxResolver, foregroundColorResolver, backgroundColorResolver, labelResolver);
			ProgressBarConfig config = GetConfig();
			Rect rect = EditorGUILayout.GetControlRect(label != null, ((float)config.Height < EditorGUIUtility.singleLineHeight) ? EditorGUIUtility.singleLineHeight : ((float)config.Height));
			T min = minResolver.GetValue();
			T max = maxResolver.GetValue();
			string valueLabel = labelResolver.GetValue();
			EditorGUI.BeginChangeCheck();
			T value = DrawProgressBar(rect, label, ConvertUtility.Convert<T, double>(min), ConvertUtility.Convert<T, double>(max), config, valueLabel);
			if (EditorGUI.EndChangeCheck())
			{
				base.ValueEntry.SmartValue = value;
			}
		}

		private ProgressBarConfig GetConfig()
		{
			ProgressBarConfig config = ProgressBarConfig.Default;
			config.Height = base.Attribute.Height;
			config.DrawValueLabel = (base.Attribute.DrawValueLabelHasValue ? base.Attribute.DrawValueLabel : (!base.Attribute.Segmented));
			config.ValueLabelAlignment = (base.Attribute.ValueLabelAlignmentHasValue ? base.Attribute.ValueLabelAlignment : ((!base.Attribute.Segmented) ? TextAlignment.Center : TextAlignment.Right));
			if (base.Attribute.CustomValueStringGetter != null)
			{
				config.DrawValueLabel = false;
			}
			if (Event.current.type == EventType.Repaint)
			{
				config.ForegroundColor = foregroundColorResolver.GetValue();
				config.BackgroundColor = backgroundColorResolver.GetValue();
			}
			return config;
		}

		/// <summary>
		/// Generic implementation of progress bar field drawing.
		/// </summary>
		protected abstract T DrawProgressBar(Rect rect, GUIContent label, double min, double max, ProgressBarConfig config, string valueLabel);

		/// <summary>
		/// Converts the generic value to a double.
		/// </summary>
		/// <param name="value">The generic value to convert.</param>
		/// <returns>The generic value as a double.</returns>
		protected abstract double ConvertToDouble(T value);
	}
}

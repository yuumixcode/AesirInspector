using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws values decorated with <see cref="T:Sirenix.OdinInspector.ProgressBarAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	public sealed class ProgressBarAttributeLongDrawer : BaseProgressBarAttributeDrawer<long>
	{
		/// <summary>
		/// Draws a progress bar for a long property.
		/// </summary>
		protected override long DrawProgressBar(Rect rect, GUIContent label, double min, double max, ProgressBarConfig config, string valueLabel)
		{
			if (base.Attribute.Segmented)
			{
				return SirenixEditorFields.SegmentedProgressBarField(rect, label, base.ValueEntry.SmartValue, (long)min, (long)max, config, valueLabel);
			}
			return (long)SirenixEditorFields.ProgressBarField(rect, label, base.ValueEntry.SmartValue, min, max, config, valueLabel);
		}

		/// <summary>
		/// Converts the generic value to a double.
		/// </summary>
		/// <param name="value">The generic value to convert.</param>
		/// <returns>The generic value as a double.</returns>
		protected override double ConvertToDouble(long value)
		{
			return value;
		}
	}
}

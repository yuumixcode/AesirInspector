using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws values decorated with <see cref="T:Sirenix.OdinInspector.ProgressBarAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	public sealed class ProgressBarAttributeUlongDrawer : BaseProgressBarAttributeDrawer<ulong>
	{
		/// <summary>
		/// Draws a progress bar for a ulong property.
		/// </summary>
		protected override ulong DrawProgressBar(Rect rect, GUIContent label, double min, double max, ProgressBarConfig config, string valueLabel)
		{
			if (base.Attribute.Segmented)
			{
				return (ulong)SirenixEditorFields.SegmentedProgressBarField(rect, label, (long)base.ValueEntry.SmartValue, (long)min, (long)max, config, valueLabel);
			}
			return (ulong)SirenixEditorFields.ProgressBarField(rect, label, base.ValueEntry.SmartValue, min, max, config, valueLabel);
		}

		/// <summary>
		/// Converts the generic value to a double.
		/// </summary>
		/// <param name="value">The generic value to convert.</param>
		/// <returns>The generic value as a double.</returns>
		protected override double ConvertToDouble(ulong value)
		{
			return value;
		}
	}
}

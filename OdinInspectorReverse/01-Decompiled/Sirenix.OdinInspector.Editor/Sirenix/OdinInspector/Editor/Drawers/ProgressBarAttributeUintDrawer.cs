using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws values decorated with <see cref="T:Sirenix.OdinInspector.ProgressBarAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	public sealed class ProgressBarAttributeUintDrawer : BaseProgressBarAttributeDrawer<uint>
	{
		/// <summary>
		/// Draws a progress bar for a uint property.
		/// </summary>
		protected override uint DrawProgressBar(Rect rect, GUIContent label, double min, double max, ProgressBarConfig config, string valueLabel)
		{
			if (base.Attribute.Segmented)
			{
				return (uint)SirenixEditorFields.SegmentedProgressBarField(rect, label, base.ValueEntry.SmartValue, (long)min, (long)max, config, valueLabel);
			}
			return (uint)SirenixEditorFields.ProgressBarField(rect, label, base.ValueEntry.SmartValue, min, max, config, valueLabel);
		}

		/// <summary>
		/// Converts the generic value to a double.
		/// </summary>
		/// <param name="value">The generic value to convert.</param>
		/// <returns>The generic value as a double.</returns>
		protected override double ConvertToDouble(uint value)
		{
			return value;
		}
	}
}

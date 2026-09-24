using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(ProgressBarAttribute), "The ProgressBar attribute draws a horizontal colored bar, which can also be clicked to change the value.\n\nIt can be used to show how full an inventory might be, or to make a visual indicator for a healthbar. It can even be used to make fighting game style health bars, that stack multiple layers of health.")]
	internal sealed class ProgressBarExamples
	{
		[ProgressBar(0.0, 100.0, 0.15f, 0.47f, 0.74f)]
		public int ProgressBar = 50;

		[ProgressBar(-100.0, 100.0, 1f, 1f, 1f, Height = 30)]
		[HideLabel]
		public short BigColoredProgressBar = 50;

		[ProgressBar(0.0, 10.0, 0f, 1f, 0f, Segmented = true)]
		public int SegmentedColoredBar = 5;

		[ProgressBar(0.0, 100.0, 0.15f, 0.47f, 0.74f, ColorGetter = "GetHealthBarColor")]
		public float DynamicHealthBarColor = 50f;

		[BoxGroup("Dynamic Range", true, false, 0f)]
		[ProgressBar("Min", "Max", 0.15f, 0.47f, 0.74f)]
		public float DynamicProgressBar = 50f;

		[BoxGroup("Dynamic Range", true, false, 0f)]
		public float Min;

		[BoxGroup("Dynamic Range", true, false, 0f)]
		public float Max = 100f;

		[HideLabel]
		[BoxGroup("Stacked Health", true, false, 0f)]
		[Range(0f, 300f)]
		public float StackedHealth = 150f;

		[BoxGroup("Stacked Health", true, false, 0f)]
		[ProgressBar(0.0, 100.0, 0.15f, 0.47f, 0.74f, ColorGetter = "GetStackedHealthColor", BackgroundColorGetter = "GetStackHealthBackgroundColor", DrawValueLabel = false)]
		[ShowInInspector]
		[HideLabel]
		private float StackedHealthProgressBar => StackedHealth % 100.01f;

		private Color GetHealthBarColor(float value)
		{
			return Color.Lerp(Color.red, Color.green, Mathf.Pow(value / 100f, 2f));
		}

		private Color GetStackedHealthColor()
		{
			if (!(StackedHealth > 200f))
			{
				if (!(StackedHealth > 100f))
				{
					return Color.red;
				}
				return Color.green;
			}
			return Color.white;
		}

		private Color GetStackHealthBackgroundColor()
		{
			if (!(StackedHealth > 200f))
			{
				if (!(StackedHealth > 100f))
				{
					return new Color(0.16f, 0.16f, 0.16f, 1f);
				}
				return Color.red;
			}
			return Color.green;
		}
	}
}

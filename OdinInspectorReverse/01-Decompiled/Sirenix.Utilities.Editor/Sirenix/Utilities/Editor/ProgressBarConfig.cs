using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// Configuration for progress bar fields.
	/// </summary>
	public struct ProgressBarConfig
	{
		/// <summary>
		/// The height of the progress bar field. Default 12 pixel.
		/// </summary>
		public int Height;

		/// <summary>
		/// The foreground color of the progress bar field.
		/// </summary>
		public Color ForegroundColor;

		/// <summary>
		/// The background color of the progress bar field.
		/// </summary>
		public Color BackgroundColor;

		/// <summary>
		/// If <c>true</c> the progress bar field will draw a label ontop to show the current value.
		/// </summary>
		public bool DrawValueLabel;

		/// <summary>
		/// Alignment of the progress bar field overlay.
		/// </summary>
		public TextAlignment ValueLabelAlignment;

		/// <summary>
		/// Default configuration.
		/// </summary>
		public static ProgressBarConfig Default
		{
			get
			{
				Color foregroundColor = new Color(0.24f, 0.387f, 0.783f, 1f);
				Color backgroundColor = new Color(0.651f, 0.651f, 0.651f, 1f);
				if (EditorGUIUtility.isProSkin)
				{
					foregroundColor = new Color(0.28f, 0.659f, 0.978f, 1f);
					backgroundColor = new Color(0.16f, 0.16f, 0.16f, 1f);
				}
				return new ProgressBarConfig(12, foregroundColor, backgroundColor, textOverlay: false, TextAlignment.Center);
			}
		}

		/// <summary>
		/// Creates a copy of the configuration.
		/// </summary>
		/// <param name="config">The configuration to copy.</param>
		public ProgressBarConfig(ProgressBarConfig config)
		{
			Height = config.Height;
			ForegroundColor = config.ForegroundColor;
			BackgroundColor = config.BackgroundColor;
			DrawValueLabel = config.DrawValueLabel;
			ValueLabelAlignment = config.ValueLabelAlignment;
		}

		/// <summary>
		/// Creates a progress bar configuration.
		/// </summary>
		/// <param name="height">The height of the progress bar.</param>
		/// <param name="foregroundColor">The foreground color of the progress bar.</param>
		/// <param name="backgroundColor">The background color of the progress bar.</param>
		/// <param name="textOverlay">If <c>true</c> there will be drawn a overlay on top of the field.</param>
		/// <param name="overlayAlignment">The alignment of the text overlay.</param>
		public ProgressBarConfig(int height, Color foregroundColor, Color backgroundColor, bool textOverlay, TextAlignment overlayAlignment)
		{
			Height = height;
			ForegroundColor = foregroundColor;
			BackgroundColor = backgroundColor;
			DrawValueLabel = textOverlay;
			ValueLabelAlignment = overlayAlignment;
		}
	}
}

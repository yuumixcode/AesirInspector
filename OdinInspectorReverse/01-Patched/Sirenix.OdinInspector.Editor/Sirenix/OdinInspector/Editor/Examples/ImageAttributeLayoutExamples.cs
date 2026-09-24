using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
	[AttributeExample(typeof(ImageAttribute), "Width and Height control the image rect. When both are 0, Image uses the image's natural size unless FitToAvailableWidth is enabled.", Name = "Layout", Order = 10f)]
	internal class ImageAttributeLayoutExamples
	{
		[InfoBox("No width or height is specified here. The preview uses the texture's natural size.", InfoMessageType.Info, null)]
		[Title("Natural Size", "With Width = 0 and Height = 0, the image uses its natural pixel size.", TitleAlignments.Left, true, true)]
		[Image(DrawProperty = false)]
		[HideLabel]
		public Texture2D NaturalSize;

		[HideLabel]
		[Image(FitToAvailableWidth = true, DrawProperty = false)]
		[Title("FitToAvailableWidth", "FitToAvailableWidth scales the image inside the normal layout width.", TitleAlignments.Left, true, true)]
		[InfoBox("FitToAvailableWidth scales the image to the normal layout width while preserving aspect ratio.", InfoMessageType.Info, null)]
		public Texture2D AvailableWidth;

		[Title("Height", "A single number sets the image height. The layout provides the available width.", TitleAlignments.Left, true, true)]
		[InfoBox("Height is set to 64. The preview uses the Texture2D field it is placed on.", InfoMessageType.Info, null)]
		[Image(64f, DrawProperty = false)]
		[HideLabel]
		public Texture2D HeightPreview;

		[Title("Width And Height", "Two numbers create a fixed-size image rect that is aligned inside the available layout.", TitleAlignments.Left, true, true)]
		[InfoBox("Width is set to 260 and Height is set to 64.", InfoMessageType.Info, null)]
		[Image(260f, 64f, DrawProperty = false)]
		[HideLabel]
		public Texture2D WidthAndHeightPreview;

		[Image(220f, 56f, Alignment = 0f, DrawProperty = false)]
		[Title("Alignment", "Alignment moves the image horizontally inside the available preview area.", TitleAlignments.Left, true, true)]
		[InfoBox("Alignment = 0 places the preview on the left.", InfoMessageType.Info, null)]
		[HideLabel]
		public Texture2D AlignLeft;

		[HideLabel]
		[InfoBox("Alignment = 0.5 centers the preview.", InfoMessageType.Info, null)]
		[Image(220f, 56f, Alignment = 0.5f, DrawProperty = false)]
		public Texture2D AlignCenter;

		[InfoBox("Alignment = 1 places the preview on the right.", InfoMessageType.Info, null)]
		[HideLabel]
		[Image(220f, 56f, Alignment = 1f, DrawProperty = false)]
		public Texture2D AlignRight;

		[Title("IgnorePadding", "Toggle this to compare the same filled banner inside normal padding and with padding ignored.", TitleAlignments.Left, true, true)]
		[ToggleLeft]
		public bool UseIgnorePadding = true;

		[ShowIf("UseIgnorePadding", true)]
		[InfoBox("IgnorePadding is enabled. ScaleAndCrop fills the preview rect so the edge-to-edge width is visible.", InfoMessageType.Info, null)]
		[Image(72f, ImageScaleMode.ScaleAndCrop, IgnorePadding = true, DrawProperty = false)]
		[HideLabel]
		public Texture2D IgnorePaddingOn;

		[Image(72f, ImageScaleMode.ScaleAndCrop, DrawProperty = false)]
		[HideIf("UseIgnorePadding", true)]
		[InfoBox("IgnorePadding is disabled. The same filled banner stays inside the normal inspector padding.", InfoMessageType.Info, null)]
		[HideLabel]
		public Texture2D IgnorePaddingOff;

		[OnInspectorInit]
		private void CreateData()
		{
			CleanupData();
			NaturalSize = ExampleHelper.GetTexture(260, 80, ExampleTextureTheme.Blue);
			AvailableWidth = ExampleHelper.GetTexture(1200, 180, ExampleTextureTheme.Green);
			HeightPreview = ExampleHelper.GetTexture(1200, 180, ExampleTextureTheme.Blue);
			WidthAndHeightPreview = ExampleHelper.GetTexture(520, 128, ExampleTextureTheme.Blue);
			AlignLeft = ExampleHelper.GetTexture(520, 128, ExampleTextureTheme.Purple);
			AlignCenter = AlignLeft;
			AlignRight = AlignLeft;
			IgnorePaddingOn = ExampleHelper.GetTexture(1200, 160, ExampleTextureTheme.Green);
			IgnorePaddingOff = IgnorePaddingOn;
		}

		[OnInspectorDispose]
		private void CleanupData()
		{
			NaturalSize = null;
			AvailableWidth = null;
			HeightPreview = null;
			WidthAndHeightPreview = null;
			AlignLeft = null;
			AlignCenter = null;
			AlignRight = null;
			IgnorePaddingOn = null;
			IgnorePaddingOff = null;
		}
	}
}

using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
	[AttributeExample(typeof(ImageAttribute), "ScaleMode, FilterMode, and AlphaBlend control how the resolved image is rendered inside the image rect.", Name = "Rendering", Order = 20f)]
	internal class ImageAttributeRenderingExamples
	{
		[ToggleLeft]
		[Title("ScaleMode", "Toggle this to compare ScaleToFit with ScaleAndCrop in the same 170 x 80 rect.", TitleAlignments.Left, true, true)]
		public bool UseScaleAndCrop;

		[HideIf("UseScaleAndCrop", true)]
		[HideLabel]
		[Image(170f, 80f, DrawProperty = false)]
		[InfoBox("ScaleToFit is the default. The whole image stays visible.", InfoMessageType.Info, null)]
		public Texture2D ScaleToFit;

		[Image(170f, 80f, ImageScaleMode.ScaleAndCrop, DrawProperty = false)]
		[InfoBox("ScaleAndCrop fills the entire rect and crops what does not fit.", InfoMessageType.Info, null)]
		[ShowIf("UseScaleAndCrop", true)]
		[HideLabel]
		public Texture2D ScaleAndCrop;

		[Title("FilterMode", "Toggle this to compare smooth filtering with hard pixel edges.", TitleAlignments.Left, true, true)]
		[ToggleLeft]
		public bool UsePointFiltering = true;

		[HideLabel]
		[HideIf("UsePointFiltering", true)]
		[InfoBox("Bilinear filtering smooths the texture when it is scaled.", InfoMessageType.Info, null)]
		[Image(160f, 80f, FilterMode = FilterMode.Bilinear, DrawProperty = false)]
		public Texture2D Bilinear;

		[HideLabel]
		[Image(160f, 80f, FilterMode = FilterMode.Point, DrawProperty = false)]
		[InfoBox("Point filtering keeps each pixel sharp.", InfoMessageType.Info, null)]
		[ShowIf("UsePointFiltering", true)]
		public Texture2D Point;

		[ToggleLeft]
		[Title("AlphaBlend", "Turn alpha blending off only when the image should be treated as opaque.", TitleAlignments.Left, true, true)]
		public bool AlphaBlend = true;

		[InfoBox("AlphaBlend is enabled. Transparent parts of the image blend with the inspector background.", InfoMessageType.Info, null)]
		[ShowIf("AlphaBlend", true)]
		[HideLabel]
		[Image(120f, AlphaBlend = true, DrawProperty = false)]
		public Texture2D AlphaBlendOn;

		[InfoBox("AlphaBlend is disabled. Transparent pixels are treated as opaque.", InfoMessageType.Info, null)]
		[HideLabel]
		[Image(120f, AlphaBlend = false, DrawProperty = false)]
		[HideIf("AlphaBlend", true)]
		public Texture2D AlphaBlendOff;

		[OnInspectorInit]
		private void CreateData()
		{
			CleanupData();
			ScaleToFit = ExampleHelper.GetTexture(160, 260, ExampleTextureTheme.Blue);
			ScaleAndCrop = ScaleToFit;
			Bilinear = ExampleHelper.GetCheckerTexture(64, 32);
			Point = Bilinear;
			AlphaBlendOn = ExampleHelper.GetTransparentTexture(320, 120);
			AlphaBlendOff = AlphaBlendOn;
		}

		[OnInspectorDispose]
		private void CleanupData()
		{
			ScaleToFit = null;
			ScaleAndCrop = null;
			Bilinear = null;
			Point = null;
			AlphaBlendOn = null;
			AlphaBlendOff = null;
		}
	}
}

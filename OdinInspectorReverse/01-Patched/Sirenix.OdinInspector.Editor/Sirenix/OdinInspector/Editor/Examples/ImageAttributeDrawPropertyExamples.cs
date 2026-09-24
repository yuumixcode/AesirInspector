using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(ImageAttribute), "DrawProperty controls whether the original property is drawn, and DrawPosition controls whether the image appears before or after it.", Name = "Draw Property", Order = 40f)]
	[ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
	internal class ImageAttributeDrawPropertyExamples
	{
		[ToggleLeft]
		[Title("DrawProperty", "Toggle this to compare drawing only the image with drawing both image and property.", TitleAlignments.Left, true, true)]
		public bool DrawProperty = true;

		[ShowIf("DrawProperty", true)]
		[Image(64f)]
		[InfoBox("[Image(64)] draws the image first, then the Texture2D field.", InfoMessageType.Info, null)]
		public Texture2D DrawPropertyOn;

		[HideIf("DrawProperty", true)]
		[InfoBox("[Image(64, DrawProperty = false)] draws only the image.", InfoMessageType.Info, null)]
		[Image(64f, DrawProperty = false)]
		public Texture2D DrawPropertyOff;

		[Image(64f, DrawPosition = ImageDrawPosition.AfterProperty)]
		[InfoBox("DrawPosition = AfterProperty draws the Texture2D field first, then the image.", InfoMessageType.Info, null)]
		[Title("DrawPosition", "DrawPosition controls whether the image appears before or after the original property.", TitleAlignments.Left, true, true)]
		public Texture2D DrawAfterProperty;

		[OnInspectorInit]
		private void CreateData()
		{
			CleanupData();
			DrawPropertyOn = ExampleHelper.GetTexture(960, 180, ExampleTextureTheme.Purple);
			DrawPropertyOff = DrawPropertyOn;
			DrawAfterProperty = ExampleHelper.GetTexture(960, 180, ExampleTextureTheme.Green);
		}

		[OnInspectorDispose]
		private void CleanupData()
		{
			DrawPropertyOn = null;
			DrawPropertyOff = null;
			DrawAfterProperty = null;
		}
	}
}

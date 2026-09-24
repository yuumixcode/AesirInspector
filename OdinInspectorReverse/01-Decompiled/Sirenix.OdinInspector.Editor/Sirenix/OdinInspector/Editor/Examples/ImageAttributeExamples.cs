using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(ImageAttribute), "Image can draw the value it is placed on, or it can draw another image found through ImageSource.", Name = "Sources", Order = 0f)]
	[ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
	internal class ImageAttributeExamples
	{
		[HideLabel]
		[Image(80f, DrawProperty = false)]
		[InfoBox("Texture2D value: this preview comes from the Texture2D field that has the Image attribute.", InfoMessageType.Info, null)]
		[Title("No Image Source", "When ImageSource is empty, Image draws the value it is placed on.", TitleAlignments.Left, true, true)]
		public Texture2D Texture;

		[Title("Sprite Value", "The same no-source setup works when the decorated value is a Sprite.", TitleAlignments.Left, true, true)]
		[Image(64f, DrawProperty = false)]
		[HideLabel]
		[InfoBox("Sprite value: this preview comes from the Sprite field that has the Image attribute.", InfoMessageType.Info, null)]
		public Sprite Sprite;

		[Title("ImageSource", "The attribute is still placed on a Texture2D field, but ImageSource tells it to draw MemberBanner instead.", TitleAlignments.Left, true, true)]
		[InfoBox("This preview comes from MemberBanner, not from the decorated Texture2D field.", InfoMessageType.Info, null)]
		[Image("MemberBanner", 72f)]
		public Texture2D DecoratedTexture;

		private Texture2D SpriteTexture;

		private Texture2D MemberBanner;

		[OnInspectorInit]
		private void CreateData()
		{
			CleanupData();
			Texture = ExampleHelper.GetTexture(260, 150, ExampleTextureTheme.Blue);
			SpriteTexture = ExampleHelper.GetTexture(128, 128, ExampleTextureTheme.Purple);
			MemberBanner = ExampleHelper.GetTexture(960, 180, ExampleTextureTheme.Warm);
			DecoratedTexture = ExampleHelper.GetTexture(128, 128, ExampleTextureTheme.Green);
			Sprite = Sprite.Create(SpriteTexture, new Rect(0f, 0f, SpriteTexture.width, SpriteTexture.height), new Vector2(0.5f, 0.5f));
		}

		[OnInspectorDispose]
		private void CleanupData()
		{
			if (Sprite != null)
			{
				Object.DestroyImmediate(Sprite);
				Sprite = null;
			}
			Texture = null;
			SpriteTexture = null;
			MemberBanner = null;
			DecoratedTexture = null;
		}
	}
}

using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
	[AttributeExample(typeof(ImageAttribute), "Image can be placed on a class to draw a type-level banner before the type's members.", Name = "Class Attribute", Order = 30f)]
	[Image("TypeBanner", 72f, IgnorePadding = true)]
	internal class ImageAttributeClassExamples
	{
		[HideLabel]
		[DisplayAsString]
		[InfoBox("The banner above this message is drawn by the Image attribute on the class itself.", InfoMessageType.Info, null)]
		public string ClassAttribute = "[Image(\"TypeBanner\", 72, IgnorePadding = true)]";

		private Texture2D TypeBanner;

		[OnInspectorInit]
		private void CreateData()
		{
			CleanupData();
			TypeBanner = ExampleHelper.GetTexture(1200, 160, ExampleTextureTheme.Warm);
		}

		[OnInspectorDispose]
		private void CleanupData()
		{
			TypeBanner = null;
		}
	}
}

using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(TypeRegistryItemAttribute))]
	internal class TypeRegistryItemSettingsExample
	{
		[TypeRegistryItem(null, null, SdfIconType.None, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0, Name = "Painting Tools", Icon = SdfIconType.Tools, CategoryPath = "Sirenix.TypeSelector.Demo", Priority = int.MinValue)]
		public abstract class Base
		{
		}

		[TypeRegistryItem(null, null, SdfIconType.None, 0.3f, 0.1f, 0f, 0f, 0.8f, 0.3f, 0f, 0f, 0, Name = "Brush", CategoryPath = "Sirenix.TypeSelector.Demo/Painting Tools", Icon = SdfIconType.BrushFill, Priority = int.MinValue)]
		public class InheritorA : Base
		{
			public Color Color = Color.red;

			public float PaintRemaining = 0.4f;
		}

		[TypeRegistryItem(null, null, SdfIconType.None, 0f, 0.3f, 0.1f, 0f, 0f, 0.8f, 0.3f, 0f, 0, Name = "Paint Bucket", CategoryPath = "Sirenix.TypeSelector.Demo/Painting Tools", Icon = SdfIconType.PaintBucket, Priority = int.MinValue)]
		public class InheritorB : Base
		{
			public Color Color = Color.green;

			public float PaintRemaining = 0.8f;
		}

		[TypeRegistryItem(null, null, SdfIconType.None, 0f, 0.1f, 0.3f, 0f, 0f, 0.3f, 0.8f, 0f, 0, Name = "Palette", CategoryPath = "Sirenix.TypeSelector.Demo/Painting Tools", Icon = SdfIconType.PaletteFill, Priority = int.MinValue)]
		public class InheritorC : Base
		{
			public ColorPaletteItem[] Colors = new ColorPaletteItem[4]
			{
				new ColorPaletteItem(Color.blue, 0.8f),
				new ColorPaletteItem(Color.red, 0.5f),
				new ColorPaletteItem(Color.green, 1f),
				new ColorPaletteItem(Color.white, 0.6f)
			};
		}

		public struct ColorPaletteItem
		{
			public Color Color;

			public float Remaining;

			public ColorPaletteItem(Color color, float remaining)
			{
				Color = color;
				Remaining = remaining;
			}
		}

		private const string CATEGORY_PATH = "Sirenix.TypeSelector.Demo";

		private const string BASE_ITEM_NAME = "Painting Tools";

		private const string PATH = "Sirenix.TypeSelector.Demo/Painting Tools";

		[ShowInInspector]
		[PolymorphicDrawerSettings(ShowBaseType = false)]
		[InlineProperty]
		public Base PaintingItem;
	}
}

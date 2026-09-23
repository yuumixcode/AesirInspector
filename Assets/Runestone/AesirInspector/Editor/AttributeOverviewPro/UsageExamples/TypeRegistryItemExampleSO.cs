using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TypeRegistryItem 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class TypeRegistryItemExampleSO : AttributeExampleSO<TypeRegistryItemExampleSO>
    {
        const string CATEGORY_PATH = "Sirenix.TypeSelector.Demo";
        const string BASE_ITEM_NAME = "Painting Tools";
        const string PATH = CATEGORY_PATH + "/" + BASE_ITEM_NAME;

        [Title("Default Style")]
        [ShowInInspector]
        [PolymorphicDrawerSettings(ShowBaseType = true)]
        public BasicClass BasicItem;

        [Title("Using TypeRegistryItem Attribute")]
        [ShowInInspector]
        [PolymorphicDrawerSettings(ShowBaseType = false)]
        [InlineProperty]
        public Base PaintingItem;

        public abstract class BasicClass { }

        public class MyClassA : BasicClass
        {
            public string Name;
        }

        public class MyClassB : BasicClass
        {
            public int Number;
        }

        public class MyClassC : BasicClass
        {
            public float Number;
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

        [TypeRegistryItem(null, null, SdfIconType.None, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0, Name = BASE_ITEM_NAME, Icon = SdfIconType.Tools, CategoryPath = CATEGORY_PATH, Priority = int.MinValue)]
        public abstract class Base { }

        [TypeRegistryItem(null, null, SdfIconType.None, 0.3f, 0.1f, 0f, 0f, 0.8f, 0.3f, 0f, 0f, 0, Name = "Brush", CategoryPath = PATH, Icon = SdfIconType.BrushFill, Priority = int.MinValue)]
        public class InheritorA : Base
        {
            public Color Color = Color.red;

            public float PaintRemaining = 0.4f;
        }

        [TypeRegistryItem(null, null, SdfIconType.None, 0f, 0.3f, 0.1f, 0f, 0f, 0.8f, 0.3f, 0f, 0, Name = "Paint Bucket", CategoryPath = PATH, Icon = SdfIconType.PaintBucket, Priority = int.MinValue)]
        public class InheritorB : Base
        {
            public Color Color = Color.green;

            public float PaintRemaining = 0.8f;
        }

        [TypeRegistryItem(null, null, SdfIconType.None, 0f, 0.1f, 0.3f, 0f, 0f, 0.3f, 0.8f, 0f, 0, Name = "Palette", CategoryPath = PATH, Icon = SdfIconType.PaletteFill, Priority = int.MinValue)]
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

        public override void AesirInspectorReset()
        {
            BasicItem = null;
            PaintingItem = null;
        }
    }
}

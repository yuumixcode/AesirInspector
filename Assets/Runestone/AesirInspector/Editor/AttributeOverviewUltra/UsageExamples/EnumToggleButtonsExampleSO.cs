using System;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// EnumToggleButtons 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class EnumToggleButtonsExampleSO : AttributeExampleSO<EnumToggleButtonsExampleSO>
    {
        [Flags]
        public enum SomeBitmaskEnum
        {
            A = 1 << 0,
            B = 1 << 1,
            C = 1 << 2,
            All = A | B | C
        }

        public enum SomeEnum
        {
            First,
            Second,
            Third,
            Fourth,
            AndSoOn
        }

        public enum SomeEnumWithIcons
        {
            [LabelText(SdfIconType.TextLeft)]
            TextLeft,

            [LabelText(SdfIconType.TextCenter)]
            TextCenter,

            [LabelText(SdfIconType.TextRight)]
            TextRight
        }

        public enum SomeEnumWithIconsAndNames
        {
            [LabelText("Align Left", SdfIconType.TextLeft)]
            TextLeft,

            [LabelText("Align Center", SdfIconType.TextCenter)]
            TextCenter,

            [LabelText("Align Right", SdfIconType.TextRight)]
            TextRight
        }

        [Title("No Parameters")]
        [EnumToggleButtons]
        public SomeEnum someEnumField;

        [Title("No Parameters")]
        [EnumToggleButtons]
        [HideLabel]
        public SomeEnum wideEnumField;

        [Title("Bitmask Enum")]
        public SomeBitmaskEnum defaultEnumBitmask;

        [Title("Bitmask Enum")]
        [EnumToggleButtons]
        public SomeBitmaskEnum bitmaskEnumField;

        [Title("Bitmask Enum")]
        [EnumToggleButtons]
        [HideLabel]
        public SomeBitmaskEnum enumFieldWide;

        [Title("Icon Enum")]
        [EnumToggleButtons]
        [HideLabel]
        public SomeEnumWithIcons enumWithIcons;

        [Title("Icon Enum")]
        [EnumToggleButtons]
        [HideLabel]
        public SomeEnumWithIconsAndNames enumWithIconsAndNames;

        public override void AesirInspectorReset()
        {
            someEnumField = SomeEnum.First;
            wideEnumField = SomeEnum.First;
            defaultEnumBitmask = 0;
            bitmaskEnumField = 0;
            enumFieldWide = 0;
            enumWithIcons = SomeEnumWithIcons.TextLeft;
            enumWithIconsAndNames = SomeEnumWithIconsAndNames.TextLeft;
        }
    }
}

using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// DisplayAsString 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class DisplayAsStringExampleSO : AttributeExampleSO<DisplayAsStringExampleSO>
    {
        [Title("No Parameters")]
        [DisplayAsString]
        public string label = "This is a string displayed as a label";

        [Title("No Parameters")]
        [InfoBox("Instead of disabling values in the inspector in order to show some information or debug a value. You can use DisplayAsString to show the value as text, instead of showing it in a disabled drawer", InfoMessageType.Info)]
        [DisplayAsString]
        public Color SomeColor;

        [Title("Parameter: FontSize")]
        [DisplayAsString(16)]
        public string largeLabel = "Large font text";

        [Title("Parameter: Alignment")]
        [DisplayAsString(TextAlignment.Center)]
        public string centerLabel = "Center aligned text";

        [Title("Parameter: Overflow")]
        [InfoBox("The DisplayAsString attribute can also be configured to enable or disable overflowing to multiple lines.", InfoMessageType.Info)]
        [HideLabel]
        [DisplayAsString]
        public string Overflow = "A very very very very very very very very very long string that has been configured to overflow.";

        [Title("Parameter: Overflow")]
        [DisplayAsString(false)]
        [HideLabel]
        public string DisplayAllOfIt = "A very very very very very very very very long string that has been configured to not overflow.";

        [Title("Parameter: Overflow, FontSize, Alignment, EnableRichText")]
        [InfoBox("Additionally, you can also configure the string's alignment, font size, and whether it should support rich text or not.", InfoMessageType.Info)]
        [DisplayAsString(false, 20, TextAlignment.Center, true)]
        public string CustomFontSizeAlignmentAndRichText = "This string is <b><color=#FF5555><i>super</i> <size=24>big</size></color></b> and centered.";

        public override void AesirInspectorReset()
        {
            label = "This is a string displayed as a label";
            SomeColor = default(Color);
            largeLabel = "Large font text";
            centerLabel = "Center aligned text";
            Overflow = "A very very very very very very very very very long string that has been configured to overflow.";
            DisplayAllOfIt = "A very very very very very very very very long string that has been configured to not overflow.";
            CustomFontSizeAlignmentAndRichText = "This string is <b><color=#FF5555><i>super</i> <size=24>big</size></color></b> and centered.";
        }
    }
}

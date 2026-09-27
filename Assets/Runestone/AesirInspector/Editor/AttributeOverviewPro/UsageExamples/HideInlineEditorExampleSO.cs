using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    [AesirExample]
    public class HideInInlineEditorsExampleSO : AttributeExampleSO<HideInInlineEditorsExampleSO>
    {
        [Title("No Parameters")]
        [InfoBox(
            "The marked member is hidden when this object is drawn inside an inline editor, and visible when the object is inspected directly. Use the pen icon of an inline editor to open a dedicated inspector and compare.")]
        [InlineEditor(Expanded = true)]
        [HideInInlineEditors]
        public HideMonoScriptExampleSO hiddenInlineEditor;

        [InlineEditor(Expanded = true)]
        public HideMonoScriptExampleSO shownInlineEditor;

        public override void AesirInspectorReset()
        {
            hiddenInlineEditor = null;
            shownInlineEditor = null;
        }
    }
}

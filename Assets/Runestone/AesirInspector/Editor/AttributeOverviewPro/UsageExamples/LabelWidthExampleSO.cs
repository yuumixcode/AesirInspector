using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// LabelWidth 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class LabelWidthExampleSO : AttributeExampleSO<LabelWidthExampleSO>
    {
        [Title("No Parameters")]
        public int DefaultWidth;

        [Title("Parameter: Width (Fixed)")]
        [LabelWidth(50f)]
        public int Thin;

        [Title("Parameter: Width (Fixed)")]
        [LabelWidth(250f)]
        public int Wide;

        [Title("Parameter: Width (Relative)")]
        [LabelWidth(-50f)]
        public int relativeLabel;

        public override void AesirInspectorReset()
        {
            DefaultWidth = 0;
            Thin = 0;
            Wide = 0;
            relativeLabel = 0;
        }
    }
}

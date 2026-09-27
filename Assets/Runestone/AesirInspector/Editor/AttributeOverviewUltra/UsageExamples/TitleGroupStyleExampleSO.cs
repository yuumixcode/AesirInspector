using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TitleGroup 特性的案例 SO：horizontalLine、boldTitle、indent 参数。
    /// </summary>
    [AesirExample]
    public class TitleGroupStyleExampleSO : AttributeExampleSO<TitleGroupStyleExampleSO>
    {
        [TitleGroup("No Horizontal Line", horizontalLine: false)]
        public int noHorizontalLine;

        [TitleGroup("Not Bold", boldTitle: false)]
        public int notBold;

        [TitleGroup("Indented", indent: true)]
        public int indented;

        [TitleGroup("Indented/Nested Indented", indent: true)]
        public int nestedIndented;

        public override void AesirInspectorReset()
        {
            noHorizontalLine = 0;
            notBold = 0;
            indented = 0;
            nestedIndented = 0;
        }
    }
}

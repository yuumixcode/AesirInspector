using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// LabelText 特性的案例 SO。
    /// </summary>
    [AesirExample]
    internal class LabelTextExampleSO : AttributeExampleSO<LabelTextExampleSO>
    {
        [Title("No Parameters")]
        [LabelText("Custom Label")]
        public int customLabel = 1;

        [Title("Member Reference ($)")]
        [LabelText("$dynamicLabel")]
        public string labelFromMember = "The label above is dynamic";

        [Title("Member Reference ($)")]
        public string dynamicLabel = "Dynamic Label Text";

        [Title("Member Reference ($)")]
        [LabelText("3")]
        public int MyInt3 = 123;

        [Title("Member Reference ($)")]
        [LabelText("$MyInt3")]
        public string LabelText = "The label is taken from the number 3 above";

        [Title("Expression (@)")]
        [LabelText("@\"Current Time: \" + DateTime.Now.ToString(\"HH:mm:ss\")")]
        public string expressionLabel;

        [Title("Expression (@)")]
        [LabelText("@DateTime.Now.ToString(\"HH:mm:ss\")")]
        public string DateTimeLabel;

        [Title("Parameter: NicifyText")]
        [LabelText("m_myField", true)]
        public int nicifiedField = 10;

        [Title("Parameter: SdfIcon, IconColor")]
        [LabelText("Heart Icon", SdfIconType.HeartFill, IconColor = "red")]
        public int iconLabel = 100;

        [Title("Parameter: SdfIcon, IconColor")]
        [LabelText("Test", SdfIconType.HeartFill)]
        public int LabelIcon1 = 123;

        [Title("Parameter: SdfIcon, IconColor")]
        [LabelText("", SdfIconType.HeartFill)]
        public int LabelIcon2 = 123;

        public override void AesirInspectorReset()
        {
            customLabel = 1;
            labelFromMember = "The label above is dynamic";
            dynamicLabel = "Dynamic Label Text";
            MyInt3 = 123;
            LabelText = "The label is taken from the number 3 above";
            expressionLabel = "";
            DateTimeLabel = null;
            nicifiedField = 10;
            iconLabel = 100;
            LabelIcon1 = 123;
            LabelIcon2 = 123;
        }
    }
}

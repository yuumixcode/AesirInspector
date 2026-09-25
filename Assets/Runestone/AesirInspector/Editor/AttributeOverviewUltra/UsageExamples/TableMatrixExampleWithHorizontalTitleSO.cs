using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace Runestone.AesirInspector.Editor
{
    [AesirExample]
    public class
        TableMatrixExampleWithHorizontalTitleSO : OdinAttributeExampleSO<
        TableMatrixExampleWithHorizontalTitleSO>
    {
        [OdinSerialize]
        [Title("Parameter: HorizontalTitle, IsReadOnly")]
        [TableMatrix(HorizontalTitle = "Read Only Matrix", IsReadOnly = true)]
        public int[,] readOnlyMatrix = new int[5, 5];

        [OdinSerialize]
        [Title("Parameter: HorizontalTitle, VerticalTitle")]
        [TableMatrix(HorizontalTitle = "X axis", VerticalTitle = "Y axis")]
        public InfoMessageType[,] labeledMatrix = new InfoMessageType[6, 6];

        [Title("Member Reference ($)")]
        public string Title = "Peace, Love & Ducks";

        [Title("Member Reference ($)")]
        public string AlternativeTitle = "Peace, Love & Fenrir";

        [Title("Member Reference ($)")]
        public bool UseAlternativeTitle;

        [OdinSerialize]
        [Title("Member Reference ($)")]
        [TableMatrix(HorizontalTitle = "$Title")]
        public bool[,] fieldNameExample = new bool[5, 5];

        [OdinSerialize]
        [Title("Member Reference ($)")]
        [TableMatrix(HorizontalTitle = "$TitleProperty")]
        public bool[,] propertyNameExample = new bool[5, 5];

        [OdinSerialize]
        [Title("Member Reference ($)")]
        [TableMatrix(HorizontalTitle = "$GetTitle")]
        public bool[,] methodNameExample = new bool[5, 5];

        [OdinSerialize]
        [Title("Expression (@)")]
        [TableMatrix(HorizontalTitle = "@UseAlternativeTitle ? AlternativeTitle : Title")]
        public bool[,] attributeExpressionExample = new bool[5, 5];

        public string TitleProperty => UseAlternativeTitle ? AlternativeTitle : Title;

        string GetTitle() => UseAlternativeTitle ? AlternativeTitle : Title;

        public override void AesirInspectorReset()
        {
            readOnlyMatrix = new int[5, 5];
            labeledMatrix = new InfoMessageType[6, 6];
            Title = "Peace, Love & Ducks";
            AlternativeTitle = "Peace, Love & Fenrir";
            UseAlternativeTitle = false;
            fieldNameExample = new bool[5, 5];
            propertyNameExample = new bool[5, 5];
            methodNameExample = new bool[5, 5];
            attributeExpressionExample = new bool[5, 5];
        }
    }
}

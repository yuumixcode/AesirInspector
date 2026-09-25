using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Required 特性 ErrorMessage 参数的案例 SO。
    /// </summary>
    [AesirExample]
    public class RequiredExampleWithErrorMessageSO : AttributeExampleSO<RequiredExampleWithErrorMessageSO>
    {
        [Title("Member Reference ($)")]
        [InfoBox("Use $ to indicate a member string as message.")]
        public string customError = "My custom error message from field";

        [Title("Member Reference ($)")]
        [Required("$customError")]
        public string referenceExample;

        [Title("Expression (@)")]
        [Required("@$property.NiceName + \" is required!\"")]
        public string expressionExample;

        public override void AesirInspectorReset()
        {
            customError = "My custom error message from field";
            referenceExample = "";
            expressionExample = "";
        }
    }
}

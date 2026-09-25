using Sirenix.OdinInspector;
using UnityEditor;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// EnumPaging 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class EnumPagingExampleSO : AttributeExampleSO<EnumPagingExampleSO>
    {
        public enum SomeEnum
        {
            A,
            B,
            C
        }

        [Title("No Parameters")]
        [EnumPaging]
        public SomeEnum someEnumField;

        [Title("Combining With OnValueChanged")]
        [OnValueChanged("SetCurrentTool")]
        [InfoBox("Changing this property will change the current selected tool in the Unity editor.")]
        [EnumPaging]
        public Tool sceneTool;

        void SetCurrentTool()
        {
            Tools.current = sceneTool;
        }

        public override void AesirInspectorReset()
        {
            someEnumField = SomeEnum.A;
            sceneTool = Tool.View;
        }
    }
}

using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// OnStateUpdate 特性案例。
    /// </summary>
    [AesirExample]
    internal class OnStateUpdateExampleSO : AttributeExampleSO<OnStateUpdateExampleSO>
    {
        [Title("Expression (@)")]
        [OnStateUpdate("@#(list).State.Expanded = $value")]
        public bool ExpandList;

        [Title("Expression (@)")]
        [OnStateUpdate("@$property.State.Expanded = ExpandList")]
        public List<string> list;

        [Title("Expression (@)")]
        public bool ToggleMyInt;

        [Title("Expression (@)")]
        [OnStateUpdate("@$property.State.Visible = ToggleMyInt")]
        public int MyInt;

        [Title("Expression (@)")]
        public bool ToggleField = true;

        [Title("Expression (@)")]
        [OnStateUpdate("@$property.State.Visible = ToggleField")]
        public string VisibleIfToggled = "Hello";

        [Title("Parameter: Action (InspectorProperty property)")]
        [OnStateUpdate("UpdateState")]
        public int DisabledIfZero = 1;

        void UpdateState(InspectorProperty property)
        {
            property.State.Enabled = DisabledIfZero != 0;
        }

        public override void AesirInspectorReset()
        {
            ExpandList = false;
            list = null;
            ToggleMyInt = false;
            MyInt = 0;
            ToggleField = true;
            VisibleIfToggled = "Hello";
            DisabledIfZero = 1;
        }
    }
}

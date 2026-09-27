using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HideReferenceObjectPicker 特性的案例 SO。
    /// 字段类型为自定义引用类型（非 Unity 序列化），Unity 无法序列化，必须走 Odin 序列化。
    /// </summary>
    [AesirExample]
    public class HideReferenceObjectPickerExampleSO : OdinAttributeExampleSO<HideReferenceObjectPickerExampleSO>
    {
        [Title("Hidden Object Pickers")]
        [HideReferenceObjectPicker]
        public MyCustomReferenceType hiddenPickerOne = new MyCustomReferenceType();

        [HideReferenceObjectPicker]
        public MyCustomReferenceType hiddenPickerTwo = new MyCustomReferenceType();

        [Title("Shown Object Pickers (Default)")]
        public MyCustomReferenceType shownPickerOne = new MyCustomReferenceType();

        public MyCustomReferenceType shownPickerTwo = new MyCustomReferenceType();

        public override void AesirInspectorReset()
        {
            hiddenPickerOne = new MyCustomReferenceType();
            hiddenPickerTwo = new MyCustomReferenceType();
            shownPickerOne = new MyCustomReferenceType();
            shownPickerTwo = new MyCustomReferenceType();
        }

        public class MyCustomReferenceType
        {
            public int A;

            public int B;

            public int C;
        }
    }
}

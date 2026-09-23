using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// SuffixLabel 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class SuffixLabelExampleSO : AttributeExampleSO<SuffixLabelExampleSO>
    {
        [Title("No Parameters")]
        [SuffixLabel("Unit: meters")]
        public float distance;

        [Title("No Parameters")]
        [SuffixLabel("Prefab", false)]
        public GameObject GameObject;

        [Title("Parameter: Overlay")]
        [SuffixLabel("Percentage", true)]
        public float progress = 0.5f;

        [Title("Parameter: Overlay")]
        [SuffixLabel("ms", false, Overlay = true)]
        public float Speed;

        [Title("Parameter: Overlay")]
        [SuffixLabel("radians", false, Overlay = true)]
        public float Angle;

        [Title("Member Reference ($)")]
        [SuffixLabel("$_dynamicLabel")]
        public string dynamicProperty = "Hello";

        [Title("Member Reference ($)")]
        public string _dynamicLabel = "Dynamic Suffix";

        [Title("Member Reference ($)")]
        [SuffixLabel("$Suffix", false, Overlay = true)]
        public string Suffix = "Dynamic suffix label";

        [Title("Expression (@)")]
        [SuffixLabel("@\"Current Length: \" + (dynamicProperty == null ? 0 : dynamicProperty.Length)")]
        public string expressionProperty = "World";

        [Title("Expression (@)")]
        [SuffixLabel("@DateTime.Now.ToString(\"HH:mm:ss\")", true)]
        public string Expression;

        [Title("Parameter: Icon")]
        [SuffixLabel("Suffix with icon", SdfIconType.HeartFill, false)]
        public string IconAndText1;

        [Title("Parameter: Icon")]
        [SuffixLabel(SdfIconType.HeartFill)]
        public string OnlyIcon1;

        [Title("Parameter: Icon")]
        [SuffixLabel("Suffix with icon", SdfIconType.HeartFill, false, Overlay = true)]
        public string IconAndText2;

        [Title("Parameter: Icon")]
        [SuffixLabel(SdfIconType.HeartFill, Overlay = true)]
        public string OnlyIcon2;

        public override void AesirInspectorReset()
        {
            distance = 0;
            GameObject = null;
            progress = 0.5f;
            Speed = 0;
            Angle = 0;
            dynamicProperty = "Hello";
            _dynamicLabel = "Dynamic Suffix";
            Suffix = "Dynamic suffix label";
            expressionProperty = "World";
            Expression = null;
            IconAndText1 = null;
            OnlyIcon1 = null;
            IconAndText2 = null;
            OnlyIcon2 = null;
        }
    }
}

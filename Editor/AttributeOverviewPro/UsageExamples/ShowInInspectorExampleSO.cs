using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ShowInInspector 特性案例。
    /// </summary>
    [AesirExample]
    internal class ShowInInspectorExampleSO : AttributeExampleSO<ShowInInspectorExampleSO>
    {
        [Title("Usage with Private Fields")]
        [ShowInInspector]
        private int myPrivateInt;

        [Title("Usage with Properties")]
        [ShowInInspector]
        public int MyPropertyInt { get; set; }

        [Title("Usage with Properties")]
        [ShowInInspector]
        public int ReadOnlyProperty => myPrivateInt;

        [Title("Usage with Static Members")]
        [ShowInInspector]
        public static bool StaticProperty { get; set; }

        [Title("Usage with Serialized Backing Field")]
        [SerializeField]
        [HideInInspector]
        private int evenNumber;

        [Title("Usage with Serialized Backing Field")]
        [ShowInInspector]
        public int EvenNumber
        {
            get
            {
                return evenNumber;
            }
            set
            {
                evenNumber = value - value % 2;
            }
        }

        public override void AesirInspectorReset()
        {
            myPrivateInt = 0;
            MyPropertyInt = 0;
            evenNumber = 0;
        }
    }
}

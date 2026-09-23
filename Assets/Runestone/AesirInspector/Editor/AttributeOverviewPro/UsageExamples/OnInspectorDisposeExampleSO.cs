using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// OnInspectorDispose 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class OnInspectorDisposeExampleSO : AttributeExampleSO<OnInspectorDisposeExampleSO>
    {
        [Title("No Parameters")]
        [InfoBox("Methods marked with OnInspectorDispose are hidden in the inspector. The method body runs when the property setup is disposed, for example when this example is deselected.")]
        [OnInspectorDispose]
        void DisposeByMethod()
        {
            Debug.Log("OnInspectorDispose invoked via method");
        }

        [Title("Parameter: Action (Expression)")]
        [OnInspectorDispose("@Debug.Log(\"OnInspectorDispose invoked\", this)")]
        public string expressionField = "OnInspectorDispose trigger";

        [Title("Parameter: Action (Polymorphic Field)")]
        [OnInspectorDispose("@UnityEngine.Debug.Log(\"Dispose event invoked!\")")]
        [ShowInInspector]
        [InfoBox("When you change the type of this field, or set it to null, the former property setup is disposed. The property setup will also be disposed when you deselect this example.", InfoMessageType.Info)]
        [DisplayAsString]
        public BaseClass PolymorphicField;

        [Serializable]
        public abstract class BaseClass
        {
            public override string ToString()
            {
                return GetType().Name;
            }
        }

        [Serializable]
        public class A : BaseClass
        {
        }

        [Serializable]
        public class B : BaseClass
        {
        }

        [Serializable]
        public class C : BaseClass
        {
        }

        public override void AesirInspectorReset()
        {
            expressionField = "OnInspectorDispose trigger";
            PolymorphicField = null;
        }
    }
}

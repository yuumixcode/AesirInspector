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
        [Title("Parameter: Action (Method Name)")]
        [OnInspectorDispose("DisposeByNamedMethod")]
        public string methodNameExample;

        [Title("Parameter: Action (Expression)")]
        [OnInspectorDispose("@Debug.Log(\"OnInspectorDispose invoked\", this)")]
        public string expressionField = "OnInspectorDispose trigger";

        [Title("Parameter: Action (Expression) (Member Reference)")]
        public string disposeMessage = "Dispose action reading a member via expression";

        [Title("Parameter: Action (Expression) (Member Reference)")]
        [OnInspectorDispose("@Debug.Log(disposeMessage, this)")]
        public string memberReferenceExample;

        [Title("Parameter: Action (Expression) (Polymorphic Field)")]
        [OnInspectorDispose("@UnityEngine.Debug.Log(\"Dispose event invoked!\")")]
        [ShowInInspector]
        [InfoBox(
            "When you change the type of this field, or set it to null, the former property setup is disposed. The property setup will also be disposed when you deselect this example.")]
        [DisplayAsString]
        public BaseClass PolymorphicField;

        [Title("No Parameters")]
        [InfoBox(
            "Methods marked with OnInspectorDispose are hidden in the inspector. The method body runs when the property setup is disposed, for example when this example is deselected.")]
        [OnInspectorDispose]
        void DisposeByMethod()
        {
            Debug.Log("OnInspectorDispose invoked via method");
        }

        void DisposeByNamedMethod()
        {
            Debug.Log("OnInspectorDispose invoked via named method");
        }

        public override void AesirInspectorReset()
        {
            methodNameExample = string.Empty;
            expressionField = "OnInspectorDispose trigger";
            disposeMessage = "Dispose action reading a member via expression";
            memberReferenceExample = string.Empty;
            PolymorphicField = null;
        }

        [Serializable]
        public abstract class BaseClass
        {
            public override string ToString() => GetType().Name;
        }

        [Serializable]
        public class A : BaseClass { }

        [Serializable]
        public class B : BaseClass { }

        [Serializable]
        public class C : BaseClass { }
    }
}

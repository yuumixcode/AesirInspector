using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// OnInspectorDispose 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class OnInspectorDisposeExampleWithActionSO : AttributeExampleSO<OnInspectorDisposeExampleWithActionSO>
    {
        [Title("Parameter: Action (Method Name)")]
        [OnInspectorDispose("DisposeByNamedMethod")]
        public string methodNameExample;

        [Title("Expression (@) (Member Reference)")]
        public string disposeMessage = "Dispose action reading a member via expression";

        [Title("Expression (@) (Member Reference)")]
        [OnInspectorDispose("@Debug.Log(disposeMessage, this)")]
        public string memberReferenceExample;

        [Title("Expression (@)")]
        [OnInspectorDispose("@Debug.Log(\"OnInspectorDispose invoked via expression\", this)")]
        public string expressionExample;

        void DisposeByNamedMethod()
        {
            Debug.Log("OnInspectorDispose invoked via named method");
        }

        public override void AesirInspectorReset()
        {
            methodNameExample = string.Empty;
            disposeMessage = "Dispose action reading a member via expression";
            memberReferenceExample = string.Empty;
            expressionExample = string.Empty;
        }
    }
}

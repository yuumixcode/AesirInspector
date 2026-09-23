using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// OnInspectorInit 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class OnInspectorInitExampleWithActionSO : AttributeExampleSO<OnInspectorInitExampleWithActionSO>
    {
        [Title("Parameter: Action (Method Name)")]
        [OnInspectorInit("InitializeByNamedMethod")]
        public string methodNameExample;

        [Title("Expression (@) (Member Reference)")]
        public string initMessage = "Init action reading a member via expression";

        [Title("Expression (@) (Member Reference)")]
        [OnInspectorInit("@Debug.Log(initMessage, this)")]
        public string memberReferenceExample;

        [Title("Expression (@)")]
        [OnInspectorInit("@TimeWhenExampleWasOpened = DateTime.Now.ToString()")]
        public string TimeWhenExampleWasOpened;

        [Title("Expression (@)")]
        [OnInspectorInit("@Debug.Log(\"OnInspectorInit Action invoked via expression\", this)")]
        public string expressionExample;

        void InitializeByNamedMethod()
        {
            methodNameExample = "Initialized by named method: " + DateTime.Now.ToString();
        }

        public override void AesirInspectorReset()
        {
            methodNameExample = string.Empty;
            initMessage = "Init action reading a member via expression";
            memberReferenceExample = string.Empty;
            TimeWhenExampleWasOpened = string.Empty;
            expressionExample = string.Empty;
        }
    }
}

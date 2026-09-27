using System;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// OnInspectorInit 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class OnInspectorInitExampleSO : AttributeExampleSO<OnInspectorInitExampleSO>
    {
        [Title("Parameter: Action (Method Name)")]
        [OnInspectorInit(nameof(InitializeMethod))]
        public string methodNameField;

        [Title("Parameter: Action (Expression)")]
        [OnInspectorInit("@fieldSetByExpression = \"Set by expression on init\"")]
        public string fieldSetByExpression;

        [Title("Parameter: Action (Expression)")]
        [OnInspectorInit("@TimeWhenExampleWasOpened = DateTime.Now.ToString()")]
        public string TimeWhenExampleWasOpened;

        [Title("Parameter: Action (Expression) (Member Reference)")]
        public string initMessage = "Init action reading a member via expression";

        [Title("Parameter: Action (Expression) (Member Reference)")]
        [OnInspectorInit("@Debug.Log(initMessage, this)")]
        public string memberReferenceExample;

        [FoldoutGroup("Delayed Initialization", Expanded = false, HideWhenChildrenAreInvisible = false)]
        [OnInspectorInit("@TimeFoldoutWasOpened = DateTime.Now.ToString()")]
        public string TimeFoldoutWasOpened;

        [Title("Current Time")]
        [ShowInInspector]
        [DisplayAsString]
        [PropertyOrder(-1f)]
        public string CurrentTime
        {
            get
            {
                GUIHelper.RequestRepaint();
                return DateTime.Now.ToString();
            }
        }

        [Title("No Parameters")]
        [OnInspectorInit]
        void InitializeWithoutParameters()
        {
            Debug.Log("OnInspectorInit invoked without parameters");
        }

        void InitializeMethod()
        {
            Debug.Log("OnInspectorInit: Initialize method called");
        }

        public override void AesirInspectorReset()
        {
            methodNameField = null;
            fieldSetByExpression = null;
            TimeWhenExampleWasOpened = null;
            initMessage = "Init action reading a member via expression";
            memberReferenceExample = null;
            TimeFoldoutWasOpened = null;
        }
    }
}

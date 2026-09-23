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

        [Title("Parameter: Action (Method Name)")]
        [OnInspectorInit(nameof(InitializeMethod))]
        public string methodNameField;

        [Title("Parameter: Action (Expression)")]
        [OnInspectorInit("@fieldSetByExpression = \"Set by expression on init\"")]
        public string fieldSetByExpression;

        [Title("Parameter: Action (Expression)")]
        [OnInspectorInit("@TimeWhenExampleWasOpened = DateTime.Now.ToString()")]
        public string TimeWhenExampleWasOpened;

        [FoldoutGroup("Delayed Initialization", 0f, Expanded = false, HideWhenChildrenAreInvisible = false)]
        [OnInspectorInit("@TimeFoldoutWasOpened = DateTime.Now.ToString()")]
        public string TimeFoldoutWasOpened;

        void InitializeMethod()
        {
            Debug.Log("OnInspectorInit: Initialize method called");
        }

        public override void AesirInspectorReset()
        {
            methodNameField = null;
            fieldSetByExpression = null;
            TimeWhenExampleWasOpened = null;
            TimeFoldoutWasOpened = null;
        }
    }
}

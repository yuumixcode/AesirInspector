using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.BankPrototype
{
    /// <summary>
    /// 面板宿主原型：完全复刻 AbstractAttributePanelSO 的示例预览形态——
    /// [InlineEditor(Hidden)] 字段 + OnInspectorInit 从银行恢复的内存实例。
    /// 注意 currentSelectedExample 不序列化（状态走银行，字段只活在内存）。
    /// </summary>
    public class BankPrototypePanelHostSO : SerializedScriptableObject
    {
        const string BankPath = "Assets/Prototype/AttributeBankPrototype/TestBank.asset";

        [NonSerialized]
        [ShowInInspector]
        [HideLabel]
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [PropertyOrder(0)]
        public ScriptableObject currentSelectedExample;

        [ShowInInspector]
        [HideIf(nameof(NoExampleSelected))]
        [PropertyOrder(-10)]
        [OnInspectorGUI]
        void DrawExampleTabButtons()
        {
            SirenixEditorGUI.BeginHorizontalToolbar();
            if (SirenixEditorGUI.ToolbarButton(new GUIContent("Unity 示例")) &&
                currentSelectedExample is not ProtoUnityExampleSO)
            {
                currentSelectedExample = Restore("Demo/ProtoUnityExampleSO", typeof(ProtoUnityExampleSO));
            }

            if (SirenixEditorGUI.ToolbarButton(new GUIContent("Odin 示例")) &&
                currentSelectedExample is not ProtoOdinExampleSO)
            {
                currentSelectedExample = Restore("Demo/ProtoOdinExampleSO", typeof(ProtoOdinExampleSO));
            }

            SirenixEditorGUI.EndHorizontalToolbar();
        }

        bool NoExampleSelected => currentSelectedExample == null;

        [OnInspectorInit]
        void InitializeExample()
        {
            currentSelectedExample = Restore("Demo/ProtoUnityExampleSO", typeof(ProtoUnityExampleSO));
        }

        static ScriptableObject Restore(string key, System.Type type)
        {
            var bank = AssetDatabase.LoadAssetAtPath<ExampleStateBankSO>(BankPath);
            if (bank == null)
            {
                bank = ScriptableObject.CreateInstance<ExampleStateBankSO>();
                AssetDatabase.CreateAsset(bank, BankPath);
            }

            return bank.RestoreOrCreate(key, type);
        }
    }

    /// <summary>
    /// InlineEditor 验证窗口：画宿主（宿主本身也是内存实例），观察嵌套编辑是否正常。
    /// </summary>
    public class BankPrototypePanelWindow : OdinEditorWindow
    {
        public static bool DrawnWithoutException;
        public static int DrawnFrameCount;
        public static string DrawException;

        BankPrototypePanelHostSO _host;

        [MenuItem("Tools/Bank Prototype/Open Panel Host Window")]
        static void Open()
        {
            var window = GetWindow<BankPrototypePanelWindow>("Bank Panel Host");
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(600, 500);
            window.Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            DrawnWithoutException = false;
            DrawnFrameCount = 0;
            DrawException = null;
            _host = ScriptableObject.CreateInstance<BankPrototypePanelHostSO>();
        }

        protected override void DrawEditor(int index)
        {
            try
            {
                base.DrawEditor(index);
                DrawnFrameCount++;
                DrawnWithoutException = DrawException == null;
            }
            catch (System.Exception e)
            {
                DrawException = e.ToString();
                throw;
            }
        }

        protected override void OnDestroy()
        {
            var bank = AssetDatabase.LoadAssetAtPath<ExampleStateBankSO>("Assets/Prototype/AttributeBankPrototype/TestBank.asset");
            if (bank != null && _host?.currentSelectedExample != null)
            {
                bank.SaveState("Demo/" + _host.currentSelectedExample.GetType().Name, _host.currentSelectedExample);
                AssetDatabase.SaveAssets();
            }

            base.OnDestroy();
        }

        protected override IEnumerable<object> GetTargets()
        {
            yield return _host;
        }
    }
}

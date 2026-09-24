using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.BankPrototype
{
    /// <summary>
    /// 原型演示窗口：验证"内存实例 + 银行持久化"的完整用户体验闭环。
    /// 打开窗口 → 左侧菜单选择示例 → 右侧真实交互调试 → 关闭窗口自动快照。
    /// 重启编辑器后再打开，上次调试的示例状态原样恢复。
    /// </summary>
    public class BankPrototypeWindow : OdinMenuEditorWindow
    {
        const string BankPath = "Assets/Prototype/AttributeBankPrototype/TestBank.asset";

        ExampleStateBankSO _bank;
        ProtoUnityExampleSO _unityExample;
        ProtoOdinExampleSO _odinExample;
        ProtoButtonExampleSO _buttonExample;

        [MenuItem("Tools/Bank Prototype/Open Demo Window")]
        static void Open()
        {
            var window = GetWindow<BankPrototypeWindow>("Bank Prototype Demo");
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
            window.Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _bank = AssetDatabase.LoadAssetAtPath<ExampleStateBankSO>(BankPath);
            if (_bank == null)
            {
                _bank = ScriptableObject.CreateInstance<ExampleStateBankSO>();
                AssetDatabase.CreateAsset(_bank, BankPath);
            }

            // 恢复或创建 —— 这就是目标 API 的最终形态
            _unityExample = (ProtoUnityExampleSO)_bank.RestoreOrCreate("Demo/ProtoUnityExampleSO",
                typeof(ProtoUnityExampleSO));
            _odinExample = (ProtoOdinExampleSO)_bank.RestoreOrCreate("Demo/ProtoOdinExampleSO",
                typeof(ProtoOdinExampleSO));
            _buttonExample = (ProtoButtonExampleSO)_bank.RestoreOrCreate("Demo/ProtoButtonExampleSO",
                typeof(ProtoButtonExampleSO));
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree
            {
                Config = { DrawSearchToolbar = false }
            };
            tree.Add("Unity 序列化示例", _unityExample);
            tree.Add("Odin 序列化示例", _odinExample);
            tree.Add("Button 示例", _buttonExample);
            return tree;
        }

        protected override void OnDestroy()
        {
            // 关窗自动快照 —— 用户调试状态在无感知中持久化
            if (_bank != null)
            {
                _bank.SaveState("Demo/ProtoUnityExampleSO", _unityExample);
                _bank.SaveState("Demo/ProtoOdinExampleSO", _odinExample);
                _bank.SaveState("Demo/ProtoButtonExampleSO", _buttonExample);
                AssetDatabase.SaveAssets();
            }

            base.OnDestroy();
        }
    }
}

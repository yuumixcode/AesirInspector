using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Examples;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HideMonoScript 特性的案例 SO：对比标注与未标注类型顶部的 Script 字段。
    /// 内联绘制（本面板的案例预览）会统一隐藏 Script 字段，因此差异需要在对象的 Inspector 窗口中查看。
    /// </summary>
    [AesirExample]
    public class HideMonoScriptExampleSO : AttributeExampleSO<HideMonoScriptExampleSO>
    {
        [InfoBox(
            "两个对象的字段完全相同，只有类型上是否标注 [HideMonoScript] 不同。\n内联预览不绘制 Script 字段，点击下方按钮打开对象的 Inspector 窗口即可对比：标注过的类型顶部没有 Script 字段，未标注的有。")]
        public HideMonoScriptDemoObject hiddenScriptObject;

        public ShowMonoScriptDemoObject shownScriptObject;

        [Title("Open Inspector")]
        [Button("Open Hidden Inspector (has [HideMonoScript])")]
        void OpenHiddenInspector()
        {
            GUIHelper.OpenInspectorWindow(hiddenScriptObject);
        }

        [Button("Open Shown Inspector")]
        void OpenShownInspector()
        {
            GUIHelper.OpenInspectorWindow(shownScriptObject);
        }

        [OnInspectorInit]
        void CreateData()
        {
            hiddenScriptObject = ExampleHelper.GetScriptableObject<HideMonoScriptDemoObject>("Hidden");
            shownScriptObject = ExampleHelper.GetScriptableObject<ShowMonoScriptDemoObject>("Shown");
        }

        [OnInspectorDispose]
        void CleanupData()
        {
            if (hiddenScriptObject != null)
            {
                DestroyImmediate(hiddenScriptObject);
            }

            if (shownScriptObject != null)
            {
                DestroyImmediate(shownScriptObject);
            }
        }

        public override void AesirInspectorReset()
        {
            CreateData();
        }
    }
}

using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HideMonoScript 案例用的对象类型：标注 [HideMonoScript]，Inspector 顶部不绘制 Script 字段。
    /// </summary>
    [HideMonoScript]
    public class HideMonoScriptDemoObject : ScriptableObject
    {
        public string Value = "Sample field";
    }
}

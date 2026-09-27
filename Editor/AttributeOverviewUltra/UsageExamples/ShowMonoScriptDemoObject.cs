using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HideMonoScript 案例的对照对象类型：未标注特性，Inspector 顶部保留 Script 字段。
    /// </summary>
    public class ShowMonoScriptDemoObject : ScriptableObject
    {
        public string Value = "Sample field";
    }
}

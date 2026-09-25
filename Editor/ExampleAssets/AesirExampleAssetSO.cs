using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Attribute Overview 示例使用的占位 ScriptableObject 资产类型。
    /// 让 AssetSelector 等必须指向真实资产的案例有稳定落点；本类型与 ExampleAssets 目录
    /// 均位于 Editor 目录内，随包分发但不会进入构建。
    /// </summary>
    public class AesirExampleAssetSO : ScriptableObject
    {
    }
}

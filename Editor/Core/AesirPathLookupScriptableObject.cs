using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Aesir Inspector 包位置的锚点资产类型。
    /// 资产 <c>Editor/ExampleAssets/AesirPathLookup.asset</c> 使用固定 GUID，
    /// 因此可以在不依赖绝对路径的前提下定位包的安装位置（Assets 或 Packages），
    /// 机制与 Odin 的 <c>SirenixPathLookupScriptableObject</c> 相同。
    /// </summary>
    public class AesirPathLookupScriptableObject : ScriptableObject
    {
    }
}

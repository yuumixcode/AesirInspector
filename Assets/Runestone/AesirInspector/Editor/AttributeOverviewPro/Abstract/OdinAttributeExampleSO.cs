using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Odin 序列化的特性案例 SO 泛型抽象基类，提供内存单例模式。
    /// 单例经 UltraStateBankSO 银行路由：有用户调试状态快照则恢复，否则全新默认实例，全程零资产写操作。
    /// </summary>
    public abstract class OdinAttributeExampleSO<T> : SerializedScriptableObject, IAesirInspectorReset
        where T : OdinAttributeExampleSO<T>
    {
        static T _asset;

        /// <summary>
        /// 获取内存单例实例，状态由 Ultra 状态银行按需恢复。
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_asset)
                {
                    return _asset;
                }

                _asset = UltraStateBankSO.GetMemoryExample<T>();
                return _asset;
            }
        }

        /// <summary>
        /// 重置案例数据到初始状态。
        /// </summary>
        public abstract void AesirInspectorReset();
    }
}

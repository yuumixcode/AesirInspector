using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Unity 原生序列化的特性案例 SO 泛型抽象基类，提供内存单例模式。
    /// 单例经 UltraStateStoreSO 状态存储路由：有用户数据快照则恢复，否则全新默认实例，全程零资产写操作。
    /// </summary>
    public abstract class AttributeExampleSO<T> : ScriptableObject, IAesirInspectorReset
        where T : AttributeExampleSO<T>
    {
        static T _asset;

        /// <summary>
        /// 获取内存单例实例，数据由 Attribute Overview Ultra 状态存储按需恢复。
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_asset)
                {
                    return _asset;
                }

                _asset = UltraStateStoreSO.GetMemoryExample<T>();
                return _asset;
            }
        }

        /// <summary>
        /// 重置案例数据到初始状态。
        /// </summary>
        public abstract void AesirInspectorReset();
    }
}

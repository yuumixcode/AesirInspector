using UnityEngine;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// Aesir Inspector 的编辑器单例设置基类。
    /// 自动处理资源的获取、创建以及在 EditorBuildSettings 中的注册。
    /// </summary>
    /// <typeparam name="T">设置项类型</typeparam>
    public abstract class AesirInspectorSettings<T> : ScriptableObject where T : AesirInspectorSettings<T>
    {
        /// <summary>更早版本使用的命名空间前缀（当时配置键等于类型全名）。</summary>
        const string LegacyNamespacePrefix = "RunLab.AesirInspector.";

        static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                var type = typeof(T);
                var assetName = type.Name;
                // 配置键使用与命名空间无关的稳定字符串：否则命名空间调整会让已注册的配置对象失效，
                // 并且每次解析都要重新写一遍 ProjectSettings。
                var configName = ScriptableObjectSafeEditorUtility.GetEditorConfigKey(assetName);

                _instance = ScriptableObjectSafeEditorUtility.GetOrCreateEditorScriptableObject<T>(configName,
                    AesirInspectorPaths.PreferencesAssetsFolderPath, assetName,
                    type.FullName, LegacyNamespacePrefix + assetName);

                return _instance;
            }
        }
    }
}

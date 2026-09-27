using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// 把 AssetSelector 示例中的包内硬编码路径替换为运行时解析出的真实路径，
    /// 使示例在 Assets 安装与 UPM（Packages）安装下都指向包内真实的 ExampleAssets 目录。
    /// <c>AssetSelectorAttribute.Paths</c> 只接受字面量、也不做 $/@ 解析，因此在这里改写其 SearchInFolders 字段。
    /// </summary>
    internal sealed class AesirExampleAssetSelectorProcessor : OdinAttributeProcessor<AssetSelectorExampleSO>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member,
            List<Attribute> attributes)
        {
            if (!AesirPackagePaths.IsResolved)
            {
                // 解析失败时保留示例源码中的字面量路径，由 AssetSelector 自行处理。
                return;
            }

            var folders = member.Name switch
            {
                nameof(AssetSelectorExampleSO.scriptableObjectsFromFolder) => new[]
                {
                    AesirPackagePaths.ScriptableObjectsPath
                },
                nameof(AssetSelectorExampleSO.scriptableObjectsFromMultipleFolders) => new[]
                {
                    AesirPackagePaths.MaterialsPrimaryPath,
                    AesirPackagePaths.MaterialsSecondaryPath
                },
                _ => null
            };

            if (folders == null)
            {
                return;
            }

            for (var i = 0; i < attributes.Count; i++)
            {
                if (attributes[i] is not AssetSelectorAttribute)
                {
                    continue;
                }

                // 这两个字段只设置了 Paths，替换实例即可；SearchInFolders 是 AssetSelectorAttribute 的公开字段，
                // AssetSelectorAttributeDrawer 每次打开下拉时都会重新读取它。
                attributes[i] = new AssetSelectorAttribute { SearchInFolders = folders };
                return;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// 剥离 Odin 从「示例类型定义」传播到面板示例宿主字段上的类级特性。
    /// Odin 的 TypeDefinitionAttributeProcessor（优先级 1000）会把属性值类型上的类级特性加到属性自身；
    /// 示例宿主字段（AbstractAttributePanelSO.currentSelectedExample）位于面板的属性树中，
    /// 传播过来的类级特性会以「面板」为上下文解析成员引用（如 [Image("typeBanner")] 在面板上找不到 typeBanner），
    /// 从而画出错误框。示例自身的内联编辑器属性树不受影响，仍会正常绘制这些类级特性。
    /// 本处理器使用默认优先级 0，晚于 TypeDefinitionAttributeProcessor 执行，因此可以安全移除。
    /// </summary>
    internal sealed class ExampleHostAttributeProcessor : OdinAttributeProcessor
    {
        /// <summary>AbstractAttributePanelSO 中承载当前示例的字段名。</summary>
        const string HostMemberName = "currentSelectedExample";

        static readonly HashSet<Type> EmptyTypes = new HashSet<Type>();

        static readonly Dictionary<MemberInfo, HashSet<Type>> MemberAttributeTypes =
            new Dictionary<MemberInfo, HashSet<Type>>();

        static readonly Dictionary<Type, HashSet<Type>> ValueTypeAttributeTypes =
            new Dictionary<Type, HashSet<Type>>();

        public override bool CanProcessSelfAttributes(InspectorProperty property)
        {
            if (property.ValueEntry == null)
            {
                return false;
            }

            var member = property.Info.GetMemberInfo();
            return member != null
                && member.Name == HostMemberName
                && member.DeclaringType == typeof(AbstractAttributePanelSO);
        }

        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            var propagatedTypes = GetValueTypeAttributeTypes(property.ValueEntry.TypeOfValue);
            if (propagatedTypes.Count == 0)
            {
                return;
            }

            // 宿主字段自己声明的同名特性保留，只移除由值类型定义传播来的那一份。
            var declaredTypes = GetMemberAttributeTypes(property.Info.GetMemberInfo());
            attributes.RemoveAll(attribute =>
                propagatedTypes.Contains(attribute.GetType()) && !declaredTypes.Contains(attribute.GetType()));
        }

        /// <summary>获取值类型自身（含基类）声明的特性类型集合，即 Odin 会传播到属性上的那些特性。</summary>
        static HashSet<Type> GetValueTypeAttributeTypes(Type valueType)
        {
            if (valueType == null)
            {
                return EmptyTypes;
            }

            if (ValueTypeAttributeTypes.TryGetValue(valueType, out var result))
            {
                return result;
            }

            result = new HashSet<Type>();
            for (var current = valueType; current != null && current != typeof(object); current = current.BaseType)
            {
                foreach (var attribute in current.GetCustomAttributes(false))
                {
                    result.Add(attribute.GetType());
                }
            }

            ValueTypeAttributeTypes[valueType] = result;
            return result;
        }

        /// <summary>获取成员自身声明的特性类型集合。</summary>
        static HashSet<Type> GetMemberAttributeTypes(MemberInfo member)
        {
            if (member == null)
            {
                return EmptyTypes;
            }

            if (MemberAttributeTypes.TryGetValue(member, out var result))
            {
                return result;
            }

            result = new HashSet<Type>();
            foreach (var attribute in member.GetCustomAttributes(true))
            {
                result.Add(attribute.GetType());
            }

            MemberAttributeTypes[member] = result;
            return result;
        }
    }
}

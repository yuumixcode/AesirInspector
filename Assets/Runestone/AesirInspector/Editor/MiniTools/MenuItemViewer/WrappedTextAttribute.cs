using System;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// 以自动换行的只读文本绘制字符串。Odin 自带的 DisplayAsString 在宽度不足时直接截断
    /// （不换行、也不加省略号），长内容会静默丢失尾部，这里改为按可用宽度换行。
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class WrappedTextAttribute : Attribute
    {
        /// <summary>
        /// 行内标签宽度
        /// </summary>
        public float LabelWidth { get; set; } = 76f;

        /// <summary>
        /// 文本字号
        /// </summary>
        public int FontSize { get; set; } = 12;
    }

    public sealed class WrappedTextAttributeDrawer : OdinAttributeDrawer<WrappedTextAttribute, string>
    {
        GUIStyle _textStyle;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            _textStyle ??= new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                richText = true,
                fontSize = base.Attribute.FontSize
            };

            var text = ValueEntry.SmartValue ?? string.Empty;
            if (label == null)
            {
                GUILayout.Label(text, _textStyle);
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(label, EditorStyles.label, GUILayout.Width(base.Attribute.LabelWidth));
            GUILayout.Label(text, _textStyle);
            GUILayout.EndHorizontal();
        }
    }
}

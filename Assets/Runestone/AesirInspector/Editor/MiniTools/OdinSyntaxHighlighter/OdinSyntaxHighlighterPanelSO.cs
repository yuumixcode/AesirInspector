using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Odin 语法高亮处理器可视化面板，基于 OdinCodeHighlighter 提供语法高亮测试功能
    /// </summary>
    public class OdinSyntaxHighlighterPanelSO : ScriptableObject
    {
        /// <summary>
        /// EditorBuildSettings 存储引用的 Key
        /// </summary>
        static readonly string ConfigName = typeof(OdinSyntaxHighlighterPanelSO).GetNiceFullName();

        [PropertyOrder(-100)]
        public BilingualHeaderControl bilingualHeader;

        [BilingualTitle("源码示例", "Source Code Example")]
        [HideLabel]
        [TextArea(10, 15)]
        public string exampleSourceCode = @"using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example : ScriptableObject
{
    public int ExampleInt;
    public float ExampleFloat;
    public string ExampleString;
    public bool ExampleBool;
    public Vector3 ExampleVector3;
    public Color ExampleColor;
    public GameObject ExampleGameObject;
    public List<int> ExampleList;
    public Dictionary<string, int> ExampleDictionary; 
}";

        [PropertyOrder(-5)]
        public BilingualDisplayAsStringControl firstTip;

        [PropertyOrder(-5)]
        public BilingualDisplayAsStringControl secondTip;

        [PropertyOrder(-5)]
        public BilingualDisplayAsStringControl thirdTip;

        [PropertyOrder(-5)]
        public BilingualDisplayAsStringControl fourthTip;

        /// <summary>
        /// 获取 OdinSyntaxHighlighterPanelSO 单例
        /// </summary>
        public static OdinSyntaxHighlighterPanelSO Instance =>
            ScriptableObjectSafeEditorUtility.GetOrCreateEditorScriptableObject<OdinSyntaxHighlighterPanelSO>(
                ConfigName, AesirInspectorPaths.MiniToolsAssetsFolderPath, "OdinSyntaxHighlighter");

        void OnEnable()
        {
            bilingualHeader = new BilingualHeaderControl("语法高亮处理器", "Syntax Highlighter",
                "获取 Odin 的语法高亮富文本，用于自定义代码展示。",
                "Get Odin's syntax-highlighted rich text for custom code display.");
            firstTip = new BilingualDisplayAsStringControl("1.不能包含 namespace 声明。",
                "1.No namespace declarations.");
            secondTip = new BilingualDisplayAsStringControl("2.不能包含 $ 内插字符串。",
                "2.No $ interpolated strings.");
            thirdTip = new BilingualDisplayAsStringControl("3.源码需预先格式化，保证合理的空格。",
                "3.Source must be pre-formatted with proper spacing.");
            fourthTip = new BilingualDisplayAsStringControl("4.注意富文本标签的使用，高亮失效时优先排查此项。",
                "4.Mind rich-text tags; check them first when highlighting fails.");
        }

        /// <summary>
        /// 使用富文本标记进行脚本语法高亮。委托给 OdinCodeHighlighter 实现。
        /// </summary>
        public static string ApplyCodeHighlighting(string code) =>
            OdinCodeHighlighter.ApplyHighlighting(code);

        [PropertySpace(10)]
        [BilingualInfoBox("查看 Console 窗口输出", "See Console Window Output")]
        [BilingualInfoBox("使用 OdinSyntaxHighlighterPanelSO.ApplyCodeHighlighting(sourceCode) 处理源代码",
            "Use OdinSyntaxHighlighterPanelSO.ApplyCodeHighlighting(sourceCode) to process source code")]
        [BilingualButton("输出语法高亮结果", "Log Syntax Highlighting Result", ButtonSizes.Large)]
        public void TestSyntaxHighlight()
        {
            Debug.Log(ApplyCodeHighlighting(exampleSourceCode));
        }

        [PropertyOrder(-10)]
        [BilingualTitle("使用限制", "Limitations", TitleAlignment = TitleAlignments.Centered)]
        [OnInspectorGUI]
        void OnGUI1() { }
    }
}

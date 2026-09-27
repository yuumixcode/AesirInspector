using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// 示例预览渲染器，负责绘制特性的使用示例及其控制按钮。
    /// </summary>
    public class ExamplePreviewRenderer : IAttributeComponentRenderer
    {
        const int ExampleNumberOneRow = 3;
        static readonly BilingualData _usageExampleLabel = new BilingualData("使用案例预览", "Usage Examples");

        static readonly BilingualData _pingMonoScriptButtonLabel =
            new BilingualData("Ping 脚本文件", "Ping Script File");

        static readonly BilingualData _resetExampleButtonLabel = new BilingualData("重置案例", "Reset Example");

        readonly AbstractAttributePanelSO _panel;
        AttributeExamplePreviewItem[] _examplePreviewItems;
        Rect _usageExampleContentRect;
        Rect _usageHeaderToolbarRect;

        public ExamplePreviewRenderer(AbstractAttributePanelSO panel) => _panel = panel;

        public bool IsVisible => _examplePreviewItems != null && _examplePreviewItems.Length > 0;

        public void Draw()
        {
            // 默认 Draw 不再执行完整逻辑，由 BeginDraw/EndDraw 替代以支持跨成员容器包裹
            BeginDraw();
            EndDraw();
        }

        public void OnLanguageChanged() { }

        public void Reset() { }

        /// <summary>
        /// 开始绘制示例预览容器。
        /// </summary>
        public void BeginDraw()
        {
            if (!IsVisible)
            {
                return;
            }

            _usageExampleContentRect =
                AbstractAttributePanelSO.BeginDrawContainerWithTitle(_usageExampleLabel,
                    out _usageHeaderToolbarRect);

            DrawExamplePreviewItems();
        }

        /// <summary>
        /// 结束绘制示例预览容器。
        /// </summary>
        public void EndDraw()
        {
            if (!IsVisible)
            {
                return;
            }

            AbstractAttributePanelSO.EndDrawContainerWithTitle(_usageExampleContentRect);
            DrawUsageExampleTitleButton();
        }

        public void SetData(AttributeExamplePreviewItem[] items)
        {
            _examplePreviewItems = items;
        }

        void DrawExamplePreviewItems()
        {
            if (_examplePreviewItems is not { Length: > 1 })
            {
                return;
            }

            EditorGUILayout.BeginVertical();
            for (var i = 0; i < _examplePreviewItems.Length; i += ExampleNumberOneRow)
            {
                EditorGUILayout.BeginHorizontal();
                for (var j = 0; j < ExampleNumberOneRow && i + j < _examplePreviewItems.Length; j++)
                {
                    DrawExampleTabButton(_examplePreviewItems[i + j]);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(10f);
        }

        void DrawExampleTabButton(AttributeExamplePreviewItem item)
        {
            var content = GUIHelper.TempContent(" " + item.ItemName,
                GUIHelper.GetAssetThumbnail(null, typeof(MonoBehaviour), false));

            var selectExample = item.ExampleType == AttributeExampleType.OdinSerialized
                ? item.OdinSerializedExample
                : item.UnitySerializedExample;

            var currentSelected = _panel.CurrentSelectedExample;
            var isSelected = selectExample == currentSelected;

            var iconSizeBackup = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));
            var rect = GUILayoutUtility.GetRect(content,
                AttributeOverviewEditorUtility.TabButtonCellTextStyle, GUILayoutOptions.Height(26));

            if (Event.current.type == EventType.Repaint)
            {
                Color color;
                if (isSelected)
                {
                    color = EditorGUIUtility.isProSkin
                        ? new Color(0.25f, 0.4f, 0.6f, 1f)
                        : new Color(0.75f, 0.83f, 0.9f, 1f);
                }
                else
                {
                    color = EditorGUIUtility.isProSkin
                        ? new Color(0.3f, 0.3f, 0.3f, 1f)
                        : new Color(0.85f, 0.85f, 0.85f, 1f);
                }

                SirenixEditorGUI.DrawSolidRect(rect, color);
                SirenixEditorGUI.DrawBorders(rect, 1);
            }

            if (GUI.Button(rect, content, AttributeOverviewEditorUtility.TabButtonCellTextStyle))
            {
                _panel.CurrentSelectedExample = selectExample;
            }

            EditorGUIUtility.SetIconSize(iconSizeBackup);
        }

        void DrawUsageExampleTitleButton()
        {
            var headerButtonRect = _usageHeaderToolbarRect.AlignCenterY(_usageHeaderToolbarRect.height)
                .AlignRight(240);
            var leftButtonRect = headerButtonRect.Split(0, 2);
            var pingTexture =
                SdfIcons.CreateTransparentIconTexture(SdfIconType.HandIndexFill, Color.white, 20, 20, 0);
            if (GUI.Button(leftButtonRect,
                    GUIHelper.TempContent(" " + _pingMonoScriptButtonLabel, pingTexture),
                    SirenixGUIStyles.ToolbarButton))
            {
                HandlePingMonoScript();
            }

            var rightButtonRect = headerButtonRect.Split(1, 2);
            var resetTexture =
                SdfIcons.CreateTransparentIconTexture(SdfIconType.ArrowClockwise, Color.white, 20, 20, 0);
            if (GUI.Button(rightButtonRect,
                    GUIHelper.TempContent(" " + _resetExampleButtonLabel, resetTexture),
                    SirenixGUIStyles.ToolbarButton))
            {
                var currentSelected = _panel.CurrentSelectedExample;
                if (currentSelected is IAesirInspectorReset canReset)
                {
                    canReset.AesirInspectorReset();
                    AttributeOverviewEditorUtility.LogEditorResetSuccess(currentSelected.GetType().Name);
                }
                else if (currentSelected != null)
                {
                    AttributeOverviewEditorUtility.LogEditorResetWarning(currentSelected.GetType().Name);
                }
            }
        }

        /// <summary>
        /// 「Ping 脚本文件」按钮：
        /// Assets 安装时在 Project 窗口中定位脚本；UPM 安装时脚本位于只读的 Packages 目录，
        /// 且用户可能隐藏了 Project 窗口的 Packages 区域，因此改为直接打开源码（定位磁盘文件兜底）。
        /// </summary>
        void HandlePingMonoScript()
        {
            var markAttribute = GetCurrentExampleMarkAttribute();
            if (markAttribute == null)
            {
                return;
            }

            if (AesirInspectorInstallationChecker.IsUpm)
            {
                OpenOrRevealExampleSource(markAttribute.FilePath);
                return;
            }

            var monoScriptAbsolutepath = markAttribute.FilePath;
            var assetRelativePath =
                "Assets/" + PathUtilities.MakeRelative(Application.dataPath, monoScriptAbsolutepath);
            var monoScript = AssetDatabase.LoadAssetAtPath<Object>(assetRelativePath);
            if (monoScript)
            {
                EditorGUIUtility.PingObject(monoScript);
            }
        }

        /// <summary>
        /// UPM 安装下的处理：优先在代码编辑器中打开示例脚本源码（不依赖 Project 窗口是否显示 Packages），
        /// 找不到 MonoScript 时退化为在文件管理器中揭示磁盘文件。
        /// </summary>
        void OpenOrRevealExampleSource(string monoScriptAbsolutePath)
        {
            var monoScript = FindMonoScriptByAbsolutePath(monoScriptAbsolutePath);
            if (monoScript)
            {
                AssetDatabase.OpenAsset(monoScript);
                Debug.Log(AesirInspectorLanguageSettingsSO.CurrentIsEnglish
                    ? "[Aesir Inspector] UPM installation: the example script source was opened in the code " +
                      "editor (the package lives in the read-only Packages folder, so it cannot be pinged from " +
                      "the Project window)."
                    : "[Aesir Inspector] 当前为 UPM 安装：已在代码编辑器中打开示例脚本源码" +
                      "（包位于只读的 Packages 目录，无法在 Project 窗口中 Ping）。");
                return;
            }

            EditorUtility.RevealInFinder(monoScriptAbsolutePath);
            Debug.Log(AesirInspectorLanguageSettingsSO.CurrentIsEnglish
                ? "[Aesir Inspector] UPM installation: the example script was revealed in the file manager: " +
                  monoScriptAbsolutePath
                : "[Aesir Inspector] 当前为 UPM 安装：已在文件管理器中定位示例脚本：" + monoScriptAbsolutePath);
        }

        /// <summary>
        /// 按文件名反查 MonoScript：UPM 安装时 [AesirExample] 捕获的是磁盘绝对路径，
        /// 无法直接作为 AssetDatabase 路径使用，这里改用文件名（示例脚本名唯一）反查。
        /// </summary>
        static Object FindMonoScriptByAbsolutePath(string monoScriptAbsolutePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(monoScriptAbsolutePath);
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            foreach (var guid in AssetDatabase.FindAssets(fileName + " t:MonoScript"))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(assetPath) != fileName)
                {
                    continue;
                }

                var monoScript = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                if (monoScript)
                {
                    return monoScript;
                }
            }

            return null;
        }

        AesirExampleAttribute GetCurrentExampleMarkAttribute()
        {
            var currentSelected = _panel.CurrentSelectedExample;
            if (!currentSelected)
            {
                return null;
            }

            return AttributeOverviewEditorUtility.GetAttributeInExampleType(currentSelected.GetType());
        }
    }
}

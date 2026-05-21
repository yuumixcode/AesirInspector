# 变更日志

本项目所有重大变更都将记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，本项目遵循[语义化版本](https://semver.org/lang/zh-CN/)规范。

[English](Documentation~/en/CHANGELOG.md)

---

## [Unreleased]

### Changed
- 将 `OdinWrapper` 重命名为 `Odin Integration`（目录）/ `OdinIntegration`（命名空间与程序集），以更准确表达集成层的语义
- 将 `Runtime/Unity/Bilingualism/` 重命名为 `Runtime/Unity/Localization/`，对齐 Unity 官方 Localization 包命名
- 将 `Runtime/Unity/InspectorControls/` 重命名为 `Runtime/Unity/Inspector/`，采用 Unity 单数名词惯例
- 将 `Runtime/Unity/Logger/` 重命名为 `Runtime/Unity/Logging/`，对齐 Unity 源码 `Runtime/Export/Logging/` 命名

---

## [0.4.0-pre.1] - 2026-04-29

### Architecture

#### Added
- 新增 `OdinWrapper` 独立程序集，包含 Runtime（`RunLab.AesirInspector.OdinWrapper`）与 Editor（`RunLab.AesirInspector.OdinWrapper.Editor`）两个 asmdef，均设 `defineConstraints: ODIN_INSPECTOR`，将 Odin Inspector 依赖从核心程序集完全隔离 [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

#### Changed
- Runtime 核心程序集 `RunLab.AesirInspector` 移除 `ODIN_INSPECTOR` 编译约束，不再强依赖 Odin Inspector [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- 编辑器程序集 `RunLab.AesirInspector.Editor` 调整程序集引用，不再直接依赖 Odin [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

### OdinBridge

#### Added
- 新增 `IOdinBridge` 接口，定义 `IsOdinPresent` 等 Odin 可用性查询能力 [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- 新增 `DefaultOdinBridge`，无 Odin 时自动回退的默认桥接实现 [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- 新增 `OdinBridgeLocator`，自动查找 Odin 桥接或回退至默认实现 [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- 新增 `OdinInspectorBridge`（OdinWrapper/Editor/Bridge/），Odin 可用时提供编辑器侧桥接实现 [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

### OdinWrapper

#### Added
- 新增 `OdinWrapper/Editor/AttributeProcessors/` 目录，包含 5 个 OdinAttributeProcessor：`AesirInspectorLanguageSettingsProcessor`、`AesirInspectorResetProcessor`、`BilingualDisplayAsStringProcessor`、`BilingualHeaderProcessor`、`HorizontalSeparateProcessor` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

#### Changed
- `Editor/AttributeOverviewPro/` 移动至 `OdinWrapper/Editor/AttributeOverviewPro/` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- `Editor/Drawers/Bilingual/` 移动至 `OdinWrapper/Editor/Drawers/` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- `Editor/ExtensionManager/` 移动至 `OdinWrapper/Editor/ExtensionManager/` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- `Editor/MiniTools/` 移动至 `OdinWrapper/Editor/MiniTools/` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- `Editor/ScriptDocGenerator/` 移动至 `OdinWrapper/Editor/ScriptDocGenerator/` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- `Editor/Core/Windows/` 移动至 `OdinWrapper/Editor/Windows/` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- `Runtime/Bilingual/Attributes/` 下 6 个 Bilingual 特性移动至 `OdinWrapper/Runtime/Attributes/` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- `Editor/Core/AesirCodeHighlighter.cs` 移动至 `OdinWrapper/Runtime/OdinCodeHighlighter.cs` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- `OdinSyntaxHighlighterSO` 重命名为 `OdinSyntaxHighlighterPanelSO` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

### Bilingualism

#### Changed
- `Runtime/Bilingual/` 重命名为 `Runtime/Bilingualism/` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- `AesirInspectorLanguageSettingsSO` 精简，移除 Odin 依赖逻辑，由 `AesirInspectorLanguageSettingsProcessor` 接管 [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

#### Removed
- 移除 `DisplayAsStringBilingualConfigAttribute`，由 `BilingualDisplayAsStringControl` + Processor 替代 [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- 移除 `ShowIfChineseAttribute`、`ShowIfEnglishAttribute`，由 Processor 替代 [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- 移除 `DisplayAsStringBilingualWidget`、`HeaderBilingualWidget`，由对应 Control 替代 [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

### InspectorControls

#### Added
- 新增 `BilingualDisplayAsStringControl`，替代原 `DisplayAsStringBilingualWidget` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- 新增 `BilingualHeaderControl`，替代原 `HeaderBilingualWidget` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

#### Changed
- `Runtime/InspectorWidgets/` 重命名为 `Runtime/InspectorControls/`，Widget 统一改名为 Control [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- `HorizontalSeparateWidget` 重命名为 `HorizontalSeparateControl` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

### Core

#### Changed
- `IAesirInspectorReset` 精简接口定义，重置逻辑移至 `AesirInspectorResetProcessor` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- `AesirInspectorLogger` 从 `Runtime/Core/` 移动至 `Runtime/Logger/` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- `AesirInspectorLoggerSettings` 从 `Runtime/Core/` 移动至 `Runtime/Logger/` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- `SummaryAttribute` 从 `Runtime/Attributes/Docs/` 移动至 `Runtime/Attributes/`，扁平化目录 [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

#### Removed
- 移除 `ShowEnablePropertyAttribute` 废弃特性 [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

### Utilities

#### Changed
- `ReflectionUtility` 大幅增强，新增反射工具方法 [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

#### Removed
- 移除 `OdinInspectorSafeEditorUtility`，由 OdinBridge 模式替代 [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

### ScriptDocGenerator

#### Changed
- 所有 AnalysisData 类（ConstructorData、EventData、FieldData、MemberData、MethodData、ParameterData、ParameterDirection、PropertyData、TypeData）移除 Odin 特性依赖 [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

### Samples

#### Changed
- `Samples~/` 移动至 `Samples/`（Plugin Config Solutions、RuntimeInitializeLoadType），示例目录对用户可见 [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

#### Removed
- 移除 Codely Skills Library 示例（custom-package-creator） [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

### Tests

#### Changed
- `RunLab.AesirInspector.Tests` asmdef 移除 `ODIN_INSPECTOR` 编译约束 [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- `RunLab.AesirInspector.Editor.Tests` asmdef 调整程序集引用 [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- 多个测试文件调整代码格式与区域重排，移除未使用的 using 引用 [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

### Code Style

#### Changed
- 更新 `AESIR_INSPECTOR_CODE_STYLE.cs` 代码风格指南，适配新的程序集架构与命名规范 [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

---

## [0.3.1] - 2026-04-27

### Core

#### Added
- 新增 `AesirInspectorLoggerSettings` ScriptableObject，支持通过 `enableInfoLog`（默认 false）和 `enableWarningLog`（默认 true）控制日志输出 [`45a4837`](https://github.com/yuumixcode/aesir-inspector/commit/45a4837c99913708e8d16218cf4b9acf5459fbb2)

#### Changed
- `AesirInspectorLogger` 从 Utilities 迁移至 Core 目录，Info/Warning 方法集成 `AesirInspectorLoggerSettings` 开关检查，移除 `MethodImpl` 特性 [`45a4837`](https://github.com/yuumixcode/aesir-inspector/commit/45a4837c99913708e8d16218cf4b9acf5459fbb2)
- `AesirInspectorWebLinks` 重命名 `GitWebsite` 为 `GitUrl`，`OdinInspectorDocsUrl` 链接由 documentation 改为 tutorials [`45a4837`](https://github.com/yuumixcode/aesir-inspector/commit/45a4837c99913708e8d16218cf4b9acf5459fbb2)
- `IAesirInspectorReset` 右键菜单标签由 "Aesir Toolkit Reset" 改为 "Aesir Inspector Reset" [`45a4837`](https://github.com/yuumixcode/aesir-inspector/commit/45a4837c99913708e8d16218cf4b9acf5459fbb2)
- `AesirInspectorMenuItems` 菜单路径重构：`ToolsMenuRoot` 拆分为 `ToolsAesirRoot`（Tools/Aesir）与 `ToolsAesirInspectorRoot`（Tools/Aesir/Inspector），新增各菜单项优先级常量 [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)
- `AesirCodeHighlighter` 移除 `#if UNITY_EDITOR && ODIN_INSPECTOR_3_3` 宏包裹，using 语句移至命名空间外部 [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

#### Removed
- 移除全项目 `#if ODIN_INSPECTOR_3_3` 预处理指令，Odin Inspector 作为硬依赖 [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

### Bilingual

#### Changed
- `AesirInspectorLanguageSettings` 重命名为 `AesirInspectorLanguageSettingsSO`，符合 ScriptableObject 命名规范 [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)
- `DisplayAsStringBilingualWidgetConfigAttribute` 重命名为 `DisplayAsStringBilingualConfigAttribute`，移除 Widget 中间词 [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)
- `BilingualData` 从 `Runtime/Bilingual/Attributes/` 移动至 `Runtime/Bilingual/` [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)
- `HeaderBilingualWidget` 的 `_chineseIntroduction`、`_englishIntroduction` 字段标记为 readonly，条件编译由 `#if ODIN_INSPECTOR_3_3` 改为 `#if UNITY_EDITOR` [`45a4837`](https://github.com/yuumixcode/aesir-inspector/commit/45a4837c99913708e8d16218cf4b9acf5459fbb2) [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)
- `BilingualBoxGroupAttribute`、`BilingualButtonAttribute` 移除 `#region Internal` [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)
- `BilingualTitleGroupAttribute` 的 `TitleAlignment` 属性移出 `#if ODIN_INSPECTOR_3_3` 宏包裹 [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

#### Removed
- 移除所有 Bilingual 属性与 Drawer 中的 `#if ODIN_INSPECTOR_3_3` 宏 [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

### AttributeOverviewPro

#### Changed
- `Editor/AttributeOverview/` 整个目录重命名为 `Editor/AttributeOverviewPro/` [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)
- 内部 `Data/` 目录下 `AttributeExamplePreviewItem`、`ParameterValue`、`ResolvedStringParameterValue` 移动至 `Core/` 子目录 [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)
- `AssetListExampleForCustomFilterMethodSO` 重命名为 `AssetListExampleWithCustomFilterMethodSO` [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

### Utilities

#### Changed
- `OdinInspectorSafeEditorUtility` 中 `new T[0]` 替换为 `Array.Empty<T>()`，`new Type[1]` 替换为 `new[]` [`45a4837`](https://github.com/yuumixcode/aesir-inspector/commit/45a4837c99913708e8d16218cf4b9acf5459fbb2)
- `PathSafeEditorUtility.EnsureDirectoryExists` 添加 `[Conditional("UNITY_EDITOR")]` [`45a4837`](https://github.com/yuumixcode/aesir-inspector/commit/45a4837c99913708e8d16218cf4b9acf5459fbb2)

#### Removed
- 移除所有 Utilities 类中的 `#region Public Methods` 和 `#region` 模式 [`45a4837`](https://github.com/yuumixcode/aesir-inspector/commit/45a4837c99913708e8d16218cf4b9acf5459fbb2)

### MiniTools

#### Changed
- `AssemblyFilterExample` 重命名为 `FilterOutAesirInspectorAssembly` [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

#### Removed
- 移除 MiniTools 模块中的 `#if ODIN_INSPECTOR_3_3` 宏 [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

### ScriptDocGenerator

#### Changed
- 所有 AnalysisData 类中 Odin 特性移至 XML 注释之前 [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

#### Removed
- 移除 ScriptDocGenerator 模块中的 `#if ODIN_INSPECTOR_3_3` 宏 [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

### Code Style

#### Changed
- `HorizontalSeparateWidget` 的 `_darkLineHeight`、`_lightLineHeight`、`_spaceAfter`、`_spaceBefore` 字段标记为 readonly，`DarkLineColor`、`LightLineColor` 属性标记为 static [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

#### Removed
- 移除 `#region Internal` 模式，更新代码风格指南与示例代码 [`45a4837`](https://github.com/yuumixcode/aesir-inspector/commit/45a4837c99913708e8d16218cf4b9acf5459fbb2) [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

### Samples

#### Changed
- PluginConfig 示例目录重命名 [`58fdbce`](https://github.com/yuumixcode/aesir-inspector/commit/58fdbce495c6d25e601f8eb0a6ae6bd17d403f75)

### Docs

#### Added
- 新增 `ATTRIBUTE_OVERVIEW_PRO_GUIDE.md` AttributeOverviewPro 模块编码指南，涵盖 Data-Panel-Example 三件套、单例 SO 模式、OdinAttributeProcessor 注入、GUITable 缓存、双语系统、命名速查等 [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)
- 新增 `SCRIPT_DOC_GENERATOR_GUIDE.md` ScriptDocGenerator 模块编码规范，涵盖架构分层、单例、重置、事件通信、文件输出等 [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)
- 新增 `UTILITIES_GUIDE.md` Utilities 编码指南文档 [`45a4837`](https://github.com/yuumixcode/aesir-inspector/commit/45a4837c99913708e8d16218cf4b9acf5459fbb2)

#### Changed
- `AESIR_INSPECTOR_CODE_STYLE_GUIDE.md` 移除 #region Internal 规则，简化 Odin Inspector 集成规范 [`45a4837`](https://github.com/yuumixcode/aesir-inspector/commit/45a4837c99913708e8d16218cf4b9acf5459fbb2) [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

---

## [0.3.0] - 2026-04-25

### Core

#### Added
- 新增菜单路径与优先级统一管理类 `AesirInspectorMenuItems`，统一管理 Tools 菜单和 Assets 上下文菜单 [`77f3b1b`](https://github.com/yuumixcode/aesir-inspector/commit/77f3b1b219eda197a67637c49c24c54ba4e8e0a5)
- 新增 Getting Started 窗口，展示版本号、功能列表和文档链接 [`77f3b1b`](https://github.com/yuumixcode/aesir-inspector/commit/77f3b1b219eda197a67637c49c24c54ba4e8e0a5)
- 新增 Preferences 偏好设置窗口，集成语言设置 [`77f3b1b`](https://github.com/yuumixcode/aesir-inspector/commit/77f3b1b219eda197a67637c49c24c54ba4e8e0a5)
- 新增 `AesirInspectorVersion` 版本信息静态类 [`77f3b1b`](https://github.com/yuumixcode/aesir-inspector/commit/77f3b1b219eda197a67637c49c24c54ba4e8e0a5)
- 新增 `IAesirInspectorReset` 重置接口及 `AesirInspectorResetAttributeProcessor`，为实现该接口的类自动添加右键重置菜单 [`77f3b1b`](https://github.com/yuumixcode/aesir-inspector/commit/77f3b1b219eda197a67637c49c24c54ba4e8e0a5)
- 新增代码语法高亮器 `AesirCodeHighlighter` [`77f3b1b`](https://github.com/yuumixcode/aesir-inspector/commit/77f3b1b219eda197a67637c49c24c54ba4e8e0a5)

#### Changed
- 静默安装检测日志输出（注释掉 `Debug.Log`） [`77f3b1b`](https://github.com/yuumixcode/aesir-inspector/commit/77f3b1b219eda197a67637c49c24c54ba4e8e0a5)
- 扩展 `AesirInspectorPaths` 路径常量，新增 AttributeOverview 和 MiniTools 路径 [`77f3b1b`](https://github.com/yuumixcode/aesir-inspector/commit/77f3b1b219eda197a67637c49c24c54ba4e8e0a5)
- 扩展 `AesirInspectorWebLinks` 链接常量，新增 GitHub 仓库、许可证、更新日志和 Odin Inspector 文档链接 [`77f3b1b`](https://github.com/yuumixcode/aesir-inspector/commit/77f3b1b219eda197a67637c49c24c54ba4e8e0a5)

### Bilingual

#### Added
- 新增 `ShowEnablePropertyAttribute` 复合特性 [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- 新增 `HorizontalSeparateWidget` 水平分隔线 Inspector 组件 [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)

#### Changed
- 重构 `HeaderBilingualWidget` [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)

### Utilities

#### Added
- 新增 `AesirInspectorLogger` 日志工具类 [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- 新增 `PathUtility`、`PathSafeEditorUtility` 路径工具类 [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- 新增 `ReflectionUtility` 反射工具类 [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- 新增 `RegexUtility` 正则表达式工具类 [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- 新增 `HierarchyUtility`、`HierarchySafeEditorUtility` Hierarchy 工具类 [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- 新增 `MonoScriptSafeEditorUtility` MonoScript 工具类 [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- 新增 `PlayerLoopUtility` PlayerLoop 工具类 [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- 新增 `PredefinedAssemblyUtility` 预定义程序集工具类 [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- 新增 `ProjectSafeEditorUtility` 项目安全编辑器工具类 [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)

#### Changed
- 扩展 `ScriptableObjectSafeEditorUtility`，新增大量 ScriptableObject 编辑器操作方法 [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- 扩展 `OdinInspectorSafeEditorUtility` 和 `UrlUtility` [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)

### MiniTools

#### Added
- 新增 `AesirInspectorMiniToolsWindow` MiniTools 主窗口 [`b7068eb`](https://github.com/yuumixcode/aesir-inspector/commit/b7068eb0d1ab0a6cf7d7db9293ae874516a59d89)
- 新增 MenuItemViewer 菜单项检查器，支持 `IAssemblyFilter` 程序集过滤和 `ISearchFilterable` 搜索 [`b7068eb`](https://github.com/yuumixcode/aesir-inspector/commit/b7068eb0d1ab0a6cf7d7db9293ae874516a59d89)
- 新增 OdinSyntaxHighlighter 语法高亮处理器面板，委托 `AesirCodeHighlighter` 实现 [`b7068eb`](https://github.com/yuumixcode/aesir-inspector/commit/b7068eb0d1ab0a6cf7d7db9293ae874516a59d89)
- 新增 QuickCreateSO 右键快捷生成 ScriptableObject 工具，支持单选和多选批量创建 [`b7068eb`](https://github.com/yuumixcode/aesir-inspector/commit/b7068eb0d1ab0a6cf7d7db9293ae874516a59d89)

### ScriptDocGenerator

#### Added
- 新增文档生成器窗口和可视化面板 `ScriptableObject` 单例 [`c2f2e75`](https://github.com/yuumixcode/aesir-inspector/commit/c2f2e7503937cd2f4fae106938605464e512c332)
- 新增文档生成器逻辑控制类 `ScriptDocGeneratorController` [`c2f2e75`](https://github.com/yuumixcode/aesir-inspector/commit/c2f2e7503937cd2f4fae106938605464e512c332)
- 新增 Assets 上下文菜单项，支持添加脚本到 TargetType 或 TemporaryTypes [`c2f2e75`](https://github.com/yuumixcode/aesir-inspector/commit/c2f2e7503937cd2f4fae106938605464e512c332)
- 新增中文 Scripting API 配置和文档生成器设置 [`c2f2e75`](https://github.com/yuumixcode/aesir-inspector/commit/c2f2e7503937cd2f4fae106938605464e512c332)
- 新增完整的类型分析数据模型层：`MemberData`、`FieldData`、`PropertyData`、`MethodData`、`ConstructorData`、`EventData`、`ParameterData`、`TypeData` 及对应接口 [`c2f2e75`](https://github.com/yuumixcode/aesir-inspector/commit/c2f2e7503937cd2f4fae106938605464e512c332)
- 新增类型分析器静态扩展 `TypeAnalyzerStaticExtensions` 和工具类 `TypeAnalyzerUtility` [`c2f2e75`](https://github.com/yuumixcode/aesir-inspector/commit/c2f2e7503937cd2f4fae106938605464e512c332)
- 新增 `AccessModifierType`、`TypeCategory`、`ParameterDirection` 枚举 [`c2f2e75`](https://github.com/yuumixcode/aesir-inspector/commit/c2f2e7503937cd2f4fae106938605464e512c332)
- 新增 `DefaultAnalysisDataFactory`、`DefaultAttributeFilter`、`DerivedMemberDataComparer` 核心工具 [`c2f2e75`](https://github.com/yuumixcode/aesir-inspector/commit/c2f2e7503937cd2f4fae106938605464e512c332)
- 新增 `ReferenceLinkURLAttribute` 引用链接特性 [`c2f2e75`](https://github.com/yuumixcode/aesir-inspector/commit/c2f2e7503937cd2f4fae106938605464e512c332)

### AttributeOverview

#### Added
- 新增特性概览窗口 `AttributeOverviewWindow` 和数据库 `AttributeOverviewDatabaseSO` [`0e53a40`](https://github.com/yuumixcode/aesir-inspector/commit/0e53a40ef55c26dab037f3f6aaf11cc09fbc9dad)
- 新增面板抽象框架：泛型基类 `AttributeOverviewPanelSO<T>`、`AbstractAttributePanelSO` 及 Odin AttributeProcessor 自动配置 [`0e53a40`](https://github.com/yuumixcode/aesir-inspector/commit/0e53a40ef55c26dab037f3f6aaf11cc09fbc9dad)
- 新增 AssetList、AssetsOnly、CustomValueDrawer 三个内置特性面板 [`0e53a40`](https://github.com/yuumixcode/aesir-inspector/commit/0e53a40ef55c26dab037f3f6aaf11cc09fbc9dad)
- 新增 `AesirExampleAttribute`、`AttributeCategoryAttribute` 特性标记 [`0e53a40`](https://github.com/yuumixcode/aesir-inspector/commit/0e53a40ef55c26dab037f3f6aaf11cc09fbc9dad)
- 新增特性数据模型 `AbstractAttributeData`、`ParameterValue`、`ResolvedStringParameterValue`、`AttributeExamplePreviewItem` [`0e53a40`](https://github.com/yuumixcode/aesir-inspector/commit/0e53a40ef55c26dab037f3f6aaf11cc09fbc9dad)
- 新增 `AesirAttributeCategory` 分类枚举和 `OdinInspectorDocumentationLinks` 文档链接常量 [`0e53a40`](https://github.com/yuumixcode/aesir-inspector/commit/0e53a40ef55c26dab037f3f6aaf11cc09fbc9dad)
- 新增特性概览编辑器工具类和用法示例 [`0e53a40`](https://github.com/yuumixcode/aesir-inspector/commit/0e53a40ef55c26dab037f3f6aaf11cc09fbc9dad)

### SummaryTool

#### Added
- 新增 XML Summary 注释处理工具 `XmlSummaryTool`，支持 Sync/Replace/Remove 操作 [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- 新增 `XmlCodePart` XML 代码段解析类 [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- 新增 SummaryTool Assets 上下文菜单项 [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)

### ExtensionManager

#### Added
- 新增扩展包管理器窗口 `ExtensionPackageManagerWindow`，支持 Git URL 安装 [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- 新增 `ExtensionPackageCard` 扩展包卡片数据类 [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- 新增 `PackageManagerEditorUtility` Package Manager 编辑器工具类 [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)

### Samples

#### Added
- 新增 PluginConfigSolutions 示例模块，演示 ScriptableSingleton 在 Preferences 和 Project 中的使用方式 [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- 新增 RuntimeInitializeLoadType 示例模块，演示五个初始化时机的执行顺序与最佳实践 [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)

### Tests

#### Added
- 新增 ScriptDocGenerator 完整单元测试，覆盖构造方法、事件、字段、方法、属性、类型数据及成员继承 [`1cf6d6d`](https://github.com/yuumixcode/aesir-inspector/commit/1cf6d6ddd746628a03917783ca41cf378155f093)
- 新增 SummaryTool XML 注释处理测试 [`1cf6d6d`](https://github.com/yuumixcode/aesir-inspector/commit/1cf6d6ddd746628a03917783ca41cf378155f093)
- 新增 UnityEngine.Object 运算符重载 Runtime 测试 [`1cf6d6d`](https://github.com/yuumixcode/aesir-inspector/commit/1cf6d6ddd746628a03917783ca41cf378155f093)

#### Changed
- 为两个测试 asmdef 添加 `ODIN_INSPECTOR` 编译约束 [`1cf6d6d`](https://github.com/yuumixcode/aesir-inspector/commit/1cf6d6ddd746628a03917783ca41cf378155f093)

---

## [0.2.1] - 2026-04-23

### Added

- 新增 Aesir Inspector 安装方式检测功能 [`b7de538`](https://github.com/yuumixcode/aesir-inspector/commit/b7de538eaf24efdd5b96d2c26d7c2897a8e6a6b5)

---

## [0.2.0] - 2026-04-23

### Added

- 实现双语 Inspector 系统与核心基础设施 [`a2c750b`](https://github.com/yuumixcode/aesir-inspector/commit/a2c750b3e693f7a0d9d356e987a6fcf1ef8d59ea)
- 新增 Codely Skills Library 示例，包含 custom-package-creator 技能 [`9422695`](https://github.com/yuumixcode/aesir-inspector/commit/942269529be76eba7425dcbd1ffcd6fef4964ece)

---

## [0.1.0] - 2026-04-22

### Added

- Initial release.

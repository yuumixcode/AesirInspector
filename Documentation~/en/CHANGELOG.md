# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

[中文](../../CHANGELOG.md)

---

## [Unreleased]

### Changed
- Renamed `OdinWrapper` to `Odin Integration` (directories) / `OdinIntegration` (namespaces and assemblies) for more accurate semantic representation of the integration layer
- Renamed `Runtime/Unity/Bilingualism/` to `Runtime/Unity/Localization/`, aligning with Unity's official Localization package naming
- Renamed `Runtime/Unity/InspectorControls/` to `Runtime/Unity/Inspector/`, adopting Unity's singular noun convention
- Renamed `Runtime/Unity/Logger/` to `Runtime/Unity/Logging/`, aligning with Unity source `Runtime/Export/Logging/` naming

---

## [0.4.0-pre.1] - 2026-04-29

### Architecture

#### Added
- Added `OdinWrapper` independent assembly with Runtime (`RunLab.AesirInspector.OdinWrapper`) and Editor (`RunLab.AesirInspector.OdinWrapper.Editor`) asmdef files, both with `defineConstraints: ODIN_INSPECTOR`, fully isolating Odin Inspector dependency from the core assembly [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

#### Changed
- Removed `ODIN_INSPECTOR` define constraint from core Runtime assembly `RunLab.AesirInspector`, eliminating hard dependency on Odin Inspector [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- Adjusted Editor assembly `RunLab.AesirInspector.Editor` references to remove direct Odin dependency [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

### OdinBridge

#### Added
- Added `IOdinBridge` interface defining `IsOdinPresent` and other Odin availability queries [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- Added `DefaultOdinBridge` as fallback implementation when Odin is not present [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- Added `OdinBridgeLocator` for automatic Odin bridge discovery with default fallback [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- Added `OdinInspectorBridge` (OdinWrapper/Editor/Bridge/) as editor-side bridge implementation when Odin is available [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

### OdinWrapper

#### Added
- Added `OdinWrapper/Editor/AttributeProcessors/` directory with 5 OdinAttributeProcessors: `AesirInspectorLanguageSettingsProcessor`, `AesirInspectorResetProcessor`, `BilingualDisplayAsStringProcessor`, `BilingualHeaderProcessor`, `HorizontalSeparateProcessor` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

#### Changed
- Moved `Editor/AttributeOverviewPro/` to `OdinWrapper/Editor/AttributeOverviewPro/` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- Moved `Editor/Drawers/Bilingual/` to `OdinWrapper/Editor/Drawers/` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- Moved `Editor/ExtensionManager/` to `OdinWrapper/Editor/ExtensionManager/` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- Moved `Editor/MiniTools/` to `OdinWrapper/Editor/MiniTools/` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- Moved `Editor/ScriptDocGenerator/` to `OdinWrapper/Editor/ScriptDocGenerator/` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- Moved `Editor/Core/Windows/` to `OdinWrapper/Editor/Windows/` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- Moved 6 Bilingual attributes from `Runtime/Bilingual/Attributes/` to `OdinWrapper/Runtime/Attributes/` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- Moved `Editor/Core/AesirCodeHighlighter.cs` to `OdinWrapper/Runtime/OdinCodeHighlighter.cs` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- Renamed `OdinSyntaxHighlighterSO` to `OdinSyntaxHighlighterPanelSO` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

### Bilingualism

#### Changed
- Renamed `Runtime/Bilingual/` to `Runtime/Bilingualism/` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- Simplified `AesirInspectorLanguageSettingsSO`, removed Odin dependency logic, now handled by `AesirInspectorLanguageSettingsProcessor` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

#### Removed
- Removed `DisplayAsStringBilingualConfigAttribute`, replaced by `BilingualDisplayAsStringControl` + Processor [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- Removed `ShowIfChineseAttribute` and `ShowIfEnglishAttribute`, replaced by Processor [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- Removed `DisplayAsStringBilingualWidget` and `HeaderBilingualWidget`, replaced by corresponding Controls [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

### InspectorControls

#### Added
- Added `BilingualDisplayAsStringControl`, replacing `DisplayAsStringBilingualWidget` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- Added `BilingualHeaderControl`, replacing `HeaderBilingualWidget` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

#### Changed
- Renamed `Runtime/InspectorWidgets/` to `Runtime/InspectorControls/`, unified Widget naming to Control [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- Renamed `HorizontalSeparateWidget` to `HorizontalSeparateControl` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

### Core

#### Changed
- Simplified `IAesirInspectorReset` interface, reset logic moved to `AesirInspectorResetProcessor` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- Moved `AesirInspectorLogger` from `Runtime/Core/` to `Runtime/Logger/` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- Moved `AesirInspectorLoggerSettings` from `Runtime/Core/` to `Runtime/Logger/` [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- Moved `SummaryAttribute` from `Runtime/Attributes/Docs/` to `Runtime/Attributes/`, flattened directory [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

#### Removed
- Removed `ShowEnablePropertyAttribute` deprecated attribute [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

### Utilities

#### Changed
- Significantly enhanced `ReflectionUtility` with additional reflection utility methods [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

#### Removed
- Removed `OdinInspectorSafeEditorUtility`, replaced by OdinBridge pattern [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

### ScriptDocGenerator

#### Changed
- Removed Odin attribute dependencies from all AnalysisData classes (ConstructorData, EventData, FieldData, MemberData, MethodData, ParameterData, ParameterDirection, PropertyData, TypeData) [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

### Samples

#### Changed
- Moved `Samples~/` to `Samples/` (Plugin Config Solutions, RuntimeInitializeLoadType), making samples visible to users [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

#### Removed
- Removed Codely Skills Library sample (custom-package-creator) [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

### Tests

#### Changed
- Removed `ODIN_INSPECTOR` define constraint from `RunLab.AesirInspector.Tests` asmdef [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- Adjusted `RunLab.AesirInspector.Editor.Tests` asmdef assembly references [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)
- Reformatted multiple test files with region reordering and removed unused using directives [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

### Code Style

#### Changed
- Updated `AESIR_INSPECTOR_CODE_STYLE.cs` code style guide to align with new assembly architecture and naming conventions [`473640f`](https://github.com/yuumixcode/aesir-inspector/commit/473640f6e38b0d2cd7b4c308cebcac0cde32643d)

---

## [0.3.1] - 2026-04-27

### Core

#### Added
- Added `AesirInspectorLoggerSettings` ScriptableObject for log level configuration with `enableInfoLog` (default false) and `enableWarningLog` (default true) [`45a4837`](https://github.com/yuumixcode/aesir-inspector/commit/45a4837c99913708e8d16218cf4b9acf5459fbb2)

#### Changed
- Moved `AesirInspectorLogger` from Utilities to Core directory, integrated LoggerSettings switch check in Info/Warning methods, removed `MethodImpl` attribute [`45a4837`](https://github.com/yuumixcode/aesir-inspector/commit/45a4837c99913708e8d16218cf4b9acf5459fbb2)
- Renamed `AesirInspectorWebLinks.GitWebsite` to `GitUrl`, changed `OdinInspectorDocsUrl` link from documentation to tutorials [`45a4837`](https://github.com/yuumixcode/aesir-inspector/commit/45a4837c99913708e8d16218cf4b9acf5459fbb2)
- Changed `IAesirInspectorReset` context menu label from "Aesir Toolkit Reset" to "Aesir Inspector Reset" [`45a4837`](https://github.com/yuumixcode/aesir-inspector/commit/45a4837c99913708e8d16218cf4b9acf5459fbb2)
- Restructured `AesirInspectorMenuItems` menu paths: split `ToolsMenuRoot` into `ToolsAesirRoot` (Tools/Aesir) and `ToolsAesirInspectorRoot` (Tools/Aesir/Inspector), added priority constants for all menu items [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)
- Removed `#if UNITY_EDITOR && ODIN_INSPECTOR_3_3` guard from `AesirCodeHighlighter`, moved using statements outside namespace [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

#### Removed
- Removed `#if ODIN_INSPECTOR_3_3` preprocessor guards across the entire project, making Odin Inspector a hard dependency [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

### Bilingual

#### Changed
- Renamed `AesirInspectorLanguageSettings` to `AesirInspectorLanguageSettingsSO` following ScriptableObject naming convention [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)
- Renamed `DisplayAsStringBilingualWidgetConfigAttribute` to `DisplayAsStringBilingualConfigAttribute`, removed Widget infix [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)
- Moved `BilingualData` from `Runtime/Bilingual/Attributes/` to `Runtime/Bilingual/` [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)
- Marked `_chineseIntroduction` and `_englishIntroduction` fields as readonly in `HeaderBilingualWidget`, changed conditional compilation from `#if ODIN_INSPECTOR_3_3` to `#if UNITY_EDITOR` [`45a4837`](https://github.com/yuumixcode/aesir-inspector/commit/45a4837c99913708e8d16218cf4b9acf5459fbb2) [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)
- Removed `#region Internal` from `BilingualBoxGroupAttribute` and `BilingualButtonAttribute` [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)
- Moved `TitleAlignment` property out of `#if ODIN_INSPECTOR_3_3` guard in `BilingualTitleGroupAttribute` [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

#### Removed
- Removed `#if ODIN_INSPECTOR_3_3` guards from all Bilingual attributes and drawers [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

### AttributeOverviewPro

#### Changed
- Renamed entire `Editor/AttributeOverview/` directory to `Editor/AttributeOverviewPro/` [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)
- Moved `AttributeExamplePreviewItem`, `ParameterValue`, `ResolvedStringParameterValue` from `Data/` to `Core/` subdirectory [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)
- Renamed `AssetListExampleForCustomFilterMethodSO` to `AssetListExampleWithCustomFilterMethodSO` [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

### Utilities

#### Changed
- Replaced `new T[0]` with `Array.Empty<T>()` and `new Type[1]` with `new[]` in `OdinInspectorSafeEditorUtility` [`45a4837`](https://github.com/yuumixcode/aesir-inspector/commit/45a4837c99913708e8d16218cf4b9acf5459fbb2)
- Added `[Conditional("UNITY_EDITOR")]` to `PathSafeEditorUtility.EnsureDirectoryExists` [`45a4837`](https://github.com/yuumixcode/aesir-inspector/commit/45a4837c99913708e8d16218cf4b9acf5459fbb2)

#### Removed
- Removed `#region Public Methods` and `#region` patterns from all utility classes [`45a4837`](https://github.com/yuumixcode/aesir-inspector/commit/45a4837c99913708e8d16218cf4b9acf5459fbb2)

### MiniTools

#### Changed
- Renamed `AssemblyFilterExample` to `FilterOutAesirInspectorAssembly` [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

#### Removed
- Removed `#if ODIN_INSPECTOR_3_3` guards from MiniTools module [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

### ScriptDocGenerator

#### Changed
- Moved Odin attributes before XML comments in all AnalysisData classes [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

#### Removed
- Removed `#if ODIN_INSPECTOR_3_3` guards from ScriptDocGenerator module [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

### Code Style

#### Changed
- Marked `_darkLineHeight`, `_lightLineHeight`, `_spaceAfter`, `_spaceBefore` as readonly and `DarkLineColor`, `LightLineColor` as static in `HorizontalSeparateWidget` [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

#### Removed
- Removed `#region Internal` pattern, updated code style guide and example code [`45a4837`](https://github.com/yuumixcode/aesir-inspector/commit/45a4837c99913708e8d16218cf4b9acf5459fbb2) [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

### Samples

#### Changed
- Renamed PluginConfig sample directory [`58fdbce`](https://github.com/yuumixcode/aesir-inspector/commit/58fdbce495c6d25e601f8eb0a6ae6bd17d403f75)

### Docs

#### Added
- Added `ATTRIBUTE_OVERVIEW_PRO_GUIDE.md` coding guide for AttributeOverviewPro module covering Data-Panel-Example trio, singleton SO pattern, OdinAttributeProcessor injection, GUITable caching, bilingual system, naming conventions [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)
- Added `SCRIPT_DOC_GENERATOR_GUIDE.md` coding standards for ScriptDocGenerator module covering architecture layering, singleton, reset, event communication, file output [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)
- Added `UTILITIES_GUIDE.md` coding guide for Utilities module [`45a4837`](https://github.com/yuumixcode/aesir-inspector/commit/45a4837c99913708e8d16218cf4b9acf5459fbb2)

#### Changed
- Simplified `AESIR_INSPECTOR_CODE_STYLE_GUIDE.md` by removing #region Internal rules and simplifying Odin Inspector integration guidelines [`45a4837`](https://github.com/yuumixcode/aesir-inspector/commit/45a4837c99913708e8d16218cf4b9acf5459fbb2) [`cf6126c`](https://github.com/yuumixcode/aesir-inspector/commit/cf6126cc4f1388647bb7febf376459cc4fc5abd7)

---

## [0.3.0] - 2026-04-25

### Core

#### Added
- Added `AesirInspectorMenuItems` unified menu path and priority management class for Tools menu and Assets context menu [`77f3b1b`](https://github.com/yuumixcode/aesir-inspector/commit/77f3b1b219eda197a67637c49c24c54ba4e8e0a5)
- Added Getting Started window with version display, feature list, and documentation links [`77f3b1b`](https://github.com/yuumixcode/aesir-inspector/commit/77f3b1b219eda197a67637c49c24c54ba4e8e0a5)
- Added Preferences window with integrated language settings [`77f3b1b`](https://github.com/yuumixcode/aesir-inspector/commit/77f3b1b219eda197a67637c49c24c54ba4e8e0a5)
- Added `AesirInspectorVersion` version info static class [`77f3b1b`](https://github.com/yuumixcode/aesir-inspector/commit/77f3b1b219eda197a67637c49c24c54ba4e8e0a5)
- Added `IAesirInspectorReset` interface and `AesirInspectorResetAttributeProcessor` for auto-adding context menu reset entry [`77f3b1b`](https://github.com/yuumixcode/aesir-inspector/commit/77f3b1b219eda197a67637c49c24c54ba4e8e0a5)
- Added `AesirCodeHighlighter` code syntax highlighter [`77f3b1b`](https://github.com/yuumixcode/aesir-inspector/commit/77f3b1b219eda197a67637c49c24c54ba4e8e0a5)

#### Changed
- Silenced installation detection log output (commented out `Debug.Log`) [`77f3b1b`](https://github.com/yuumixcode/aesir-inspector/commit/77f3b1b219eda197a67637c49c24c54ba4e8e0a5)
- Extended `AesirInspectorPaths` with AttributeOverview and MiniTools path constants [`77f3b1b`](https://github.com/yuumixcode/aesir-inspector/commit/77f3b1b219eda197a67637c49c24c54ba4e8e0a5)
- Extended `AesirInspectorWebLinks` with GitHub repository, license, changelog, and Odin Inspector docs links [`77f3b1b`](https://github.com/yuumixcode/aesir-inspector/commit/77f3b1b219eda197a67637c49c24c54ba4e8e0a5)

### Bilingual

#### Added
- Added `ShowEnablePropertyAttribute` composite attribute [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- Added `HorizontalSeparateWidget` horizontal separator Inspector component [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)

#### Changed
- Refactored `HeaderBilingualWidget` [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)

### Utilities

#### Added
- Added `AesirInspectorLogger` logging utility class [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- Added `PathUtility` and `PathSafeEditorUtility` path utility classes [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- Added `ReflectionUtility` reflection utility class [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- Added `RegexUtility` regular expression utility class [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- Added `HierarchyUtility` and `HierarchySafeEditorUtility` Hierarchy utility classes [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- Added `MonoScriptSafeEditorUtility` MonoScript utility class [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- Added `PlayerLoopUtility` PlayerLoop utility class [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- Added `PredefinedAssemblyUtility` predefined assembly utility class [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- Added `ProjectSafeEditorUtility` project-safe editor utility class [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)

#### Changed
- Extended `ScriptableObjectSafeEditorUtility` with additional ScriptableObject editor operation methods [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- Extended `OdinInspectorSafeEditorUtility` and `UrlUtility` [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)

### MiniTools

#### Added
- Added `AesirInspectorMiniToolsWindow` main window [`b7068eb`](https://github.com/yuumixcode/aesir-inspector/commit/b7068eb0d1ab0a6cf7d7db9293ae874516a59d89)
- Added MenuItemViewer with `IAssemblyFilter` assembly filtering and `ISearchFilterable` search support [`b7068eb`](https://github.com/yuumixcode/aesir-inspector/commit/b7068eb0d1ab0a6cf7d7db9293ae874516a59d89)
- Added OdinSyntaxHighlighter panel delegating to `AesirCodeHighlighter` [`b7068eb`](https://github.com/yuumixcode/aesir-inspector/commit/b7068eb0d1ab0a6cf7d7db9293ae874516a59d89)
- Added QuickCreateSO context menu tool for quick ScriptableObject generation with single and multi-selection support [`b7068eb`](https://github.com/yuumixcode/aesir-inspector/commit/b7068eb0d1ab0a6cf7d7db9293ae874516a59d89)

### ScriptDocGenerator

#### Added
- Added document generator window and visual panel ScriptableObject singleton [`c2f2e75`](https://github.com/yuumixcode/aesir-inspector/commit/c2f2e7503937cd2f4fae106938605464e512c332)
- Added `ScriptDocGeneratorController` logic controller for type analysis and document generation [`c2f2e75`](https://github.com/yuumixcode/aesir-inspector/commit/c2f2e7503937cd2f4fae106938605464e512c332)
- Added Assets context menu items for adding scripts to TargetType or TemporaryTypes [`c2f2e75`](https://github.com/yuumixcode/aesir-inspector/commit/c2f2e7503937cd2f4fae106938605464e512c332)
- Added Chinese Scripting API configuration and document generator settings [`c2f2e75`](https://github.com/yuumixcode/aesir-inspector/commit/c2f2e7503937cd2f4fae106938605464e512c332)
- Added complete type analysis data model layer: `MemberData`, `FieldData`, `PropertyData`, `MethodData`, `ConstructorData`, `EventData`, `ParameterData`, `TypeData` and corresponding interfaces [`c2f2e75`](https://github.com/yuumixcode/aesir-inspector/commit/c2f2e7503937cd2f4fae106938605464e512c332)
- Added `TypeAnalyzerStaticExtensions` and `TypeAnalyzerUtility` type analyzer utilities [`c2f2e75`](https://github.com/yuumixcode/aesir-inspector/commit/c2f2e7503937cd2f4fae106938605464e512c332)
- Added `AccessModifierType`, `TypeCategory`, `ParameterDirection` enumerations [`c2f2e75`](https://github.com/yuumixcode/aesir-inspector/commit/c2f2e7503937cd2f4fae106938605464e512c332)
- Added `DefaultAnalysisDataFactory`, `DefaultAttributeFilter`, `DerivedMemberDataComparer` core utilities [`c2f2e75`](https://github.com/yuumixcode/aesir-inspector/commit/c2f2e7503937cd2f4fae106938605464e512c332)
- Added `ReferenceLinkURLAttribute` reference link attribute [`c2f2e75`](https://github.com/yuumixcode/aesir-inspector/commit/c2f2e7503937cd2f4fae106938605464e512c332)

### AttributeOverview

#### Added
- Added `AttributeOverviewWindow` and `AttributeOverviewDatabaseSO` for attribute overview management [`0e53a40`](https://github.com/yuumixcode/aesir-inspector/commit/0e53a40ef55c26dab037f3f6aaf11cc09fbc9dad)
- Added panel abstract framework: generic base `AttributeOverviewPanelSO<T>`, `AbstractAttributePanelSO` with Odin AttributeProcessor auto-configuration [`0e53a40`](https://github.com/yuumixcode/aesir-inspector/commit/0e53a40ef55c26dab037f3f6aaf11cc09fbc9dad)
- Added AssetList, AssetsOnly, CustomValueDrawer built-in attribute panels [`0e53a40`](https://github.com/yuumixcode/aesir-inspector/commit/0e53a40ef55c26dab037f3f6aaf11cc09fbc9dad)
- Added `AesirExampleAttribute` and `AttributeCategoryAttribute` attribute markers [`0e53a40`](https://github.com/yuumixcode/aesir-inspector/commit/0e53a40ef55c26dab037f3f6aaf11cc09fbc9dad)
- Added attribute data models: `AbstractAttributeData`, `ParameterValue`, `ResolvedStringParameterValue`, `AttributeExamplePreviewItem` [`0e53a40`](https://github.com/yuumixcode/aesir-inspector/commit/0e53a40ef55c26dab037f3f6aaf11cc09fbc9dad)
- Added `AesirAttributeCategory` category enumeration and `OdinInspectorDocumentationLinks` documentation link constants [`0e53a40`](https://github.com/yuumixcode/aesir-inspector/commit/0e53a40ef55c26dab037f3f6aaf11cc09fbc9dad)
- Added attribute overview editor utility and usage examples [`0e53a40`](https://github.com/yuumixcode/aesir-inspector/commit/0e53a40ef55c26dab037f3f6aaf11cc09fbc9dad)

### SummaryTool

#### Added
- Added `XmlSummaryTool` for XML Summary comment processing with Sync/Replace/Remove operations [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- Added `XmlCodePart` XML code part parser [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- Added SummaryTool Assets context menu items [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)

### ExtensionManager

#### Added
- Added `ExtensionPackageManagerWindow` with Git URL installation support [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- Added `ExtensionPackageCard` extension package card data class [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- Added `PackageManagerEditorUtility` Package Manager editor utility class [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)

### Samples

#### Added
- Added PluginConfigSolutions sample demonstrating ScriptableSingleton usage in Preferences and Project [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)
- Added RuntimeInitializeLoadType sample demonstrating five initialization timings and best practices [`2ac8573`](https://github.com/yuumixcode/aesir-inspector/commit/2ac85734743e115703d19fd5e548e85acf52c65a)

### Tests

#### Added
- Added ScriptDocGenerator comprehensive unit tests covering constructors, events, fields, methods, properties, type data, and member inheritance [`1cf6d6d`](https://github.com/yuumixcode/aesir-inspector/commit/1cf6d6ddd746628a03917783ca41cf378155f093)
- Added SummaryTool XML comment processing tests [`1cf6d6d`](https://github.com/yuumixcode/aesir-inspector/commit/1cf6d6ddd746628a03917783ca41cf378155f093)
- Added UnityEngine.Object operator overload Runtime tests [`1cf6d6d`](https://github.com/yuumixcode/aesir-inspector/commit/1cf6d6ddd746628a03917783ca41cf378155f093)

#### Changed
- Added `ODIN_INSPECTOR` define constraint to both test asmdef files [`1cf6d6d`](https://github.com/yuumixcode/aesir-inspector/commit/1cf6d6ddd746628a03917783ca41cf378155f093)

---

## [0.2.1] - 2026-04-23

### Added

- Added Aesir Inspector installation detection feature [`b7de538`](https://github.com/yuumixcode/aesir-inspector/commit/b7de538eaf24efdd5b96d2c26d7c2897a8e6a6b5)

---

## [0.2.0] - 2026-04-23

### Added

- Implemented bilingual Inspector system and core infrastructure [`a2c750b`](https://github.com/yuumixcode/aesir-inspector/commit/a2c750b3e693f7a0d9d356e987a6fcf1ef8d59ea)
- Added Codely Skills Library sample, including custom-package-creator skill [`9422695`](https://github.com/yuumixcode/aesir-inspector/commit/942269529be76eba7425dcbd1ffcd6fef4964ece)

---

## [0.1.0] - 2026-04-22

### Added

- Initial release.

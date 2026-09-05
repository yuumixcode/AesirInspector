# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

[中文](../CHANGELOG.md)

---

## [0.13.0] - 2026-09-03

### Changed

- Version number synced with Aesir Architecture / Aesir Modules to `0.13.0`; no functional changes in this package for this release

---

## [0.12.0] - 2026-08-22

### Changed

- Version number synced with Aesir Architecture / Aesir Modules to `0.12.0`; no functional changes in this package for this release

---

## [0.11.0] - 2026-08-22

### Changed

- Version number synced with Aesir Architecture / Aesir Modules to `0.11.0`; no functional changes in this package for this release

---

## [0.9.0] - 2026-08-15

### Changed

- **Odin assembly rename** — `OdinIntegration` → `OdinInspector` (unified across the three packages):
  - Runtime: `Runestone.AesirInspector.OdinIntegration` → `Runestone.AesirInspector.OdinInspector`
  - Editor: `Runestone.AesirInspector.OdinIntegration.Editor` → `Runestone.AesirInspector.Editor.OdinInspector`
  - Directories `OdinIntegration/` → `OdinInspector/` (Runtime/Editor/Tests)
  - 6 referencing asmdefs updated (Tests + Samples~×3 + Assets Samples×3)
- **Doc sync** — assembly tables in aesir-inspector.md, dependency graph in development.md, and OdinIntegration references in README updated to OdinInspector

## [0.8.0] - 2026-08-06

### Changed

- Version number synced with Aesir Architecture / Aesir Modules to `0.8.0`; no functional changes in this package for this release

## [0.7.0] - 2026-08-05

### Changed

- Version number synced with Aesir Architecture / Aesir Modules to `0.7.0`; no functional changes in this package for this release

## [0.6.0] - 2026-08-01

### Added

- **Source file lookup and content cache**: new `SourceFileEntry` data container binding a `.cs` file path to its code content, with caching to avoid repeated reads
- **Fake XML comments inside block comments filtered out**: when parsing source, the parser now tracks `/* */` block-comment state line by line; `///` lines inside block comments are no longer misidentified as XML doc comments
- **Same-name types across assemblies distinguished**: the summary cache key now includes an assembly-name prefix (`AssemblyName.Namespace.TypeName.MemberName`), avoiding key collisions for identical namespace + type names in different assemblies
- **Overload summaries distinguished**: method summary keys now append the parameter type list (e.g. `MethodName(int, string)`), so each overload resolves independently. Multi-line parameter declarations spanning lines are supported
- **Nested type summaries**: summary lookups for nested types (e.g. `OuterClass.NestedStruct`) now work instead of incorrectly returning the outer class's summary
- **Generic type summaries**: summary lookups for generic types (e.g. `AbstractContext<T>`) now work
- **Source file lookup when file name mismatches type name**: when one `.cs` file defines multiple types and the file name matches none of them (e.g. `Capabilities.cs` defining 7 interfaces), the source file is found via a global content scan
- **Multi-assembly batch analysis mode**: `ScriptDocGeneratorSO.TypeSource` enum gained a `MultipleAssemblies` mode to analyze all types of multiple assemblies at once
- **Reflection parser moved to Runtime/Unity**: the 19 Runtime reflection-parser files moved from `Runtime/OdinIntegration` to `Runtime/Unity`, so the `[Summary]` and `[ReferenceLinkURL]` attributes no longer fall under the `ODIN_INSPECTOR` assembly constraint
- **Source parsing unit tests**: 34 new tests covering block comments, fully qualified keys, namespaces, single-line/multi-line summaries, multi-file merging, multi-line property declarations, generic methods, expression-bodied generic methods, overloaded methods, nested types, multi-line method declarations, and more
- **Overload prefix unit tests**: 4 new tests covering `[Overload]` prefixes for 2/3/4 overloaded methods and non-overloaded methods

### Changed

- **OdinBridge layer removed**: Odin is no longer invoked indirectly through the `IOdinBridge` interface; `Sirenix.Utilities` APIs are used directly behind `#if ODIN_INSPECTOR` conditional compilation
- **Module consolidation**: `ReflectionAnalyzer`, `SummaryTool`, and `OdinSourceFileHelper` consolidated under the `ScriptDocGenerator` module to reduce cross-layer fragmentation
- **Back to a single panel**: the 4 separate Panel SOs regressed to a single `ScriptDocGeneratorSO` + `TypeSource` enum mode switching
- **OdinSourceFileHelper slimmed down**: removed brace tracking, type-body location, and string sanitizing logic; only source file lookup and member name extraction remain
- **Summary resolution priority**: the `[Summary]` attribute is checked first and returned directly when present; otherwise parsing falls back to the source XML `/// <summary>` comment
- **Editor directory reorganization**: source parsing tools moved to `SourceFileTool/`, Summary tools to `SummaryAttributeTool/`

### Removed

- **OdinAutoTooltip feature**: removed the feature that auto-generated Inspector tooltips from source XML comments
- **OdinBridge pattern**: deleted `IOdinBridge`, `DefaultOdinBridge`, `OdinBridgeLocator`, and `OdinInspectorBridge` (4 files)
- **Multi-panel design**: deleted `ScriptDocGeneratorPanelBase` and the 4 Panel SOs (5 files)

### Fixed

- **XML comments inside block comments were misparsed**: when a `/* */` block comment spanned lines and one line started with `///`, that line was misidentified as an XML doc comment, extracting the wrong summary. Such `///` lines are now correctly ignored
- **Generic type summaries unresolvable**: analyzing a generic type (e.g. `AbstractContext<T>`) returned an empty summary; now resolves correctly
- **Type's own summary unresolvable**: analyzing the type itself returned an empty summary; now resolves correctly
- **Nested types returned the outer class's comment**: analyzing a nested type returned the outer class's summary; each type now returns its own
- **Member name extraction failed for multi-line property declarations**: when a property declaration spanned multiple lines, the member name could not be extracted and the summary was lost; now extracted correctly
- **Wrong member name for generic and expression-bodied generic methods**: the member name was incorrectly extracted as the constraint type name instead of the method name; now extracted correctly
- **Overloaded method summaries overwrote each other**: same-name overloads shared one cache key, so later summaries overwrote earlier ones; each overload is now distinguished by its parameter type list
- **`[Overload]` prefix appended repeatedly**: with N overloads, the `[Overload]` prefix was appended N-1 times; each overloaded method now gets the prefix exactly once
- **`ReferenceLinkURL` attribute displayed incomplete**: `[ReferenceLinkURL("https://...")]` showed as just `[ReferenceLinkURL]` in generated docs; now displayed in full with its argument
- **Source file not found when file name mismatches type names**: when one `.cs` file defined multiple types and the file name matched none, all type summaries were empty; the source file is now found via a global content scan
- **`null` keyword misextracted as member name**: the `return null;` statement caused `null` to be extracted as a member name; no longer extracted
- **Parameter type extraction failed for multi-line method declarations**: when `(` and `)` of a method declaration were not on the same line, the parameter type list could not be extracted; declaration text is now collected across lines until the parentheses match

## [0.5.0] - 2026-08-01

### Added

- **Odin Auto Tooltip (OdinAutoTooltip)** ⚡: an Odin attribute processor generating Inspector tooltips from source XML `/// <summary>` comments. Extracted from [JakePineOdinTools](https://github.com/JakePineGames/JakePineOdinTools) (MIT, © 2026 Jake Pine). When a tooltip already exists, the existing value is read, new content appended, and the original attribute dynamically replaced
- **ScriptDocGenerator source summary parsing**: `MemberData` gained a `SummaryResolver` delegate, injected at editor-assembly load, that reads member summaries from XML `/// <summary>` comments in `.cs` files
- **ScriptDocGenerator OdinMenuEditorWindow refactor**: the window was rewritten from `OdinEditorWindow` to `OdinMenuEditorWindow` with 4 work modes in the left menu (single script, multi script, single assembly, multi assembly), each with its own panel SO
- **Shared source parsing utilities**: `OdinSourceFileHelper` (source file location and member declaration extraction) and `SourceSummaryParser` (XML summary parsing), eliminating duplicated code between `SourceSummaryInitializer` and `OdinAutoTooltipAttributeProcessor`

### Changed

- Directory rename: `Odin Integration` → `OdinIntegration`
- **README top monorepo block rewritten**: from a bilingual side-by-side to a single-language version, clarifying that Aesir Inspector does **not** depend on other Aesir sub-packages (installable independently)
- **Third Party Notices updated**: placeholder content replaced with a record of the JakePineOdinTools third-party component
- **Summary tool marked as the recommended alternative**: the README now recommends OdinAutoTooltip for new code

### Removed

- **`[Summary]` attribute decoration removed**: all 897 `[Summary("...")]` decorations across 252 files were removed. The `SummaryAttribute` class remains as a fallback for ScriptDocGenerator compatibility
- **MIT LICENSE headers removed**: LICENSE headers were removed from all `.cs` files; one copy remains in `CodeStyle/AesirInspectorCodeStyle.cs`

### Fixed

- Fixed a bug in `ScriptDocGeneratorController.GenerateMultipleTypeDocs` where `generatorSettings` was treated as a bool

## [0.4.2] - 2026-07-24

### Changed

- Version number synced with Aesir Architecture / Aesir Modules to `0.4.2`; no functional changes in this package for this release

## [0.4.1] - 2026-07-24

### Changed

- **Samples version folder**: `Assets/Samples/Aesir Inspector/0.4.0-pre.1/` → `0.4.0/`, aligned with the `package.json` version

## [0.4.0] - 2026-07-24

### ⚠ BREAKING CHANGES (Read before upgrading)

> **Brand namespace unification**: all `RunLab` references were unified to `Runestone` (符文石), consistent with Aesir Architecture / Aesir Modules.
> All `RunLab.*` namespaces, the `cn.runlab.aesir-inspector` package name, and 9 asmdefs were renamed to `Runestone.*` / `cn.runestone.aesir.inspector`.
> After upgrading, **all code using this package needs a batch replace of `using RunLab.*` → `using Runestone.*`**.

#### Migration Guide

| Scope | Before | After |
|---|---|---|
| Package ID | `cn.runlab.aesir-inspector` | `cn.runestone.aesir.inspector` |
| Namespace | `RunLab.AesirInspector` | `Runestone.AesirInspector` |
| Namespace | `RunLab.AesirInspector.Editor` | `Runestone.AesirInspector.Editor` |
| Namespace | `RunLab.AesirInspector.Tests` | `Runestone.AesirInspector.Tests` |
| Namespace | `RunLab.AesirInspector.Editor.Tests` | `Runestone.AesirInspector.Editor.Tests` |
| Namespace | `RunLab.AesirInspector.OdinIntegration` | `Runestone.AesirInspector.OdinIntegration` |
| Namespace | `RunLab.AesirInspector.OdinIntegration.Editor` | `Runestone.AesirInspector.OdinIntegration.Editor` |
| Namespace | `RunLab.AesirInspector.Samples.*` | `Runestone.AesirInspector.Samples.*` |
| Assembly name | `RunLab.AesirInspector` (and all variants) | `Runestone.AesirInspector` (and all variants) |
| Copyright string | `Copyright (c) 2026 RunLab - Yuumix` | `Copyright (c) 2026 Runestone - Yuumix` |

#### Code-side replace examples

```csharp
// Before
using RunLab.AesirInspector;
using RunLab.AesirInspector.Editor;
using RunLab.AesirInspector.OdinIntegration;

// After
using Runestone.AesirInspector;
using Runestone.AesirInspector.Editor;
using Runestone.AesirInspector.OdinIntegration;
```

```jsonc
// asmdef references — Before
"references": [
  "RunLab.AesirInspector",
  "RunLab.AesirInspector.Editor"
]

// After
"references": [
  "Runestone.AesirInspector",
  "Runestone.AesirInspector.Editor"
]
```

#### Scope
- 422 .cs files / 12 asmdefs + 12 asmdef.metas / 1 package.json / 1 LICENSE.md / multiple README/CHANGELOG/CONTRIBUTING files

### Changed
- `OdinWrapper` renamed to `Odin Integration` (directories) / `OdinIntegration` (namespaces and assemblies) to express the integration-layer semantics more accurately
- `Runtime/Unity/Bilingualism/` renamed to `Runtime/Unity/Localization/`, aligning with the official Unity Localization package naming
- `Runtime/Unity/InspectorControls/` renamed to `Runtime/Unity/Inspector/`, adopting the Unity singular-noun convention
- `Runtime/Unity/Logger/` renamed to `Runtime/Unity/Logging/`, aligning with Unity source `Runtime/Export/Logging/` naming

---

## [0.4.0-pre.1] - 2026-04-29

### Architecture

#### Added
- New standalone `OdinWrapper` assembly with Runtime (`Runestone.AesirInspector.OdinWrapper`) and Editor (`Runestone.AesirInspector.OdinWrapper.Editor`) asmdefs, both with `defineConstraints: ODIN_INSPECTOR`, fully isolating the Odin Inspector dependency from core assemblies `473640f`

#### Changed
- Core Runtime assembly `Runestone.AesirInspector` removed the `ODIN_INSPECTOR` compile constraint and no longer hard-depends on Odin Inspector `473640f`
- Editor assembly `Runestone.AesirInspector.Editor` adjusted assembly references and no longer references Odin directly `473640f`

### OdinBridge

#### Added
- New `IOdinBridge` interface defining Odin availability queries such as `IsOdinPresent` `473640f`
- New `DefaultOdinBridge`, the fallback implementation used when Odin is absent `473640f`
- New `OdinBridgeLocator`, locating an Odin bridge automatically or falling back to the default implementation `473640f`
- New `OdinInspectorBridge` (`OdinWrapper/Editor/Bridge/`), the editor-side bridge used when Odin is available `473640f`

### OdinWrapper

#### Added
- New `OdinWrapper/Editor/AttributeProcessors/` directory with 5 OdinAttributeProcessors: `AesirInspectorLanguageSettingsProcessor`, `AesirInspectorResetProcessor`, `BilingualDisplayAsStringProcessor`, `BilingualHeaderProcessor`, `HorizontalSeparateProcessor` `473640f`

#### Changed
- `Editor/AttributeOverviewPro/` moved to `OdinWrapper/Editor/AttributeOverviewPro/` `473640f`
- `Editor/Drawers/Bilingual/` moved to `OdinWrapper/Editor/Drawers/` `473640f`
- `Editor/ExtensionManager/` moved to `OdinWrapper/Editor/ExtensionManager/` `473640f`
- `Editor/MiniTools/` moved to `OdinWrapper/Editor/MiniTools/` `473640f`
- `Editor/ScriptDocGenerator/` moved to `OdinWrapper/Editor/ScriptDocGenerator/` `473640f`
- `Editor/Core/Windows/` moved to `OdinWrapper/Editor/Windows/` `473640f`
- The 6 Bilingual attributes under `Runtime/Bilingual/Attributes/` moved to `OdinWrapper/Runtime/Attributes/` `473640f`
- `Editor/Core/AesirCodeHighlighter.cs` moved to `OdinWrapper/Runtime/OdinCodeHighlighter.cs` `473640f`
- `OdinSyntaxHighlighterSO` renamed to `OdinSyntaxHighlighterPanelSO` `473640f`

### Bilingualism

#### Changed
- `Runtime/Bilingual/` renamed to `Runtime/Bilingualism/` `473640f`
- `AesirInspectorLanguageSettingsSO` slimmed down, removing Odin-dependent logic now handled by `AesirInspectorLanguageSettingsProcessor` `473640f`

#### Removed
- Removed `DisplayAsStringBilingualConfigAttribute`, replaced by `BilingualDisplayAsStringControl` + Processor `473640f`
- Removed `ShowIfChineseAttribute` and `ShowIfEnglishAttribute`, replaced by the Processor `473640f`
- Removed `DisplayAsStringBilingualWidget` and `HeaderBilingualWidget`, replaced by the corresponding Controls `473640f`

### InspectorControls

#### Added
- New `BilingualDisplayAsStringControl`, replacing `DisplayAsStringBilingualWidget` `473640f`
- New `BilingualHeaderControl`, replacing `HeaderBilingualWidget` `473640f`

#### Changed
- `Runtime/InspectorWidgets/` renamed to `Runtime/InspectorControls/`; Widget uniformly renamed to Control `473640f`
- `HorizontalSeparateWidget` renamed to `HorizontalSeparateControl` `473640f`

### Core

#### Changed
- `IAesirInspectorReset` interface definition slimmed; reset logic moved to `AesirInspectorResetProcessor` `473640f`
- `AesirInspectorLogger` moved from `Runtime/Core/` to `Runtime/Logger/` `473640f`
- `AesirInspectorLoggerSettings` moved from `Runtime/Core/` to `Runtime/Logger/` `473640f`
- `SummaryAttribute` moved from `Runtime/Attributes/Docs/` to `Runtime/Attributes/`, flattening the directory `473640f`

#### Removed
- Removed the deprecated `ShowEnablePropertyAttribute` `473640f`

### Utilities

#### Changed
- `ReflectionUtility` greatly enhanced with new reflection helper methods `473640f`

#### Removed
- Removed `OdinInspectorSafeEditorUtility`, replaced by the OdinBridge pattern `473640f`

### ScriptDocGenerator

#### Changed
- All AnalysisData classes (`ConstructorData`, `EventData`, `FieldData`, `MemberData`, `MethodData`, `ParameterData`, `ParameterDirection`, `PropertyData`, `TypeData`) removed their Odin attribute dependencies `473640f`

### Samples

#### Changed
- `Samples~/` moved to `Samples/` (Plugin Config Solutions, RuntimeInitializeLoadType), making the samples directory user-visible `473640f`

#### Removed
- Removed the Codely Skills Library sample (custom-package-creator) `473640f`

### Tests

#### Changed
- `Runestone.AesirInspector.Tests` asmdef removed the `ODIN_INSPECTOR` compile constraint `473640f`
- `Runestone.AesirInspector.Editor.Tests` asmdef adjusted assembly references `473640f`
- Multiple test files received code formatting and region reordering, and unused using directives were removed `473640f`

### Code Style

#### Changed
- Updated `AESIR_INSPECTOR_CODE_STYLE.cs` code style guide to match the new assembly architecture and naming conventions `473640f`

---

## [0.3.1] - 2026-04-27

### Core

#### Added
- New `AesirInspectorLoggerSettings` ScriptableObject controlling log output via `enableInfoLog` (default false) and `enableWarningLog` (default true) `45a4837`

#### Changed
- `AesirInspectorLogger` moved from Utilities to the Core directory; Info/Warning methods integrated the `AesirInspectorLoggerSettings` switches and the `MethodImpl` attribute was removed `45a4837`
- `AesirInspectorWebLinks` renamed `GitWebsite` to `GitUrl`, and `OdinInspectorDocsUrl` changed from documentation to tutorials `45a4837`
- `IAesirInspectorReset` context menu label changed from "Aesir Toolkit Reset" to "Aesir Inspector Reset" `45a4837`
- `AesirInspectorMenuItems` menu path refactor: `ToolsMenuRoot` split into `ToolsAesirRoot` (Tools/Aesir) and `ToolsAesirInspectorRoot` (Tools/Aesir/Inspector), adding priority constants for each menu item `cf6126c`
- `AesirCodeHighlighter` removed the `#if UNITY_EDITOR && ODIN_INSPECTOR_3_3` wrapper; using statements moved outside the namespace `cf6126c`

#### Removed
- Removed all project-wide `#if ODIN_INSPECTOR_3_3` preprocessor directives; Odin Inspector became a hard dependency `cf6126c`

### Bilingual

#### Changed
- `AesirInspectorLanguageSettings` renamed to `AesirInspectorLanguageSettingsSO`, matching ScriptableObject naming conventions `cf6126c`
- `DisplayAsStringBilingualWidgetConfigAttribute` renamed to `DisplayAsStringBilingualConfigAttribute`, dropping the middle word Widget `cf6126c`
- `BilingualData` moved from `Runtime/Bilingual/Attributes/` to `Runtime/Bilingual/` `cf6126c`
- `HeaderBilingualWidget` fields `_chineseIntroduction` and `_englishIntroduction` marked readonly; conditional compilation changed from `#if ODIN_INSPECTOR_3_3` to `#if UNITY_EDITOR` `45a4837` `cf6126c`
- `BilingualBoxGroupAttribute` and `BilingualButtonAttribute` removed `#region Internal` `cf6126c`
- `BilingualTitleGroupAttribute`'s `TitleAlignment` property moved out of the `#if ODIN_INSPECTOR_3_3` wrapper `cf6126c`

#### Removed
- Removed the `#if ODIN_INSPECTOR_3_3` macros from all Bilingual attributes and drawers `cf6126c`

### AttributeOverviewPro

#### Changed
- The whole `Editor/AttributeOverview/` directory renamed to `Editor/AttributeOverviewPro/` `cf6126c`
- `AttributeExamplePreviewItem`, `ParameterValue`, and `ResolvedStringParameterValue` under the internal `Data/` directory moved to the `Core/` subdirectory `cf6126c`
- `AssetListExampleForCustomFilterMethodSO` renamed to `AssetListExampleWithCustomFilterMethodSO` `cf6126c`

### Utilities

#### Changed
- `OdinInspectorSafeEditorUtility`: `new T[0]` replaced with `Array.Empty<T>()`, `new Type[1]` replaced with `new[]` `45a4837`
- `PathSafeEditorUtility.EnsureDirectoryExists` added `[Conditional("UNITY_EDITOR")]` `45a4837`

#### Removed
- Removed the `#region Public Methods` and `#region` patterns from all Utility classes `45a4837`

### MiniTools

#### Changed
- `AssemblyFilterExample` renamed to `FilterOutAesirInspectorAssembly` `cf6126c`

#### Removed
- Removed the `#if ODIN_INSPECTOR_3_3` macros from the MiniTools module `cf6126c`

### ScriptDocGenerator

#### Changed
- All AnalysisData classes moved Odin attributes before XML comments `cf6126c`

#### Removed
- Removed the `#if ODIN_INSPECTOR_3_3` macros from the ScriptDocGenerator module `cf6126c`

### Code Style

#### Changed
- `HorizontalSeparateWidget` fields `_darkLineHeight`, `_lightLineHeight`, `_spaceAfter`, and `_spaceBefore` marked readonly; `DarkLineColor` and `LightLineColor` properties made static `cf6126c`

#### Removed
- Removed the `#region Internal` pattern and updated the code style guide with example code `45a4837` `cf6126c`

### Samples

#### Changed
- PluginConfig sample directory renamed `58fdbce`

### Docs

#### Added
- New `ATTRIBUTE_OVERVIEW_PRO_GUIDE.md` covering the AttributeOverviewPro module: Data-Panel-Example trio, singleton SO pattern, OdinAttributeProcessor injection, GUITable caching, the bilingual system, naming cheat sheet, etc. `cf6126c`
- New `SCRIPT_DOC_GENERATOR_GUIDE.md` covering ScriptDocGenerator module coding standards: architecture layers, singletons, reset, event communication, file output, etc. `cf6126c`
- New `UTILITIES_GUIDE.md` covering Utilities coding guidelines `45a4837`

#### Changed
- `AESIR_INSPECTOR_CODE_STYLE_GUIDE.md` removed the #region Internal rule and simplified the Odin Inspector integration guidelines `45a4837` `cf6126c`

---

## [0.3.0] - 2026-04-25

### Core

#### Added
- New `AesirInspectorMenuItems` unifying menu paths and priorities for the Tools menu and Assets context menu `77f3b1b`
- New Getting Started window showing version, feature list, and documentation links `77f3b1b`
- New Preferences window integrating language settings `77f3b1b`
- New `AesirInspectorVersion` static class for version info `77f3b1b`
- New `IAesirInspectorReset` reset interface and `AesirInspectorResetAttributeProcessor`, automatically adding a right-click reset menu to implementing classes `77f3b1b`
- New code syntax highlighter `AesirCodeHighlighter` `77f3b1b`

#### Changed
- Silenced installation-detection log output (commented out `Debug.Log`) `77f3b1b`
- Extended `AesirInspectorPaths` with AttributeOverview and MiniTools paths `77f3b1b`
- Extended `AesirInspectorWebLinks` with the GitHub repository, license, changelog, and Odin Inspector documentation links `77f3b1b`

### Bilingual

#### Added
- New `ShowEnablePropertyAttribute` composite attribute `2ac8573`
- New `HorizontalSeparateWidget` horizontal separator Inspector widget `2ac8573`

#### Changed
- Refactored `HeaderBilingualWidget` `2ac8573`

### Utilities

#### Added
- New `AesirInspectorLogger` logging utility `2ac8573`
- New `PathUtility` and `PathSafeEditorUtility` path utilities `2ac8573`
- New `ReflectionUtility` reflection utility `2ac8573`
- New `RegexUtility` regex utility `2ac8573`
- New `HierarchyUtility` and `HierarchySafeEditorUtility` hierarchy utilities `2ac8573`
- New `MonoScriptSafeEditorUtility` MonoScript utility `2ac8573`
- New `PlayerLoopUtility` PlayerLoop utility `2ac8573`
- New `PredefinedAssemblyUtility` predefined assembly utility `2ac8573`
- New `ProjectSafeEditorUtility` project safe editor utility `2ac8573`

#### Changed
- Extended `ScriptableObjectSafeEditorUtility` with many ScriptableObject editor operations `2ac8573`
- Extended `OdinInspectorSafeEditorUtility` and `UrlUtility` `2ac8573`

### MiniTools

#### Added
- New `AesirInspectorMiniToolsWindow` main window `b7068eb`
- New MenuItemViewer with `IAssemblyFilter` assembly filtering and `ISearchFilterable` search `b7068eb`
- New OdinSyntaxHighlighter panel delegating to `AesirCodeHighlighter` `b7068eb`
- New QuickCreateSO context menu for generating ScriptableObjects, supporting single and multi-select batch creation `b7068eb`

### ScriptDocGenerator

#### Added
- New documentation generator window and visual panel ScriptableObject singletons `c2f2e75`
- New `ScriptDocGeneratorController` logic controller `c2f2e75`
- New Assets context menu entries for adding scripts to TargetType or TemporaryTypes `c2f2e75`
- New Chinese Scripting API configuration and doc generator settings `c2f2e75`
- New complete type-analysis data model layer: `MemberData`, `FieldData`, `PropertyData`, `MethodData`, `ConstructorData`, `EventData`, `ParameterData`, `TypeData` with corresponding interfaces `c2f2e75`
- New type analyzer static extensions `TypeAnalyzerStaticExtensions` and utility `TypeAnalyzerUtility` `c2f2e75`
- New enums `AccessModifierType`, `TypeCategory`, `ParameterDirection` `c2f2e75`
- New core helpers `DefaultAnalysisDataFactory`, `DefaultAttributeFilter`, `DerivedMemberDataComparer` `c2f2e75`
- New `ReferenceLinkURLAttribute` reference link attribute `c2f2e75`

### AttributeOverview

#### Added
- New attribute overview window `AttributeOverviewWindow` and database `AttributeOverviewDatabaseSO` `0e53a40`
- New panel abstraction framework: generic base `AttributeOverviewPanelSO<T>`, `AbstractAttributePanelSO`, and automatic Odin AttributeProcessor configuration `0e53a40`
- Three built-in attribute panels: AssetList, AssetsOnly, CustomValueDrawer `0e53a40`
- New markers `AesirExampleAttribute` and `AttributeCategoryAttribute` `0e53a40`
- New data models `AbstractAttributeData`, `ParameterValue`, `ResolvedStringParameterValue`, `AttributeExamplePreviewItem` `0e53a40`
- New `AesirAttributeCategory` enum and `OdinInspectorDocumentationLinks` constants `0e53a40`
- New attribute overview editor utilities and usage examples `0e53a40`

### SummaryTool

#### Added
- New `XmlSummaryTool` XML comment processor supporting Sync/Replace/Remove `2ac8573`
- New `XmlCodePart` XML code part parser `2ac8573`
- New SummaryTool Assets context menu entries `2ac8573`

### ExtensionManager

#### Added
- New `ExtensionPackageManagerWindow` supporting Git URL installs `2ac8573`
- New `ExtensionPackageCard` package card data class `2ac8573`
- New `PackageManagerEditorUtility` Package Manager editor utility `2ac8573`

### Samples

#### Added
- New PluginConfigSolutions sample module demonstrating ScriptableSingleton usage in Preferences and Project `2ac8573`
- New RuntimeInitializeLoadType sample module demonstrating the execution order and best practices of the five initialization timings `2ac8573`

### Tests

#### Added
- New complete ScriptDocGenerator unit tests covering constructor, event, field, method, property, and type data plus member inheritance `1cf6d6d`
- New SummaryTool XML comment processing tests `1cf6d6d`
- New UnityEngine.Object operator overload Runtime test `1cf6d6d`

#### Changed
- Added `ODIN_INSPECTOR` compile constraints to both test asmdefs `1cf6d6d`

---

## [0.2.1] - 2026-04-23

### Added

- New Aesir Inspector installation mode detection `b7de538`

---

## [0.2.0] - 2026-04-23

### Added

- Implemented the bilingual Inspector system and core infrastructure `a2c750b`
- Added the Codely Skills Library sample with the custom-package-creator skill `9422695`

---

## [0.1.0] - 2026-04-22

### Added

- Initial release.

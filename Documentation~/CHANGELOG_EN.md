# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

[中文](../CHANGELOG.md)

---

## [0.21.1] - 2026-10-02

### Added

- **Third-party attribution**: `Third Party Notices.md` now credits [Odin-Resolved-Parameters-Overview](https://github.com/schwapo/Odin-Resolved-Parameters-Overview) (by schwapo, MIT), whose parameter listing informed how the parameter and resolved-string-parameter tables of Attribute Overview Pro present their information; the third-party tables in both package READMEs and the repository README were updated accordingly. / 第三方参考署名：`Third Party Notices.md` 新增对 Odin-Resolved-Parameters-Overview（作者 schwapo，MIT）的引用说明——Attribute Overview Pro 的「参数 / 解析字符串参数」表格参考了该项目的信息组织与呈现方式；包内中英 README 与仓库根 README 的第三方参考表已同步补充。

### Fixed

- **`Check Odin Dependency` menu ordering**: the item used to sit at the top of the `Tools/Aesir/Inspector` submenu; it is now fixed at the bottom (priority `-975` → `-789`, i.e. after Attribute Overview Pro `-900`, Mini Tools `-885`, Preferences `-880` and Samples `-800`), separated from the previous item by more than 10 so Unity draws a separator. Comments now also record that this value determines where the `Tools/Aesir` root menu sits in the top-level `Tools` menu. / `Check Odin Dependency` 菜单排序：该菜单项由子菜单最上方移到最下方（优先级 `-975` → `-789`），并与上一项拉开大于 10 的间隔以画出分割线；注释同时说明该值会决定 `Tools/Aesir` 根菜单在 `Tools` 顶层菜单中的位置。

## [0.21.0] - 2026-09-27

### Removed

- **Removed the `RuntimeInitializeLoadType` sample**: the sample (demonstrating the execution order and best practices of the five `RuntimeInitializeOnLoadMethod` timings) is now maintained in the Aesir Architecture package and no longer ships with this one. The `package.json` samples list and the sample tables in both READMEs were updated accordingly, leaving Plugin Config Solutions as the only bundled sample. Projects that already imported the sample are unaffected and may delete the old sample folder. / 移除 `RuntimeInitializeLoadType` 示例：已迁移至 Aesir Architecture 包维护，不再随本包分发；samples 列表与双语 README 示例表已同步移除，本包自此仅保留 Plugin Config Solutions 一个示例，已导入旧示例的工程不受影响。

## [0.20.1] - 2026-09-27

### Changed

- **Namespace-independent editor config keys**: `AesirInspectorSettings<T>`, `MenuItemViewerSO` and `OdinSyntaxHighlighterPanelSO` used the type's full name (including its namespace) as their `EditorBuildSettings` config key, so any namespace change (e.g. `RunLab` → `Runestone`) invalidated the key and made every resolution call `AddConfigObject` again, leaving `ProjectSettings/EditorBuildSettings.asset` permanently dirty. Keys are now the stable `AesirInspector/<assetName>` form (for example `AesirInspector/AesirInspectorProjectSettingsSO`); resolution migrates legacy keys (`RunLab.*`, `Runestone.*` full names and the readable full names used by the two MiniTools assets) to the new key and removes them — including entries left with a null reference. `ScriptableObjectSafeEditorUtility.GetOrCreateEditorScriptableObject<T>` gains an optional `legacyConfigNames` parameter (the previous signature keeps working) / 编辑器配置键改为与命名空间无关的稳定字符串 `AesirInspector/资产名`，并自动迁移与清理历史键；`GetOrCreateEditorScriptableObject<T>` 新增可选参数 `legacyConfigNames`。

## [0.20.0] - 2026-09-27

### Added

- **Bootstrap assembly and the "Check Odin Dependency" menu**: without Odin every package assembly used to be skipped silently by the `ODIN_INSPECTOR` constraint, leaving a package that appeared to do nothing. The new assembly `Runestone.AesirInspector.Bootstrap` deliberately carries no such constraint and references no other package assembly, so it still compiles without Odin and reports the situation from `Tools → Aesir → Inspector → Check Odin Dependency` (Odin detected / not detected, hence not compiled), along with the current install mode (UPM or Assets); the check is install-mode agnostic / 启动程序集与菜单：未装 Odin 时仍会编译，弹窗告知检测结果与安装方式。
- **Package-location anchor asset and `AesirPackagePaths`**: the install location is resolved through a fixed GUID (`Assets/…` or `Packages/…`), mirroring Odin's `SirenixAssetPaths`; AssetSelector example folders are injected by `AesirExampleAssetSelectorProcessor` while the property tree is built, leaving the literal paths in the example source untouched / 包位置锚点资产与 `AesirPackagePaths`：通过固定 GUID 定位包安装位置，AssetSelector 示例目录由 Processor 注入。

### Changed

- **Attribute Overview Ultra → Attribute Overview Pro (breaking)**: folders, types, state store, asset and menu entry are renamed consistently (`AttributeOverviewUltra*` → `AttributeOverviewPro*`, `UltraStateStoreSO` → `ProStateStoreSO`, `UltraPanelDatabase` → `ProPanelDatabase`, `UltraStateStore.asset` → `ProStateStore.asset`), the menu entry is `Tools → Aesir → Inspector → Attribute Overview Pro`, and the data subfolder becomes the space-free `AttributeOverviewPro`; `AesirInspectorDataFolderMigration` moves each step automatically with GUIDs, user state and `EditorBuildSettings` references preserved / Attribute Overview Ultra 一并更名为 Pro（破坏性），含目录、类型、状态存储、资产与菜单项，升级自动迁移。
- **Editor data folder renamed to `AesirInspectorData` (breaking path change)**: `Assets/Editor Default Resources/Aesir Inspector/` → `Assets/Editor Default Resources/AesirInspectorData/`, matching Unity Addressables' `AddressableAssetsData`; migrated automatically with GUIDs preserved, and the folder lives inside Unity's editor-only `Editor Default Resources` special folder, so it is never included in builds / 编辑器数据目录更名为 `AesirInspectorData`，自动迁移且不打包。
- **Removed the hard `com.unity.test-framework` dependency**: consumer projects no longer get the test framework forced in; the packaged test assembly keeps `UNITY_INCLUDE_TESTS` + `ODIN_INSPECTOR` constraints and `autoReferenced: false`, so projects without the test framework never compile it and see no extra noise / 移除 test-framework 硬依赖，测试程序集保留约束与 `autoReferenced: false`。
- **Removed the Getting Started window**: the window and its menu entry are gone; install-mode detection and the Odin dependency notice now live in the `Check Odin Dependency` menu / 移除 Getting Started 窗口，其职责由 `Check Odin Dependency` 菜单承担。
- **`EnsureAesirInspectorDefine` now enumerates build targets explicitly**: instead of reflecting `NamedBuildTarget`'s static fields it walks `BuildTargetGroup` through `NamedBuildTarget.FromBuildTargetGroup` (deduplicated by target name), keeping the "write `AESIR_INSPECTOR` for every platform" semantic (Aesir Architecture's `#if !AESIR_INSPECTOR` is a compile-time switch, so writing only the active platform would reintroduce duplicate functionality after switching platforms) / `EnsureAesirInspectorDefine` 改为显式枚举构建目标，保留全平台写宏的语义。
- **Install docs now pin a version tag**: the Git URL install addresses in both READMEs and the repository README carry `#v0.20.0`, noting that `main` is the development branch and production should pin a tag / 安装地址改用版本标签，并说明 `main` 为开发分支。
- **"Ping Script File" under a UPM installation**: now opens the example script source in the code editor (independent of whether the Project window shows Packages), falling back to revealing the file on disk when no MonoScript is found; Assets installations are unchanged / UPM 下改为打开示例脚本源码，找不到时在文件管理器中揭示。
- **Fixed first-time creation of editor data folders**: the new `PathSafeEditorUtility.EnsureAssetFolderExists` creates folders level by level with `AssetDatabase.CreateFolder` (immediately usable as the parent of `CreateAsset`, without a full `AssetDatabase.Refresh`); the first-time creation of the state store, Preferences and MiniTools now goes through it / 新增逐级 `CreateFolder` 工具并用于首次创建。
- **Docs**: new README sections ("Odin Dependency Check", "UPM vs. Assets installation") plus tag-pinning notes / 文档新增「Odin 依赖检查」「UPM 安装与 Assets 安装的差异」小节与锁定版本说明。

### Fixed

- **"Ping Script File" failed entirely under a UPM installation when the Project window hid Packages**: the previous implementation built an `Assets/../Library/PackageCache/...` path that could never resolve and returned null silently; the button now resolves the MonoScript by file name and opens its source / UPM 且隐藏 Packages 时 Ping 完全失效，现按文件名反查并打开源码。
- **AssetSelector examples were empty and a FolderPath example pointed at a missing folder (UPM install)**: the hard-coded package paths do not exist under `Packages/`, and `FolderPathExampleSO` now uses `ParentFolder = "Assets"` / UPM 下 AssetSelector 下拉为空、FolderPath 示例指向不存在目录。

## [0.19.0] - 2026-09-27

### Changed

- **Examples of group attributes are split by parameter topic (10 panels)**: examples for Box Group / Button Group / Horizontal Group / Vertical Group / Tab Group / Title Group / Toggle Group / Responsive Button Group / Show If Group / Hide If Group no longer use `FoldoutGroup` sections inside a single example; each parameter topic is now its own example whose name is the parameter label. A section label sharing a member with the group attribute under test is drawn inside that group by Odin (in horizontal groups the title squeezes into the button row, in vertical groups it stacks as an extra small heading), so each example now draws only the group under test. 24 example classes were added (Box Group's `BoxGroupMemberReference` / `BoxGroupCombining`, Button Group's `ButtonGroupNamedGroup` / `ButtonGroupParameter` / `ButtonGroupResolvedString`, Horizontal Group's `HorizontalGroupWidth` / `HorizontalGroupMarginRight` / `HorizontalGroupGap` / `HorizontalGroupTitle`, Vertical Group's `VerticalGroupPadding` / `VerticalGroupCombining`, Tab Group's `TabGroupFixedHeight` / `TabGroupTabStyle` / `TabGroupLayouting` / `TabGroupCombining`, Title Group's `TitleGroupStyle` / `TitleGroupOrder` / `TitleGroupCombining`, Toggle Group's `ToggleGroupMemberReference` / `ToggleGroupTitle` / `ToggleGroupOrder` / `ToggleGroupCombining`, Responsive Button Group's `ResponsiveButtonGroupParameter` / `ResponsiveButtonGroupCombining`, all ending in `ExampleSO`; `ButtonGroupNamedGroupExampleSO` replaces `ButtonGroupExampleWithGroupNameSO`). Renaming an example makes the matching old selection record in the state store expire gracefully back to the default example / Group 类特性的案例按参数主题拆分（10 个面板）：由「单案例内用 `FoldoutGroup` 分段」改为「一个参数主题一个案例」（案例名即参数标签），因为分段标签与被测组特性挂在同一成员时会被 Odin 画进该组内部（横排组里标题挤进按钮行、竖向组里叠出多余小标题）；新增 24 个案例类，`ButtonGroupNamedGroupExampleSO` 由 `ButtonGroupExampleWithGroupNameSO` 更名而来，改名后状态存储中对应的旧选中记录自动失效并回落默认案例。
- **Parameter tables show `float` with its C# alias**: `float` parameters now render as `System.Single(float)` (CLR full name plus the C# alias) across 30 parameter rows in 19 Data files; other primitive types keep their CLR full names / 参数表的「返回值类型」列统一 `float` 的显示：`float` 参数显示为 `System.Single(float)`（CLR 全名 + C# 别名），覆盖 19 个 Data 文件 30 处，其余基元类型保持 CLR 全名不变。
- **Docs**: the version badges in both READMEs now read 0.19.0, and the migrated Script Doc Generator pointer was fixed — the old `Assets/ScriptDocGenerator/README.md` link no longer exists because the tool moved into the Aesir Modules package (`Editor/ScriptDocGenerator/`) / 文档：中英双语 README 的版本徽章更新至 0.19.0，并修正已迁出的 Script Doc Generator 指向（原链接已随工具迁入 Aesir Modules 包而不存在）。

### Fixed

- **Type Filter example preview was empty**: its fields are typed as the abstract class `BaseClass` and the interface `IMyInterface`, which Unity's native serialization cannot carry, so the property tree was empty and the preview area showed nothing. The example now uses `OdinAttributeExampleSO<T>` with `[OdinSerialize]` and the Odin serialization backend / Type Filter 案例预览全空：示例字段为抽象类与接口类型，Unity 原生序列化无法承载，预览区没有任何节点；改为 `OdinAttributeExampleSO<T>` + `[OdinSerialize]` 走 Odin 序列化。
- **Hide Reference Object Picker example could not demonstrate the attribute**: it was applied to `string` fields, and strings have no polymorphic object picker, so the annotated and plain fields looked identical. The example now contrasts a custom reference type on two `[HideReferenceObjectPicker]` fields against two plain fields / Hide Reference Object Picker 案例演示不了特性：原案例把特性挂在 `string` 字段上，字符串没有多态对象选择器，标注与未标注看不出差异；改为自定义引用类型字段的对照。
- **Hide Mono Script example showed no difference in the inline preview**: Odin forces the Script field hidden when drawing an inline editor (legacy IMGUI path), so no inline preview can show it. The example now uses two objects with identical fields (`HideMonoScriptDemoObject` annotated, `ShowMonoScriptDemoObject` not) plus two buttons that open the real Inspector windows via `GUIHelper.OpenInspectorWindow` / Hide Mono Script 案例在内联预览中看不到差异：Odin 绘制内联编辑器（遗留 IMGUI 路径）时强制隐藏 Script 字段，任何内联预览都体现不出差异；改为两个字段相同的对照对象加两个按钮，用 `GUIHelper.OpenInspectorWindow` 打开真实 Inspector 窗口对比。
- **Class-level attributes of examples no longer report value-resolution errors on the panel's example host field**: Odin's `TypeDefinitionAttributeProcessor` propagates class-level attributes of an example class (such as `ImageClassExampleSO`'s `[Image("typeBanner")]`) onto the panel field that hosts the current example (`AbstractAttributePanelSO.currentSelectedExample`). That field lives in the panel's property tree, so the attribute can only resolve member references in the panel's context and draws a red error box (the banner itself was always drawn correctly by the example's own property tree). The new `ExampleHostAttributeProcessor` strips value-type-defined attributes from the example host field only, which also clears the same propagation for `[Searchable]` and `[ShowOdinSerializedPropertiesInInspector]` / 修复 Attribute Overview Ultra 中示例类级特性在面板宿主字段上误报解析错误：Odin 会把示例类上的类级特性传播到承载当前示例的面板字段上，该字段的解析上下文是面板而非示例，因而画出红色错误框；新增 `ExampleHostAttributeProcessor` 仅剥离该字段上由值类型定义传播来的特性。

## [0.18.0] - 2026-09-25

### Changed

- **Breaking rename of the state bank**: `UltraStateBankSO` → `UltraStateStoreSO`, asset `UltraStateBank.asset` → `UltraStateStore.asset` (GUID preserved, no data migration needed), `UltraStateBankEntry` / `UltraBankEntryKind` → `UltraStateStoreEntry` / `UltraStateStoreEntryKind`, `LoadOrCreateBank()` → `LoadOrCreate()`, `AesirInspectorPaths.AttributeOverviewUltraStateBankPath` → `AttributeOverviewUltraStateStorePath`; the in-memory panel registry keeps its name `UltraPanelDatabase`. Entry key prefixes (`PanelSelection/`, `ExampleState/`) are unchanged.

### Fixed

- **Assets that auto-create on first access are no longer resolved from GUI draw callbacks**: `MenuItemViewerSO.Instance` / `OdinSyntaxHighlighterPanelSO.Instance` are now cached per domain, and the Getting Started window resolves `AesirInspectorProjectSettingsSO.Instance` in `OnEnable` instead of `[OnInspectorGUI]`. Previously a missing asset triggered `CreateAsset` plus a full `AssetDatabase.Refresh()` inside IMGUI callbacks, producing `the GUIStateObj is deleted, but is accessed` errors and potentially hanging the editor.

## [0.17.0] - 2026-09-25

### Added

- **`WrappedTextAttribute`**: Odin's `DisplayAsString` silently truncates when the column is too narrow (no wrapping, no ellipsis), losing the tail of long text; the new attribute wraps to the available width and exposes `LabelWidth` / `FontSize`. It backs the menu-path column of MenuItem Viewer / `WrappedTextAttribute` 自动换行只读文本特性：`DisplayAsString` 宽度不足时直接截断且不换行，长内容静默丢失尾部；新特性按可用宽度换行，支持 `LabelWidth` / `FontSize`。

### Changed

- **MenuItem Viewer polish**: the menu path gets its own wrapping row, priority and validate state share a two-column row, the collect button is renamed and gains a search icon, the assembly-filter title is shortened, and `MenuItemInfo` now keeps a serialized, hidden `methodName` for search instead of an exposed writable `MethodName` property / MenuItem Viewer 面板打磨：菜单路径独占一行自动换行、优先级与校验并排、按钮更名配图标、过滤器标题简化，并改用隐藏的 `methodName` 序列化字段做搜索。
- **Naming and copy fixes**: stale `OdinSyntaxHighlighterSO` / `AesirCodeHighlighter` references in `OdinSyntaxHighlighterPanelSO` now read `OdinSyntaxHighlighterPanelSO` / `OdinCodeHighlighter`, the `OdinCodeHighlighter` log prefix follows suit, and the syntax-highlighter panel plus Getting Started window use tighter wording / 命名与文案修正：修正 `OdinSyntaxHighlighterPanelSO` 中过时的 `OdinSyntaxHighlighterSO` / `AesirCodeHighlighter` 引用与 `OdinCodeHighlighter` 日志前缀，语法高亮面板与 Getting Started 窗口文案精简。
- **Repository and install URL fix**: the install URL, the `package.json` UPM metadata (`documentationUrl` / `changelogUrl` / `licensesUrl`) and `AesirInspectorWebLinks` now point at `yuumixcode/AesirInspector` instead of the `Unity-Aesir-Packages` monorepo (now `AesirFramework`), which does not contain this package path — the old URL 404s. The English README also drops the migrated `[Summary]` section and renumbers / 仓库与安装地址修正：安装地址、UPM 元数据与 `AesirInspectorWebLinks` 改为本包所在仓库 `yuumixcode/AesirInspector`（原 monorepo 现名 `AesirFramework`，不含本包路径，安装会 404）；英文 README 删除已迁出的 `[Summary]` 章节。

## [0.16.0] - 2026-09-25

### Added

- **In-package example assets**: `Editor/ExampleAssets/` ships placeholder assets (`AesirExampleAssetSO`, two ScriptableObject assets and two material folders) so examples that must point at real assets (`AssetSelector.Paths`) have a stable target; living inside an `Editor` folder they ship with the package but never enter builds / 包内案例资产目录 `Editor/ExampleAssets/`：为 `AssetSelector.Paths` 等必须指向真实资产的案例提供随包分发的占位资产，位于 `Editor/` 内故不进入构建。

### Changed

- **UPM metadata and links**: `package.json` gains `documentationUrl` / `changelogUrl` / `licensesUrl` and a bilingual `description`; `AesirInspectorWebLinks` now points LICENSE / CHANGELOG at the in-package files instead of non-existent repo-root files / UPM 元数据与链接：补齐 UPM 字段，修正 LICENSE / CHANGELOG 链接到包内文件。
- **Docs**: install URLs now consistently use the monorepo git URL with the `?path=` subfolder, sample-import instructions and the version badge were updated, and the English README dropped the Script Doc Generator / Summary Tool sections that no longer belong to this package. The bilingual READMEs and `development.md` now describe Odin as a defineConstraints dependency (every asmdef carries `ODIN_INSPECTOR`, so assemblies are skipped rather than failing to compile when Odin is absent); `development.md` also drops the migrated ScriptDocGenerator module and the removed `Editor/Common` / `AesirInspectorModuleAssetMarkerSO`, adds `ExampleAssets` and `AesirInspectorProjectSettingsSO`, and fixes stale `Runtime/Unity/Utilities` / `Editor/Unity` paths plus the migrated `[Summary]` convention / 文档：统一 git URL 安装地址、补充 Samples 导入说明、英文 README 清除已迁出本包的章节，并修正双语 README 与 development.md 的 Odin 约束语义、失效模块表与路径、已迁出的 `[Summary]` 约定。
- **Directory consolidation**: `Editor/AttributeOverviewPro/` merged into `Editor/AttributeOverviewUltra/` — `Abstract`, `AttributePanels`, `Data` and `UsageExamples` moved under Ultra and the two `Core` sets merged; namespaces, type names and asset GUIDs are unchanged / 目录整合：`Editor/AttributeOverviewPro/` 整体并入 `Editor/AttributeOverviewUltra/`，Pro 目录不再存在。

### Removed

- **Removed the Extension Package Manager**: the `ExtensionPackageManagerWindow`, `ExtensionPackageCard` and `PackageManagerEditorUtility` types and the `Tools → Aesir → Inspector → Extension Package Manager` menu entry are gone, together with the matching README and Getting Started entries / 移除扩展包管理器：删除对应三个类型与菜单项。
- **Removed the deprecated example migration guide** `AESIR_ATTRIBUTE_MIGRATION_GUIDE.md`; its grouping and workflow rules are now embodied by the examples themselves / 删除示例迁移指南 `AESIR_ATTRIBUTE_MIGRATION_GUIDE.md`。

## [0.15.0] - 2026-09-09

### ⚠ BREAKING CHANGES (Read before upgrading / 升级前必读)

> **The Script Doc Generator and Summary Tool were moved out of this package** into the standalone in-repo tool at `Assets/ScriptDocGenerator/` (not a UPM package; namespaces `Runestone.ScriptDocGenerator` / `Runestone.ScriptDocGenerator.Editor`). This package now focuses solely on Odin Inspector enhancements (bilingual attributes, Attribute Overview Ultra, safe editor utilities, extension package manager) / Script Doc Generator 与 Summary 工具已移出本包，迁移至仓库内独立工具 `Assets/ScriptDocGenerator/`。

> **Attribute Overview Pro has been removed** and replaced by the fully rebuilt Attribute Overview Ultra. Panels and examples are no longer persisted as sub-assets (`AttributeOverviewDatabase.asset`, `OdinExamples.asset` and `UnityExamples.asset` are deleted on upgrade); user debug state now persists as snapshots in the `UltraStateBank.asset` state bank, keeping the Project free of sub-assets. Example SO singletons (`AttributeExampleSO<T>.Instance` / `OdinAttributeExampleSO<T>.Instance`) are now routed through the in-memory state bank while keeping the `.Instance` accessor / Attribute Overview Pro 已移除，由 Attribute Overview Ultra 取代；面板与示例改为内存实例，调试状态经状态银行持久化，Project 中零子资产。

#### Migration Guide / 迁移指南

| Scope / 范围 | Before / 旧 | After / 新 |
|---|---|---|
| Code location | `Assets/Runestone/AesirInspector/{Runtime,Editor}/ScriptDocGenerator/` | `Assets/ScriptDocGenerator/{Runtime,Editor}/` |
| Namespace (Runtime) | `Runestone.AesirInspector` (ScriptDocGenerator part) | `Runestone.ScriptDocGenerator` |
| Namespace (Editor) | `Runestone.AesirInspector.Editor` (ScriptDocGenerator part) | `Runestone.ScriptDocGenerator.Editor` |
| Assemblies | merged into `Runestone.AesirInspector(.Editor)` | standalone `Runestone.ScriptDocGenerator(.Editor)` |
| Menus | `Tools → Aesir → Inspector → Script Doc Generator`, `Assets → Aesir Inspector → …` | `Tools → Script Doc Generator`, `Assets → Script Doc Generator → …` |
| Editor assets path | `Assets/Editor Default Resources/Aesir Inspector/…` | `Assets/Editor Default Resources/Script Doc Generator/…` |
| `[Summary]` / `[ReferenceLinkURL]` attributes | `Runestone.AesirInspector` | `Runestone.ScriptDocGenerator` |
| Attribute Overview window | `Tools → Aesir → Inspector → Attribute Overview Pro` (asset database + sub-asset examples) | `Tools → Aesir → Inspector → Attribute Overview Ultra` (in-memory panels + UltraStateBank state bank) |
| Editor assets path (Attribute Overview) | `Assets/Editor Default Resources/Aesir Inspector/Attribute Overview Pro/` | `Assets/Editor Default Resources/Aesir Inspector/Attribute Overview/` (only `UltraStateBank.asset`) |

### Added

- **Attribute Overview Ultra window**: replaces the asset database with TypeCache scanning and CreateInstance in-memory panels; menu tree, search, category browsing and code preview are on par with Pro, plus narrow-window defenses (draggable menu width + unified horizontal scrolling) and example debug-state snapshots persisted with SHA256 checksum and type-name double validation / **Attribute Overview Ultra 特性总览窗口**：TypeCache 扫描 + 内存实例化，树形菜单与代码预览对齐 Pro，新增窄窗防线与状态银行快照持久化。
- **Directory structure identical to Odin's official window**: categories, multi-category registration (an attribute can appear in several categories, e.g. Button in both Groups and Buttons), display names (official nice-name rule, e.g. `Assets Only`, `GUIColor`) and ordering (official CategoryComparer) are all read directly from Odin's official registry (`AttributeExampleUtilities`), requiring zero maintenance across Odin upgrades / **目录结构与 Odin 官方完全一致**：分类归属、多分类、显示名与分类排序全部直读 Odin 官方注册表，随 Odin 升级零维护。
- **Aesir Customs category**: added panels for the bilingual attributes (Bilingual Title / Bilingual Button / Bilingual Info Box / Bilingual Text) with full parameter tables and examples, pinned to the top of the menu / **Aesir Customs 分类**：新增双语特性面板，置于菜单首位。
- **Completed the Meta / Unity / Debug categories**: added 7 panels — SuppressInvalidAttributeError (Meta), ShowDrawerChain and ShowPropertyResolver (Debug), and Multiline / Range / Space / TextArea (Unity built-in attributes as drawn by Odin; Range also appears under Validation). The category set now matches Odin's official 12 categories exactly / **补齐 Meta / Unity / Debug 三个分类**：新增 7 个面板（SuppressInvalidAttributeError、ShowDrawerChain、ShowPropertyResolver、Multiline、Range、Space、TextArea），分类集合与官方 12 个分类完全一致。

### Changed

- **Filled every remaining attribute panel for full parity with Odin's window**: added 33 panels (with 6 examples ported from Odin's official examples); the Ultra menu now covers every official entry (verified: 0 missing, 0 extra). HideNetworkBehaviourFields ships without an example because the UNET module is not enabled / **补齐全部剩余特性面板，目录与官方完全对等**：新增 33 个面板（其中 6 个示例按官方移植），逐条比对缺失 0、多余 0；HideNetworkBehaviourFields 因未启用 UNET 模块不提供示例。
- Fixed a null-reference issue for panels without examples: the code-preview refresh is skipped when the example list is empty (it previously logged an "attribute 不能为空" error) / 修复无示例面板的空引用问题：示例列表为空时跳过示例代码预览刷新。
- Merged the duplicated OnInspectorInit and OnInspectorDispose examples: their "Basic Usage" and "Action" cases demonstrated exactly the same parameters, so each was merged into a single case (all unique usages kept). Other attributes' "With" variants are kept to separate basic from advanced parameters / 收敛 OnInspectorInit 与 OnInspectorDispose 的重复案例（演示参数完全相同，合并为单一案例），其余特性的 With 变体保留以区分简单/进阶参数。

- The tool panel UI no longer uses the bilingual attributes; plain Odin attributes with Chinese text are used instead / 工具面板 UI 移除双语特性，改为纯 Odin 特性（仅中文文本）。
- Removed `AesirInspectorModuleAssetMarkerSO` (its only consumer was Script Doc Generator, replaced by the tool's own `ScriptDocGeneratorAssetMarkerSO`) / 移除 `AesirInspectorModuleAssetMarkerSO`。
- The Getting Started initialize button no longer generates 100+ example assets; it now ensures the state bank exists and smoke-builds all panels once / Getting Started 初始化按钮不再生成案例资产，改为确保状态银行可用并完成一次全量面板构建冒烟。
- Removed the duplicate CustomValueDrawer panel from Misc (the Essentials registration matches Odin's official category), eliminating the duplicated menu entry / 清理 CustomValueDrawer 双胞胎面板，消除同名菜单项。

### Removed

- The Attribute Overview Pro window (`AttributeOverviewWindow`) and its asset database (`AttributeOverviewDatabaseSO`) / Attribute Overview Pro 窗口与资产数据库。

## [0.14.1] - 2026-09-05

### Fixed

- **Compile errors when importing into projects without Odin Inspector / 无 Odin Inspector 的项目导入后编译报错**: restored the `ODIN_INSPECTOR` defineConstraints on all 7 asmdefs (Runtime, Editor, Tests, Samples) so the assemblies are skipped entirely with zero errors when Odin is absent; behavior is unchanged when Odin is installed.

### Changed

- The Unity-based export was retired (the `Tools → Aesir → Inspector → Export Package` menu was removed); maintainer exports are unified on the .NET toolchain — local `Scripts/export-package.sh`, CI via GitHub Actions / 弃用 Unity 内置导出方案，维护者导出统一为 .NET 工具链。
- `AesirInspectorVersion.Version` is now in sync (`0.14.1`); it had been left at `0.4.0-pre.1` / `AesirInspectorVersion.Version` 同步为 `0.14.1`。

## [0.14.0] - 2026-09-05

### ⚠ BREAKING CHANGES (Read before upgrading / 升级前必读)

> **Architecture restructure / 架构重构**: Odin Inspector is upgraded to a **hard dependency**; the Unity/Odin dual-assembly isolation architecture is removed.
> The 4 assemblies are merged into 2, with directories and namespaces fully restructured — follow the migration guide below to update code references.
> 程序集从 4 个合并为 2 个，目录与命名空间全面重构，升级后需按下方迁移指南更新代码引用。

#### Migration Guide / 迁移指南

| Scope / 范围 | Before / 旧 | After / 新 |
|---|---|---|
| Assemblies (Runtime) | `Runestone.AesirInspector` + `Runestone.AesirInspector.OdinInspector` | `Runestone.AesirInspector` |
| Assemblies (Editor) | `Runestone.AesirInspector.Editor` + `Runestone.AesirInspector.Editor.OdinInspector` | `Runestone.AesirInspector.Editor` |
| Namespaces (Runtime) | `Runestone.AesirInspector` / `Runestone.AesirInspector.OdinIntegration` | `Runestone.AesirInspector` |
| Namespaces (Editor) | `Runestone.AesirInspector.Editor` / `Runestone.AesirInspector.OdinIntegration.Editor` | `Runestone.AesirInspector.Editor` |
| Directories (Runtime) | `Runtime/Unity/` + `Runtime/OdinInspector/` | `Runtime/` |
| Directories (Editor) | `Editor/Unity/` + `Editor/OdinInspector/` | `Editor/` |
| Conditional compilation | `#if ODIN_INSPECTOR` | Removed; Sirenix APIs used directly |
| defineConstraints | `ODIN_INSPECTOR` | Removed |

#### Code-side replace examples / 代码侧替换示例

```csharp
// Before / 旧
using Runestone.AesirInspector.OdinIntegration;
using Runestone.AesirInspector.OdinIntegration.Editor;

// After / 新
using Runestone.AesirInspector;
using Runestone.AesirInspector.Editor;
```

```jsonc
// asmdef references — Before / 旧
"references": [
  "Runestone.AesirInspector.OdinInspector",
  "Runestone.AesirInspector.Editor.OdinInspector"
]

// After / 新
"references": [
  "Runestone.AesirInspector",
  "Runestone.AesirInspector.Editor"
]
```

### Changed

- **Standard Unity Package layout**: single-level flat `Runtime/` and `Editor/` — Runtime: Attributes, Inspector, Localization, Utilities, ScriptDocGenerator, Common, Debug, CodeStyle; Editor: AttributeOverviewPro, AttributeProcessors, Common, Core, Drawers, ExtensionManager, MiniTools, ScriptDocGenerator, Windows
- **Odin Inspector as a hard dependency**: removed `ODIN_INSPECTOR` defineConstraints and all `#if ODIN_INSPECTOR` conditional compilation (4 guards now keep the Odin branch only); compilation fails outright without Odin
- **Assembly merge**: `Runestone.AesirInspector.OdinInspector` merged into `Runestone.AesirInspector`; `Runestone.AesirInspector.Editor.OdinInspector` merged into `Runestone.AesirInspector.Editor`
- **Tests restructured**: `Tests/Editor/OdinInspector/` flattened to `Tests/Editor/`; both test assemblies' references updated (keeping the `UNITY_INCLUDE_TESTS` constraint and Sirenix precompiled references)
- **Docs fully synced**: README (zh/en), development.md, CONTRIBUTING, and Third Party Notices updated for the new architecture
- **Docs trimmed**: removed `Documentation~/aesir-inspector.md` (duplicated the README) and the in-package `CONTRIBUTING.md` copy (the repo-root documents are the source of truth)
- **Docs now consistently reference the Unity engine**; Tuanjie wording removed

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

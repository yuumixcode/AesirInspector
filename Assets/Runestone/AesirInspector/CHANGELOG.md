# 变更日志

本项目所有重大变更都将记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，本项目遵循[语义化版本](https://semver.org/lang/zh-CN/)规范。

[English](Documentation~/CHANGELOG_EN.md)

---

## [0.20.1] - 2026-09-27

### Changed

- **编辑器配置键改为与命名空间无关的稳定字符串**：`AesirInspectorSettings<T>`、`MenuItemViewerSO`、`OdinSyntaxHighlighterPanelSO` 原先以「类型全名（含命名空间）」作为 `EditorBuildSettings` 的配置键，命名空间一旦调整（如 `RunLab` → `Runestone`）旧键就会失效，导致每次解析都重新执行 `AddConfigObject` 写一遍 `ProjectSettings/EditorBuildSettings.asset`，使该文件长期处于 dirty 状态。现统一为 `AesirInspector/资产名`（如 `AesirInspector/AesirInspectorProjectSettingsSO`）；解析时会自动把历史键（`RunLab.*`、`Runestone.*` 类型全名与 `MenuItemViewerSO` / `OdinSyntaxHighlighterPanelSO` 的可读全名）迁移到新键并清理，只剩空引用的旧键条目也会被移除。`ScriptableObjectSafeEditorUtility.GetOrCreateEditorScriptableObject<T>` 新增可选参数 `legacyConfigNames`（原签名继续可用）。 / **Namespace-independent editor config keys**: `AesirInspectorSettings<T>`, `MenuItemViewerSO` and `OdinSyntaxHighlighterPanelSO` used the type's full name (including its namespace) as their `EditorBuildSettings` config key, so any namespace change (e.g. `RunLab` → `Runestone`) invalidated the key and made every resolution call `AddConfigObject` again, leaving `ProjectSettings/EditorBuildSettings.asset` permanently dirty. Keys are now the stable `AesirInspector/<assetName>` form, and resolution migrates legacy keys (`RunLab.*`, `Runestone.*` full names and the readable full names used by the two MiniTools assets) to the new key while removing them — including entries left with a null reference. `ScriptableObjectSafeEditorUtility.GetOrCreateEditorScriptableObject<T>` gains an optional `legacyConfigNames` parameter (the previous signature keeps working).

## [0.20.0] - 2026-09-27

### Added

- **启动程序集 `Runestone.AesirInspector.Bootstrap` 与 `Check Odin Dependency` 菜单**：此前未安装 Odin 时包内全部程序集被 `ODIN_INSPECTOR` 约束静默跳过编译，用户只会拿到一个「装了却毫无反应」的包。该程序集刻意不带该约束、也不引用包内其它程序集，所以未装 Odin 时仍会编译，并在 `Tools → Aesir → Inspector → Check Odin Dependency` 弹窗说明情况（已检测到 Odin / 未检测到 Odin 因而不会参与编译），同时显示当前安装方式（UPM 或 Assets）；检测与安装方式无关。 / **Bootstrap assembly and the "Check Odin Dependency" menu**: without Odin every package assembly used to be skipped silently by the `ODIN_INSPECTOR` constraint, leaving a package that appeared to do nothing. The new assembly deliberately carries no such constraint and references no other package assembly, so it still compiles without Odin and reports the situation from the menu, including the current install mode (UPM or Assets).
- **包位置锚点资产 `AesirPathLookup.asset` 与 `AesirPackagePaths`**：通过固定 GUID 定位包安装位置（`Assets/…` 或 `Packages/…`），机制对齐 Odin 的 `SirenixAssetPaths`；AssetSelector 示例的包内目录改由 `AesirExampleAssetSelectorProcessor` 在属性树构建期注入，示例源码中的字面量路径保持不变。 / **Package-location anchor asset and `AesirPackagePaths`**: the install location is resolved through a fixed GUID, mirroring Odin's `SirenixAssetPaths`; AssetSelector example folders are injected by a processor while the property tree is built.

### Changed

- **Attribute Overview Ultra → Attribute Overview Pro（破坏性）**：目录、类型、状态存储与菜单项统一更名（`AttributeOverviewUltra*` → `AttributeOverviewPro*`、`UltraStateStoreSO` → `ProStateStoreSO`、`UltraPanelDatabase` → `ProPanelDatabase`、资产 `UltraStateStore.asset` → `ProStateStore.asset`），菜单项为 `Tools → Aesir → Inspector → Attribute Overview Pro`；数据子目录 `Attribute Overview` 一并改为无空格的 `AttributeOverviewPro`；升级由 `AesirInspectorDataFolderMigration` 逐步自动迁移，GUID、用户状态与 `EditorBuildSettings` 引用均无损。 / **Attribute Overview Ultra → Attribute Overview Pro (breaking)**: folders, types, state store, asset and menu entry are renamed consistently, the data subfolder becomes the space-free `AttributeOverviewPro`, and the migration helper moves each step automatically with GUIDs, user state and `EditorBuildSettings` references preserved.
- **编辑器数据目录更名为 `AesirInspectorData`（破坏性路径变更）**：`Assets/Editor Default Resources/Aesir Inspector/` → `Assets/Editor Default Resources/AesirInspectorData/`（对齐 Unity Addressables 的 `AddressableAssetsData`），自动迁移且 GUID 不变；该目录位于 `Editor Default Resources` 特殊文件夹下，只在编辑器阶段可用、不会打包进构建。 / **Editor data folder renamed to `AesirInspectorData` (breaking path change)**: migrated automatically with GUIDs preserved; it lives inside Unity's editor-only `Editor Default Resources` folder and is never included in builds.
- **移除 `com.unity.test-framework` 硬依赖**：消费工程不再被强制引入测试框架；包内测试程序集保留 `UNITY_INCLUDE_TESTS` + `ODIN_INSPECTOR` 约束且 `autoReferenced: false`，未启用测试框架的工程不会编译它，也不会产生噪音。 / **Removed the hard `com.unity.test-framework` dependency**: consumer projects no longer get the test framework forced in; the packaged test assembly keeps its constraints and `autoReferenced: false`, so it is never compiled or surfaced otherwise.
- **移除 Getting Started 窗口**：窗口与菜单项删除，安装方式检测与 Odin 依赖提示改由 `Check Odin Dependency` 菜单承担。 / **Removed the Getting Started window**: the window and its menu entry are gone; install-mode detection and the Odin notice now live in the `Check Odin Dependency` menu.
- **`EnsureAesirInspectorDefine` 改为显式枚举构建目标**：不再反射 `NamedBuildTarget` 的静态字段，改为遍历 `BuildTargetGroup` 经 `NamedBuildTarget.FromBuildTargetGroup` 构造并按目标名去重，保留「所有平台都写入 `AESIR_INSPECTOR`」的语义（Aesir Architecture 的 `#if !AESIR_INSPECTOR` 是编译期判断，只写当前平台会在切换平台后导致功能重复）。 / **`EnsureAesirInspectorDefine` now enumerates build targets explicitly** instead of reflecting `NamedBuildTarget`'s static fields, keeping the "write the define for every platform" semantic.
- **安装文档改用版本标签**：中英双语 README 与仓库 README 的 Git URL 安装地址带 `#v0.20.0`，并说明 `main` 为开发分支、生产环境请锁定标签。 / **Install docs now pin a version tag**: the Git URL addresses carry `#v0.20.0`, noting that `main` is the development branch.
- **「Ping 脚本文件」在 UPM 安装下的行为**：改为在代码编辑器中打开示例脚本源码（不依赖 Project 窗口是否显示 Packages），找不到 MonoScript 时退化为在文件管理器中揭示磁盘文件；Assets 安装行为不变。 / **"Ping Script File" under a UPM installation** now opens the example script source in the code editor, falling back to revealing the file on disk; Assets installations are unchanged.
- **首次创建编辑器数据目录的行为修正**：新增 `PathSafeEditorUtility.EnsureAssetFolderExists`（`AssetDatabase.CreateFolder` 逐级创建，创建后即可作为 `CreateAsset` 的父目录，不再触发全项目 `AssetDatabase.Refresh`）；状态存储、Preferences 与 MiniTools 的首次自动创建均改走该方法。 / **Fixed first-time folder creation** with a new level-by-level `AssetDatabase.CreateFolder` helper.
- **文档**：双语 README 增加「Odin 依赖检查」「UPM 安装与 Assets 安装的差异」小节与锁定版本说明。 / **Docs**: new README sections ("Odin Dependency Check", "UPM vs. Assets installation") plus tag-pinning notes.

### Fixed

- **UPM 安装且 Project 窗口隐藏 Packages 时「Ping 脚本文件」完全失效**：原实现拼出的 `Assets/../Library/PackageCache/...` 路径必然解析失败并静默返回 null；现按文件名反查 MonoScript 后直接打开源码。 / **"Ping Script File" failed entirely under a UPM installation when the Project window hid Packages**: the previous path could never resolve; the button now resolves the MonoScript by file name and opens its source.
- **AssetSelector 示例下拉为空、FolderPath 示例指向不存在的目录（UPM 安装）**：硬编码包内路径在 `Packages/` 下不存在；`FolderPathExampleSO` 的 `ParentFolder` 改为 `Assets`。 / **AssetSelector examples were empty and a FolderPath example pointed at a missing folder (UPM install)**.

## [0.19.0] - 2026-09-27

### Changed

- **Group 类特性的案例按参数主题拆分（10 个面板）**：Box Group / Button Group / Horizontal Group / Vertical Group / Tab Group / Title Group / Toggle Group / Responsive Button Group / Show If Group / Hide If Group 的案例由「单案例内用 `FoldoutGroup` 分段」改为「一个参数主题一个案例」（案例名即参数标签）——分段标签与被测组特性挂在同一成员时会被 Odin 画进该组内部（横排组里标题挤进按钮行、竖向组里叠出多余小标题），拆分后每个案例只绘制被测组本身；新增 24 个案例类（Box Group 的 `BoxGroupMemberReference` / `BoxGroupCombining`，Button Group 的 `ButtonGroupNamedGroup` / `ButtonGroupParameter` / `ButtonGroupResolvedString`，Horizontal Group 的 `HorizontalGroupWidth` / `HorizontalGroupMarginRight` / `HorizontalGroupGap` / `HorizontalGroupTitle`，Vertical Group 的 `VerticalGroupPadding` / `VerticalGroupCombining`，Tab Group 的 `TabGroupFixedHeight` / `TabGroupTabStyle` / `TabGroupLayouting` / `TabGroupCombining`，Title Group 的 `TitleGroupStyle` / `TitleGroupOrder` / `TitleGroupCombining`，Toggle Group 的 `ToggleGroupMemberReference` / `ToggleGroupTitle` / `ToggleGroupOrder` / `ToggleGroupCombining`，Responsive Button Group 的 `ResponsiveButtonGroupParameter` / `ResponsiveButtonGroupCombining`，类名均以 `ExampleSO` 结尾，其中 `ButtonGroupNamedGroupExampleSO` 由 `ButtonGroupExampleWithGroupNameSO` 更名而来）；案例类改名会使状态存储中对应的旧选中记录自动失效并回落到默认案例（不会报错）。/ **Examples of group attributes are split by parameter topic (10 panels)**: examples for Box Group / Button Group / Horizontal Group / Vertical Group / Tab Group / Title Group / Toggle Group / Responsive Button Group / Show If Group / Hide If Group no longer use `FoldoutGroup` sections inside a single example; each parameter topic is now its own example whose name is the parameter label. A section label sharing a member with the group attribute under test is drawn inside that group by Odin (in horizontal groups the title squeezes into the button row, in vertical groups it stacks as an extra small heading), so each example now draws only the group under test. 24 example classes were added (named after their panel and ending in `ExampleSO`; `ButtonGroupNamedGroupExampleSO` replaces `ButtonGroupExampleWithGroupNameSO`), and renaming an example makes the matching old selection record in the state store expire gracefully back to the default example.
- **参数表的「返回值类型」列统一 `float` 的显示**：`float` 参数显示为 `System.Single(float)`（CLR 全名 + C# 别名），覆盖 19 个 Data 文件共 30 处；其余基元类型保持 CLR 全名不变。/ **Parameter tables show `float` with its C# alias**: `float` parameters now render as `System.Single(float)` (CLR full name plus the C# alias) across 30 parameter rows in 19 Data files; other primitive types keep their CLR full names.
- **文档**：中英双语 README 的版本徽章更新至 0.19.0，并修正已迁出的 Script Doc Generator 指向——原链接 `Assets/ScriptDocGenerator/README.md` 已随该工具迁入 Aesir Modules 包（`Editor/ScriptDocGenerator/`）而不存在。/ **Docs**: the version badges in both READMEs now read 0.19.0, and the migrated Script Doc Generator pointer was fixed — the old `Assets/ScriptDocGenerator/README.md` link no longer exists because the tool moved into the Aesir Modules package (`Editor/ScriptDocGenerator/`).

### Fixed

- **Type Filter 案例预览全空**：示例字段的类型为抽象类 `BaseClass` 与接口 `IMyInterface`，Unity 原生序列化无法承载，属性树为空、预览区什么都没有；改为 `OdinAttributeExampleSO<T>` + `[OdinSerialize]` 走 Odin 序列化后端。/ **Type Filter example preview was empty**: its fields are typed as the abstract class `BaseClass` and the interface `IMyInterface`, which Unity's native serialization cannot carry, so the property tree was empty. The example now uses `OdinAttributeExampleSO<T>` with `[OdinSerialize]` and the Odin serialization backend.
- **Hide Reference Object Picker 案例演示不了特性**：原案例把特性挂在 `string` 字段上，而字符串没有多态对象选择器，标注与未标注看不出任何差别；改为自定义引用类型字段的对照（两个 `[HideReferenceObjectPicker]` 字段与两个默认字段）。/ **Hide Reference Object Picker example could not demonstrate the attribute**: it was applied to `string` fields, and strings have no polymorphic object picker, so the annotated and plain fields looked identical. The example now contrasts a custom reference type on two `[HideReferenceObjectPicker]` fields against two plain fields.
- **Hide Mono Script 案例在内联预览中看不到差异**：Odin 在内联编辑器（遗留 IMGUI 路径）中绘制时会强制隐藏 Script 字段，任何内联预览都不画它；改为两个字段相同的对照对象（`HideMonoScriptDemoObject` 标注、`ShowMonoScriptDemoObject` 未标注）加两个按钮，用 `GUIHelper.OpenInspectorWindow` 打开真实 Inspector 窗口对比。/ **Hide Mono Script example showed no difference in the inline preview**: Odin forces the Script field hidden when drawing an inline editor (legacy IMGUI path), so no inline preview can show it. The example now uses two objects with identical fields (`HideMonoScriptDemoObject` annotated, `ShowMonoScriptDemoObject` not) plus two buttons that open the real Inspector windows via `GUIHelper.OpenInspectorWindow`.
- **修复 Attribute Overview Ultra 中示例类级特性在面板宿主字段上误报解析错误**：Odin 的 `TypeDefinitionAttributeProcessor` 会把示例类上的类级特性（如 `ImageClassExampleSO` 的 `[Image("typeBanner")]`）传播到面板承载当前示例的字段 `AbstractAttributePanelSO.currentSelectedExample`；该字段位于面板的属性树中，特性只能以面板为上下文解析成员引用，于是画出红色错误框（banner 本身一直由示例自身的属性树正常绘制）。新增 `ExampleHostAttributeProcessor` 仅对示例宿主字段剥离由值类型定义传播来的特性，`SearchableExampleSO` 的 `[Searchable]`、`ShowPropertyResolverExampleSO` 的 `[ShowOdinSerializedPropertiesInInspector]` 等同款传播一并清除。/ **Fixed**: Odin's `TypeDefinitionAttributeProcessor` propagates class-level attributes of an example class (such as `ImageClassExampleSO`'s `[Image("typeBanner")]`) onto the panel field that hosts the current example (`AbstractAttributePanelSO.currentSelectedExample`). That field lives in the panel's property tree, so the attribute can only resolve member references in the panel's context and draws a red error box (the banner itself was always drawn correctly by the example's own property tree). The new `ExampleHostAttributeProcessor` strips value-type-defined attributes from the example host field only, which also clears the same propagation for `[Searchable]` and `[ShowOdinSerializedPropertiesInInspector]`.

## [0.18.0] - 2026-09-25

### Changed

- **重命名状态银行（破坏性）**：`UltraStateBankSO` → `UltraStateStoreSO`，资产 `UltraStateBank.asset` → `UltraStateStore.asset`（GUID 不变，用户数据无需迁移），`UltraStateBankEntry` / `UltraBankEntryKind` → `UltraStateStoreEntry` / `UltraStateStoreEntryKind`，`LoadOrCreateBank()` → `LoadOrCreate()`，`AesirInspectorPaths.AttributeOverviewUltraStateBankPath` → `AttributeOverviewUltraStateStorePath`；内存面板注册中心 `UltraPanelDatabase` 名称保持不变。条目 key 前缀 `PanelSelection/` 与 `ExampleState/` 保持不变。/ **Breaking rename**: the state bank is now `UltraStateStoreSO` (asset renamed with GUID preserved, no data migration needed); the in-memory panel registry keeps its name `UltraPanelDatabase`. Entry key prefixes are unchanged.

### Fixed

- **避免在绘制回调中解析"缺失即自动创建"的资产**：`MenuItemViewerSO.Instance` / `OdinSyntaxHighlighterPanelSO.Instance` 改为按域缓存；Getting Started 窗口的 `AesirInspectorProjectSettingsSO.Instance` 由 `[OnInspectorGUI]` 绘制路径移到 `OnEnable` 解析一次。此前资产缺失时该解析会执行 `CreateAsset` + `AssetDatabase.Refresh()`（全项目重扫），在 GUI 回调中执行会产出大量 `the GUIStateObj is deleted, but is accessed` 并可能卡住编辑器。/ **Fixed**: assets that auto-create on first access are no longer resolved from GUI draw callbacks — `MenuItemViewerSO.Instance` / `OdinSyntaxHighlighterPanelSO.Instance` are cached per domain and the Getting Started window resolves `AesirInspectorProjectSettingsSO.Instance` in `OnEnable`. A missing asset previously triggered `CreateAsset` plus a full `AssetDatabase.Refresh()` inside IMGUI callbacks, producing `the GUIStateObj is deleted, but is accessed` errors and potentially hanging the editor.

## [0.17.0] - 2026-09-25

### Added

- **`WrappedTextAttribute` 自动换行只读文本特性**：Odin 自带的 `DisplayAsString` 在宽度不足时直接截断（不换行、也不加省略号），长内容会静默丢失尾部；新特性按可用宽度换行绘制，支持 `LabelWidth` / `FontSize` 参数，已用于 MenuItem Viewer 的菜单路径列。/ **`WrappedTextAttribute`**: Odin's `DisplayAsString` silently truncates when the column is too narrow (no wrapping, no ellipsis), losing the tail of long text; the new attribute wraps to the available width and exposes `LabelWidth` / `FontSize`. It backs the menu-path column of MenuItem Viewer.

### Changed

- **MenuItem Viewer 面板打磨**：菜单路径独占一行并按宽度自动换行，优先级与校验状态改为并排两列；"收集菜单项"按钮更名并配搜索图标；程序集过滤器标题简化；菜单项信息新增用于搜索的 `methodName` 序列化字段（面板中隐藏），不再暴露可写的 `MethodName` 属性。/ **MenuItem Viewer polish**: the menu path gets its own wrapping row, priority and validate state share a two-column row, the collect button is renamed and gains a search icon, the assembly-filter title is shortened, and `MenuItemInfo` now keeps a serialized, hidden `methodName` for search instead of an exposed writable `MethodName` property.
- **命名与文案修正**：`OdinSyntaxHighlighterPanelSO` 中过时的 `OdinSyntaxHighlighterSO` / `AesirCodeHighlighter` 引用修正为 `OdinSyntaxHighlighterPanelSO` / `OdinCodeHighlighter`，`OdinCodeHighlighter` 的日志前缀同步为 `[OdinCodeHighlighter]`；语法高亮面板与 Getting Started 窗口的中英文案改为更简洁的表述。/ **Naming and copy fixes**: stale `OdinSyntaxHighlighterSO` / `AesirCodeHighlighter` references in `OdinSyntaxHighlighterPanelSO` now read `OdinSyntaxHighlighterPanelSO` / `OdinCodeHighlighter`, the `OdinCodeHighlighter` log prefix follows suit, and the syntax-highlighter panel plus Getting Started window use tighter wording.
- **仓库与安装地址修正**：安装地址、`package.json` 的 UPM 元数据（`documentationUrl` / `changelogUrl` / `licensesUrl`）与 `AesirInspectorWebLinks` 全部由 `Unity-Aesir-Packages` monorepo 改为本包所在仓库 `yuumixcode/AesirInspector`——原地址指向的 monorepo（现名 `AesirFramework`）中并不存在本包路径，安装会 404；英文 README 同步删除已迁出本包的 `[Summary]` 章节并重新编号。/ **Repository and install URL fix**: the install URL, the `package.json` UPM metadata (`documentationUrl` / `changelogUrl` / `licensesUrl`) and `AesirInspectorWebLinks` now point at `yuumixcode/AesirInspector` instead of the `Unity-Aesir-Packages` monorepo (now `AesirFramework`), which does not contain this package path — the old URL 404s. The English README also drops the migrated `[Summary]` section and renumbers.

## [0.16.0] - 2026-09-25

### Added

- **包内案例资产目录 `Editor/ExampleAssets/`**：为必须指向真实资产的案例（如 `AssetSelector` 的 `Paths`）提供随包分发的占位资产——`AesirExampleAssetSO` 类型、两个 ScriptableObject 资产与两个材质文件夹；该目录位于 `Editor/` 内，随包分发但不会进入构建。/ **In-package example assets**: `Editor/ExampleAssets/` ships placeholder assets (`AesirExampleAssetSO`, two ScriptableObject assets and two material folders) so examples that must point at real assets (`AssetSelector.Paths`) have a stable target; living inside an `Editor` folder they ship with the package but never enter builds.

### Changed

- **UPM 元数据与链接**：`package.json` 补齐 `documentationUrl` / `changelogUrl` / `licensesUrl`，`description` 改为中英双语；`AesirInspectorWebLinks` 的 LICENSE / CHANGELOG 链接修正为包内文件路径（原指向仓库根目录下并不存在的文件）。/ **UPM metadata and links**: `package.json` gains `documentationUrl` / `changelogUrl` / `licensesUrl` and a bilingual `description`; `AesirInspectorWebLinks` now points LICENSE / CHANGELOG at the in-package files instead of non-existent repo-root files.
- **文档**：README 的 git URL 安装地址统一为 monorepo 子目录形式（`Unity-Aesir-Packages.git?path=/Assets/Runestone/AesirInspector`），新增 Samples 导入说明并更新版本徽章；英文 README 移除已迁出本包的 Script Doc Generator / Summary Tool 章节并重新编号。双语 README 与 `development.md` 的 Odin 依赖描述修正为 defineConstraints 语义（全部 asmdef 带 `ODIN_INSPECTOR` 约束，未安装 Odin 时程序集整体跳过编译而非编译失败）；`development.md` 同步移除已迁出的 ScriptDocGenerator 模块行与已删除的 `Editor/Common`、`AesirInspectorModuleAssetMarkerSO`，补 `ExampleAssets` 与 `AesirInspectorProjectSettingsSO`，并修正 `Runtime/Unity/Utilities`、`Editor/Unity` 等失效路径及已迁出的 `[Summary]` 约定。/ **Docs**: install URLs now consistently use the monorepo git URL with the `?path=` subfolder, sample-import instructions and the version badge were updated, and the English README dropped the Script Doc Generator / Summary Tool sections that no longer belong to this package. The bilingual READMEs and `development.md` now describe Odin as a defineConstraints dependency (every asmdef carries `ODIN_INSPECTOR`, so assemblies are skipped rather than failing to compile when Odin is absent); `development.md` also drops the migrated ScriptDocGenerator module and the removed `Editor/Common` / `AesirInspectorModuleAssetMarkerSO`, adds `ExampleAssets` and `AesirInspectorProjectSettingsSO`, and fixes stale `Runtime/Unity/Utilities` / `Editor/Unity` paths plus the migrated `[Summary]` convention.
- **目录整合**：`Editor/AttributeOverviewPro/` 整体并入 `Editor/AttributeOverviewUltra/`——`Abstract`、`AttributePanels`、`Data`、`UsageExamples` 平移至 Ultra 下，`Core` 两批文件合并，Pro 目录不再存在；命名空间、类型名与资产 GUID 均未变化。/ **Directory consolidation**: `Editor/AttributeOverviewPro/` merged into `Editor/AttributeOverviewUltra/` — `Abstract`, `AttributePanels`, `Data` and `UsageExamples` moved under Ultra and the two `Core` sets merged; namespaces, type names and asset GUIDs are unchanged.

### Removed

- **移除扩展包管理器**：删除 `ExtensionPackageManagerWindow`、`ExtensionPackageCard`、`PackageManagerEditorUtility` 三个类型及 `Tools → Aesir → Inspector → Extension Package Manager` 菜单项，README 与 Getting Started 窗口中的对应条目同步移除。/ **Removed the Extension Package Manager**: the `ExtensionPackageManagerWindow`, `ExtensionPackageCard` and `PackageManagerEditorUtility` types and the `Tools → Aesir → Inspector → Extension Package Manager` menu entry are gone, together with the matching README and Getting Started entries.
- **删除示例迁移指南 `AESIR_ATTRIBUTE_MIGRATION_GUIDE.md`**：其分组与流程规范已内化到示例本身，指南不再维护。/ **Removed the deprecated example migration guide** `AESIR_ATTRIBUTE_MIGRATION_GUIDE.md`; its grouping and workflow rules are now embodied by the examples themselves.

## [0.15.0] - 2026-09-09

### ⚠ BREAKING CHANGES（破坏性变更 · 升级前必读 / Read before upgrading）

> **Script Doc Generator 与 Summary 工具已移出本包**，迁移至仓库内独立工具 `Assets/ScriptDocGenerator/`（非 UPM 包）。本包定位收窄为 Odin Inspector 增强库（双语特性、Attribute Overview Ultra、安全编辑器工具、扩展包管理器）。
> The Script Doc Generator and Summary Tool were moved out of this package into the standalone tool at `Assets/ScriptDocGenerator/`.

> **Attribute Overview Pro 已移除**，由全面重做的 Attribute Overview Ultra 取代。面板与示例不再作为子资产持久化（`AttributeOverviewDatabase.asset`、`OdinExamples.asset`、`UnityExamples.asset` 随升级删除），用户调试状态改由状态银行 `UltraStateBank.asset` 以快照形式持久化，Project 中零子资产。示例 SO 单例（`AttributeExampleSO<T>.Instance` / `OdinAttributeExampleSO<T>.Instance`）后端切换为银行内存路由，外部仍按 `.Instance` 惯用法访问。
> **Attribute Overview Pro has been removed** and replaced by the fully rebuilt Attribute Overview Ultra. Panels and examples are no longer persisted as sub-assets (`AttributeOverviewDatabase.asset`, `OdinExamples.asset` and `UnityExamples.asset` are deleted on upgrade); user debug state now persists as snapshots in the `UltraStateBank.asset` state bank, keeping the Project free of sub-assets. Example SO singletons are now routed through the in-memory state bank while keeping the `.Instance` accessor.

#### 迁移指南 / Migration Guide

| 范围 / Scope | 旧 / Before | 新 / After |
|---|---|---|
| 代码位置 | `Assets/Runestone/AesirInspector/{Runtime,Editor}/ScriptDocGenerator/` | `Assets/ScriptDocGenerator/{Runtime,Editor}/` |
| 命名空间（Runtime） | `Runestone.AesirInspector`（ScriptDocGenerator 部分） | `Runestone.ScriptDocGenerator` |
| 命名空间（Editor） | `Runestone.AesirInspector.Editor`（ScriptDocGenerator 部分） | `Runestone.ScriptDocGenerator.Editor` |
| 程序集 | 并入 `Runestone.AesirInspector(.Editor)` | 独立 `Runestone.ScriptDocGenerator(.Editor)` |
| 菜单 | `Tools → Aesir → Inspector → Script Doc Generator`、`Assets → Aesir Inspector → …` | `Tools → Script Doc Generator`、`Assets → Script Doc Generator → …` |
| 编辑器资源路径 | `Assets/Editor Default Resources/Aesir Inspector/…` | `Assets/Editor Default Resources/Script Doc Generator/…` |
| `[Summary]` / `[ReferenceLinkURL]` 特性 | `Runestone.AesirInspector` | `Runestone.ScriptDocGenerator` |
| 特性总览窗口 | `Tools → Aesir → Inspector → Attribute Overview Pro`（资产数据库 + 子资产示例） | `Tools → Aesir → Inspector → Attribute Overview Ultra`（内存面板 + UltraStateBank 状态银行） |
| 编辑器资源路径（特性总览） | `Assets/Editor Default Resources/Aesir Inspector/Attribute Overview Pro/` | `Assets/Editor Default Resources/Aesir Inspector/Attribute Overview/`（仅 `UltraStateBank.asset`） |

### Added

- **Attribute Overview Ultra 特性总览窗口**：以 TypeCache 扫描 + CreateInstance 内存实例化替代资产数据库；树形菜单、搜索、分类浏览与代码预览与 Pro 对齐，并新增窄窗防线（可拖拽菜单宽度 + 内容整体横向滚动）、示例调试状态快照持久化（SHA256 校验和 + 类型名双校验）。/ **Attribute Overview Ultra window**: replaces the asset database with TypeCache scanning and CreateInstance in-memory panels; menu tree, search, category browsing and code preview are on par with Pro, plus narrow-window defenses (draggable menu width + unified horizontal scrolling) and example debug-state snapshots persisted with SHA256 checksum and type-name double validation.
- **目录结构与 Odin 官方完全一致**：分类归属、多分类（同一特性可同时出现在多个分类，如 Button 同时在 Groups 与 Buttons）、显示名（官方 nice name 规则，如 `Assets Only`、`GUIColor`）与分类排序（官方 CategoryComparer：Essentials 首位，Misc/Meta/Unity/Debug 靠后）全部直读 Odin 官方注册表（`AttributeExampleUtilities`），随 Odin 升级零维护。/ **Directory structure identical to Odin's official window**: categories, multi-category registration (an attribute can appear in several categories, e.g. Button in both Groups and Buttons), display names (official nice-name rule) and ordering (official CategoryComparer) are all read directly from Odin's official registry, requiring zero maintenance across Odin upgrades.
- **Aesir Customs 分类**：新增双语特性面板（Bilingual Title / Bilingual Button / Bilingual Info Box / Bilingual Text），含完整参数表与示例，置于菜单首位。/ **Aesir Customs category**: added panels for the bilingual attributes (Bilingual Title / Bilingual Button / Bilingual Info Box / Bilingual Text) with full parameter tables and examples, pinned to the top of the menu.
- **补齐 Meta / Unity / Debug 三个分类**：新增 7 个面板——SuppressInvalidAttributeError（Meta）、ShowDrawerChain 与 ShowPropertyResolver（Debug）、Multiline / Range / Space / TextArea（Unity 内置特性在 Odin 下的绘制，其中 Range 同时在 Unity 与 Validation）；至此分类集合与官方 12 个分类完全一致。/ **Completed the Meta / Unity / Debug categories**: added 7 panels — SuppressInvalidAttributeError (Meta), ShowDrawerChain and ShowPropertyResolver (Debug), and Multiline / Range / Space / TextArea (Unity built-in attributes as drawn by Odin; Range also appears under Validation). The category set now matches Odin's official 12 categories exactly.

### Changed

- **补齐全部剩余特性面板，目录与官方完全对等**：新增 33 个面板（Button Group、Enum Paging、Responsive Button Group、Table Column Width、Table Matrix、Asset Selector、Child Game Objects Only、Color Palette、Image、Multi Line Property、Polymorphic Drawer Settings、Preview Field、Toggle、Toggle Left、Type Drawer Settings、Type Info Box、Type Registry Item、Type Selector Settings、Unit、Wrap、Required List Length、Disallow Modifications In、Custom / Disable Context Menu、Disable / Show In Inline Editors、Hide Duplicate Reference Box / In Tables / Mono Script / Network Behaviour Fields / Reference Object Picker、Draw With Unity、On Collection Changed 等），其中 6 个特性的示例按 Odin 官方示例移植；Ultra 目录现已覆盖官方全部条目（逐条比对：缺失 0、多余 0）。HideNetworkBehaviourFields 因项目未启用 UNET 模块而不提供示例。/ **Filled every remaining attribute panel for full parity with Odin's window**: added 33 panels (with 6 examples ported from Odin's official examples); the Ultra menu now covers every official entry (verified: 0 missing, 0 extra). HideNetworkBehaviourFields ships without an example because the UNET module is not enabled.
- 修复无示例面板的空引用问题：示例列表为空时不再调用示例代码预览刷新（此前会输出 "attribute 不能为空" 错误日志）。/ Fixed a null-reference issue for panels without examples: the code-preview refresh is skipped when the example list is empty (it previously logged an "attribute 不能为空" error).
- 收敛 OnInspectorInit 与 OnInspectorDispose 的重复案例：两个特性各自的"Basic Usage"与"Action"案例演示参数完全相同，已合并为单一案例（保留全部独有写法），其余特性的 With 变体保留以区分简单/进阶参数。/ Merged the duplicated OnInspectorInit and OnInspectorDispose examples: their "Basic Usage" and "Action" cases demonstrated exactly the same parameters, so each was merged into a single case (all unique usages kept). Other attributes' "With" variants are kept to separate basic from advanced parameters.

- 工具面板 UI 移除双语特性，改为纯 Odin 特性（仅中文文本）。/ The tool panel UI no longer uses the bilingual attributes; plain Odin attributes with Chinese text are used instead.
- 移除 `AesirInspectorModuleAssetMarkerSO`（其唯一使用者为 Script Doc Generator，由工具自带的 `ScriptDocGeneratorAssetMarkerSO` 替代）。/ Removed `AesirInspectorModuleAssetMarkerSO` (its only consumer was Script Doc Generator, replaced by the tool's own `ScriptDocGeneratorAssetMarkerSO`).
- Getting Started 窗口的初始化按钮不再生成 100+ 案例资产，改为确保状态银行可用并完成一次全量面板构建冒烟。/ The Getting Started initialize button no longer generates 100+ example assets; it now ensures the state bank exists and smoke-builds all panels once.
- 清理 CustomValueDrawer 双胞胎面板（Misc 分类重复项，Essentials 归类与 Odin 官方一致），消除同名菜单项。/ Removed the duplicate CustomValueDrawer panel from Misc (the Essentials registration matches Odin's official category), eliminating the duplicated menu entry.

### Removed

- Attribute Overview Pro 窗口（`AttributeOverviewWindow`）与资产数据库（`AttributeOverviewDatabaseSO`）。/ The Attribute Overview Pro window (`AttributeOverviewWindow`) and its asset database (`AttributeOverviewDatabaseSO`).

## [0.14.1] - 2026-09-05

### Fixed

- **无 Odin Inspector 的项目导入包后编译报错 / Compile errors when importing into projects without Odin Inspector**：恢复全部 7 个 asmdef（Runtime、Editor、Tests、Samples）的 `ODIN_INSPECTOR` defineConstraints——未安装 Odin 时程序集整体跳过编译、零报错，已安装时行为不变。/ Restored the `ODIN_INSPECTOR` defineConstraints on all 7 asmdefs (Runtime, Editor, Tests, Samples) so the assemblies are skipped entirely with zero errors when Odin is absent; behavior is unchanged when Odin is installed.

### Changed

- 弃用 Unity 内置导出方案（移除 `Tools → Aesir → Inspector → Export Package` 菜单），维护者导出统一为 .NET 工具链：本地 `Scripts/export-package.sh`，CI 走 GitHub Actions。/ The Unity-based export was retired (the `Tools → Aesir → Inspector → Export Package` menu was removed); maintainer exports are unified on the .NET toolchain — local `Scripts/export-package.sh`, CI via GitHub Actions.
- `AesirInspectorVersion.Version` 同步为 `0.14.1`（此前停留在 `0.4.0-pre.1`）。/ `AesirInspectorVersion.Version` is now in sync (`0.14.1`); it had been left at `0.4.0-pre.1`.

## [0.14.0] - 2026-09-05

### ⚠ BREAKING CHANGES（破坏性变更 · 升级前必读 / Read before upgrading）

> **架构重构 / Architecture restructure**：Odin Inspector 升级为**硬依赖**，移除 Unity/Odin 双程序集隔离架构。
> 程序集从 4 个合并为 2 个，目录与命名空间全面重构，升级后需按下方迁移指南更新代码引用。
> **Odin Inspector is now a hard dependency.** The 4 assemblies are merged into 2, with directories and namespaces fully restructured.

#### 迁移指南 / Migration Guide

| 范围 / Scope | 旧 / Before | 新 / After |
|---|---|---|
| 程序集（Runtime） | `Runestone.AesirInspector` + `Runestone.AesirInspector.OdinInspector` | `Runestone.AesirInspector` |
| 程序集（Editor） | `Runestone.AesirInspector.Editor` + `Runestone.AesirInspector.Editor.OdinInspector` | `Runestone.AesirInspector.Editor` |
| 命名空间（Runtime） | `Runestone.AesirInspector` / `Runestone.AesirInspector.OdinIntegration` | `Runestone.AesirInspector` |
| 命名空间（Editor） | `Runestone.AesirInspector.Editor` / `Runestone.AesirInspector.OdinIntegration.Editor` | `Runestone.AesirInspector.Editor` |
| 目录（Runtime） | `Runtime/Unity/` + `Runtime/OdinInspector/` | `Runtime/` |
| 目录（Editor） | `Editor/Unity/` + `Editor/OdinInspector/` | `Editor/` |
| 条件编译 | `#if ODIN_INSPECTOR` | 移除，直接使用 Sirenix API |
| defineConstraints | `ODIN_INSPECTOR` | 移除 |

#### 代码侧替换示例 / Code-side replace examples

```csharp
// 旧 / Before
using Runestone.AesirInspector.OdinIntegration;
using Runestone.AesirInspector.OdinIntegration.Editor;

// 新 / After
using Runestone.AesirInspector;
using Runestone.AesirInspector.Editor;
```

```jsonc
// asmdef references 旧 / Before
"references": [
  "Runestone.AesirInspector.OdinInspector",
  "Runestone.AesirInspector.Editor.OdinInspector"
]

// 新 / After
"references": [
  "Runestone.AesirInspector",
  "Runestone.AesirInspector.Editor"
]
```

### Changed

- **标准 Unity Package 布局**：`Runtime/` 与 `Editor/` 单层扁平结构 —— Runtime：Attributes、Inspector、Localization、Utilities、ScriptDocGenerator、Common、Debug、CodeStyle；Editor：AttributeOverviewPro、AttributeProcessors、Common、Core、Drawers、ExtensionManager、MiniTools、ScriptDocGenerator、Windows
- **Odin Inspector 硬依赖**：移除 `ODIN_INSPECTOR` defineConstraints 与全部 `#if ODIN_INSPECTOR` 条件编译（4 处守卫保留 Odin 分支），未安装 Odin 时将直接编译失败
- **程序集合并**：`Runestone.AesirInspector.OdinInspector` 并入 `Runestone.AesirInspector`，`Runestone.AesirInspector.Editor.OdinInspector` 并入 `Runestone.AesirInspector.Editor`
- **Tests 重构**：`Tests/Editor/OdinInspector/` 平铺为 `Tests/Editor/`，两个测试程序集引用同步更新（保持 `UNITY_INCLUDE_TESTS` 约束与 Sirenix 预编译引用）
- **文档全面同步**：README（中英）、development.md、CONTRIBUTING、Third Party Notices 按新架构更新
- **文档精简**：删除与 README 内容重复的 `Documentation~/aesir-inspector.md`，删除包内 `CONTRIBUTING.md` 副本（以仓库根目录文档为准）
- **文档口径统一为 Unity 引擎**，移除团结/Tuanjie 表述

---

## [0.13.0] - 2026-09-03

### Changed

- 版本号与 Aesir Architecture / Aesir Modules 同步更新至 `0.13.0`，本包本版本无功能性变更

---

## [0.12.0] - 2026-08-22

### Changed

- 版本号与 Aesir Architecture / Aesir Modules 同步更新至 `0.12.0`，本包本版本无功能性变更

---

## [0.11.0] - 2026-08-22

### Changed

- 版本号与 Aesir Architecture / Aesir Modules 同步更新至 `0.11.0`，本包本版本无功能性变更

---

## [0.9.0] - 2026-08-15

### Changed

- **Odin 程序集重命名** — `OdinIntegration` → `OdinInspector`（三包统一）：
  - Runtime: `Runestone.AesirInspector.OdinIntegration` → `Runestone.AesirInspector.OdinInspector`
  - Editor: `Runestone.AesirInspector.OdinIntegration.Editor` → `Runestone.AesirInspector.Editor.OdinInspector`
  - 目录 `OdinIntegration/` → `OdinInspector/`（Runtime/Editor/Tests 三处）
  - 6 个引用方 asmdef 同步更新（Tests + Samples~×3 + Assets Samples×3）
- **文档同步** — aesir-inspector.md 程序集表、development.md 依赖图、README 中的 OdinIntegration 引用更新为 OdinInspector

## [0.8.0] - 2026-08-06

### Changed

- 版本号与 Aesir Architecture / Aesir Modules 同步更新至 `0.8.0`，本包本版本无功能性变更

## [0.7.0] - 2026-08-05

### Changed

- 版本号与 Aesir Architecture / Aesir Modules 同步更新至 `0.7.0`，本包本版本无功能性变更

## [0.6.0] - 2026-08-05

### Added

- **源代码文件查找与内容缓存**：新增 `SourceFileEntry` 数据容器，将 `.cs` 文件路径与代码内容绑定，支持缓存避免重复读取
- **块注释内的假 XML 注释过滤**：解析源代码时逐行跟踪 `/* */` 块注释状态，块注释内以 `///` 开头的行不会被误判为 XML 文档注释
- **跨程序集同名类型区分**：summary 缓存键加入程序集名前缀（`AssemblyName.Namespace.TypeName.MemberName`），避免不同程序集中同名命名空间+类型名的键冲突
- **重载方法 summary 区分**：方法成员的 summary 键附加参数类型列表（如 `MethodName(int, string)`），不同重载方法各自独立解析 summary。支持多行声明参数跨行
- **嵌套类型 summary 解析**：支持嵌套类型（如 `OuterClass.NestedStruct`）的 summary 查询，不再错误返回外层类的 summary
- **泛型类型 summary 解析**：支持泛型类型（如 `AbstractContext<T>`）的 summary 查询
- **文件名与类型名不匹配时的源文件查找**：当一个 `.cs` 文件中定义了多个类型且文件名不与任何类型名匹配时（如 `Capabilities.cs` 中定义 7 个接口），通过全局内容扫描找到源文件
- **多程序集批量分析模式**：`ScriptDocGeneratorSO.TypeSource` 枚举新增 `MultipleAssemblies` 模式，支持同时分析多个程序集的所有类型
- **反射解析器迁移至 Runtime/Unity**：将反射解析器（19 个 Runtime 文件）从 `Runtime/OdinIntegration` 迁移到 `Runtime/Unity`，使 `[Summary]` 和 `[ReferenceLinkURL]` 特性不再依赖 `ODIN_INSPECTOR` 程序集约束
- **源码解析单元测试**：新增 34 个测试覆盖块注释、全限定键、命名空间、单行/多行 summary、多文件合并、多行属性声明、泛型方法、表达式体泛型方法、重载方法、嵌套类型、多行方法声明等场景
- **重载前缀单元测试**：新增 4 个测试覆盖 2/3/4 个重载方法和非重载方法的 `[Overload]` 前缀验证

### Changed

- **移除 OdinBridge 桥接层**：不再通过 `IOdinBridge` 接口间接调用 Odin，改为 `#if ODIN_INSPECTOR` 条件编译直接使用 `Sirenix.Utilities` API
- **模块整合**：将 `ReflectionAnalyzer`、`SummaryTool`、`OdinSourceFileHelper` 全部整合到 `ScriptDocGenerator` 模块下，减少跨层碎片化
- **回归单面板设计**：从 4 个独立 Panel SO 回归为单个 `ScriptDocGeneratorSO` + `TypeSource` 枚举切换模式
- **OdinSourceFileHelper 精简**：移除花括号跟踪、类型体定位、字符串净化等复杂逻辑，仅保留源文件查找与成员名提取
- **Summary 解析优先级**：优先检查 `[Summary]` 特性，有则直接返回；无则回退到源代码 XML `/// <summary>` 注释解析
- **Editor 端目录重组**：源码解析工具重组到 `SourceFileTool/` 子目录，Summary 工具重组到 `SummaryAttributeTool/` 子目录

### Removed

- **OdinAutoTooltip 自动 Tooltip 功能**：移除从源代码 XML 注释自动生成 Inspector Tooltip 的功能
- **OdinBridge 桥接模式**：删除 `IOdinBridge`、`DefaultOdinBridge`、`OdinBridgeLocator`、`OdinInspectorBridge` 共 4 个文件
- **多 Panel 设计**：删除 `ScriptDocGeneratorPanelBase` 及 4 个 PanelSO 共 5 个文件

### Fixed

- **块注释内的 XML 注释被误解析**：当 `/* */` 块注释跨行且某行以 `///` 开头时，该行会被误判为 XML 文档注释并提取到错误的 summary。修复后，块注释内的 `///` 行被正确忽略
- **泛型类型的 summary 无法解析**：分析泛型类型（如 `AbstractContext<T>`）时，summary 为空。修复后泛型类型的 summary 可正常解析
- **Type 自身的 summary 无法解析**：分析类型自身时，summary 为空。修复后类型自身的 summary 可正常解析
- **嵌套类型的 summary 返回外层类的注释**：分析嵌套类型时，返回的是外层类的 summary。修复后嵌套类型返回各自的 summary
- **多行属性声明的成员名提取失败**：当属性声明跨多行时，成员名无法提取，导致 summary 丢失。修复后可正确提取成员名
- **泛型方法和表达式体泛型方法的成员名提取错误**：成员名被错误提取为约束类型名而非方法名。修复后可正确提取方法名
- **重载方法的 summary 互相覆盖**：同名重载方法共享同一个缓存键，后解析的 summary 覆盖先前的。修复后每个重载方法通过参数类型列表区分
- **重载方法的 `[Overload]` 前缀重复追加**：当方法有 N 个重载时，`[Overload]` 前缀被追加 N-1 次。修复后每个重载方法只追加一次
- **`ReferenceLinkURL` 特性在文档中显示不全**：`[ReferenceLinkURL("https://...")]` 在生成的文档中仅显示为 `[ReferenceLinkURL]`。修复后完整显示特性及其参数
- **文件名与类型名不匹配时源文件无法找到**：一个 `.cs` 文件中定义了多个类型且文件名不与任何类型名匹配时，所有类型的 summary 均为空。修复后通过全局内容扫描找到源文件
- **`null` 关键字被误提取为成员名**：源代码中的 `return null;` 语句，`null` 被误提取为成员名。修复后不再被提取
- **多行方法声明参数跨行时参数类型提取失败**：当方法声明的 `(` 和 `)` 不在同一行时，参数类型列表无法提取。修复后通过跨行收集声明文本直到括号匹配

## [0.5.0] - 2026-08-01

### Added

- **Odin 自动 Tooltip (OdinAutoTooltip)** ⚡：从源代码 XML `/// <summary>` 注释自动生成 Inspector Tooltip 的 Odin 属性处理器。提取自 [JakePineOdinTools](https://github.com/JakePineGames/JakePineOdinTools)（MIT, © 2026 Jake Pine）。已有 Tooltip 时读取现有值并追加新内容后动态替换原特性
- **ScriptDocGenerator 源码 Summary 解析**：`MemberData` 添加 `SummaryResolver` 委托，Editor 程序集加载时注入源码解析实现，从 `.cs` 文件的 XML `/// <summary>` 注释中读取成员摘要
- **ScriptDocGenerator OdinMenuEditorWindow 重构**：窗口从 `OdinEditorWindow` 重写为 `OdinMenuEditorWindow`，左侧菜单 4 种工作模式（单脚本、多脚本、单程序集、多程序集），每种模式独立面板 SO
- **共享源码解析工具**：`OdinSourceFileHelper`（源文件定位与成员声明提取）和 `SourceSummaryParser`（XML summary 解析），消除 `SourceSummaryInitializer` 与 `OdinAutoTooltipAttributeProcessor` 之间的重复代码

### Changed

- 目录重命名：`Odin Integration` → `OdinIntegration`
- **README 顶部 monorepo 块重写**：从双语段对照改为单语版本，明确说明 Aesir Inspector **不依赖**其他 Aesir 子包（独立可装）
- **Third Party Notices 更新**：替换占位内容，添加 JakePineOdinTools 第三方组件记录
- **Summary 工具标注为推荐替代**：README 中标注推荐新代码使用 OdinAutoTooltip

### Removed

- **移除 `[Summary]` 特性装饰**：252 个文件中 897 处 `[Summary("...")]` 装饰已全部移除。`SummaryAttribute` 类保留作为 ScriptDocGenerator 的回退兼容
- **移除 MIT LICENSE 头部**：所有 `.cs` 文件的 LICENSE 头部已移除，仅在 `CodeStyle/AesirInspectorCodeStyle.cs` 保留一份

### Fixed

- 修复 `ScriptDocGeneratorController.GenerateMultipleTypeDocs` 中 `generatorSettings` 被当作 bool 的 bug

## [0.4.2] - 2026-07-24

### Changed

- 版本号与 Aesir Architecture / Aesir Modules 同步更新至 `0.4.2`，本包本版本无功能性变更

## [0.4.1] - 2026-07-24

### Changed

- **Samples 版本文件夹**：`Assets/Samples/Aesir Inspector/0.4.0-pre.1/` → `0.4.0/`，与 `package.json` 版本对齐

## [0.4.0] - 2026-07-24

### ⚠ BREAKING CHANGES（破坏性变更 · 升级前必读 / Read before upgrading）

> **品牌命名空间统一 / Brand namespace unification**：将所有 `RunLab` 引用统一为 `Runestone`（符文石），与 Aesir Architecture / Aesir Modules 保持一致。
> 所有 `RunLab.*` 命名空间、`cn.runlab.aesir-inspector` 包名、9 个 asmdef 全部改名为 `Runestone.*` / `cn.runestone.aesir.inspector`。
> 升级后**所有使用本包的代码需要批量替换 `using RunLab.*` → `using Runestone.*`**。

#### 迁移指南 / Migration Guide

| 范围 / Scope | 旧 / Before | 新 / After |
|---|---|---|
| 包名 / Package ID | `cn.runlab.aesir-inspector` | `cn.runestone.aesir.inspector` |
| 命名空间 / Namespace | `RunLab.AesirInspector` | `Runestone.AesirInspector` |
| 命名空间 / Namespace | `RunLab.AesirInspector.Editor` | `Runestone.AesirInspector.Editor` |
| 命名空间 / Namespace | `RunLab.AesirInspector.Tests` | `Runestone.AesirInspector.Tests` |
| 命名空间 / Namespace | `RunLab.AesirInspector.Editor.Tests` | `Runestone.AesirInspector.Editor.Tests` |
| 命名空间 / Namespace | `RunLab.AesirInspector.OdinIntegration` | `Runestone.AesirInspector.OdinIntegration` |
| 命名空间 / Namespace | `RunLab.AesirInspector.OdinIntegration.Editor` | `Runestone.AesirInspector.OdinIntegration.Editor` |
| 命名空间 / Namespace | `RunLab.AesirInspector.Samples.*` | `Runestone.AesirInspector.Samples.*` |
| asmdef 名称 / Assembly name | `RunLab.AesirInspector`（及所有变体） | `Runestone.AesirInspector`（及所有变体） |
| 版权字符串 / Copyright | `Copyright (c) 2026 RunLab - Yuumix` | `Copyright (c) 2026 Runestone - Yuumix` |

#### 代码侧替换示例 / Code-side replace examples

```csharp
// 旧 / Before
using RunLab.AesirInspector;
using RunLab.AesirInspector.Editor;
using RunLab.AesirInspector.OdinIntegration;

// 新 / After
using Runestone.AesirInspector;
using Runestone.AesirInspector.Editor;
using Runestone.AesirInspector.OdinIntegration;
```

```jsonc
// asmdef references 旧 / Before
"references": [
  "RunLab.AesirInspector",
  "RunLab.AesirInspector.Editor"
]

// 新 / After
"references": [
  "Runestone.AesirInspector",
  "Runestone.AesirInspector.Editor"
]
```

#### 范围 / Scope
- 422 个 .cs 文件 / 12 个 asmdef + 12 个 asmdef.meta / 1 个 package.json / 1 个 LICENSE.md / 多份 README/CHANGELOG/CONTRIBUTING

### Changed
- 将 `OdinWrapper` 重命名为 `Odin Integration`（目录）/ `OdinIntegration`（命名空间与程序集），以更准确表达集成层的语义
- 将 `Runtime/Unity/Bilingualism/` 重命名为 `Runtime/Unity/Localization/`，对齐 Unity 官方 Localization 包命名
- 将 `Runtime/Unity/InspectorControls/` 重命名为 `Runtime/Unity/Inspector/`，采用 Unity 单数名词惯例
- 将 `Runtime/Unity/Logger/` 重命名为 `Runtime/Unity/Logging/`，对齐 Unity 源码 `Runtime/Export/Logging/` 命名

---

## [0.4.0-pre.1] - 2026-04-29

### Architecture

#### Added
- 新增 `OdinWrapper` 独立程序集，包含 Runtime（`Runestone.AesirInspector.OdinWrapper`）与 Editor（`Runestone.AesirInspector.OdinWrapper.Editor`）两个 asmdef，均设 `defineConstraints: ODIN_INSPECTOR`，将 Odin Inspector 依赖从核心程序集完全隔离 `473640f`

#### Changed
- Runtime 核心程序集 `Runestone.AesirInspector` 移除 `ODIN_INSPECTOR` 编译约束，不再强依赖 Odin Inspector `473640f`
- 编辑器程序集 `Runestone.AesirInspector.Editor` 调整程序集引用，不再直接依赖 Odin `473640f`

### OdinBridge

#### Added
- 新增 `IOdinBridge` 接口，定义 `IsOdinPresent` 等 Odin 可用性查询能力 `473640f`
- 新增 `DefaultOdinBridge`，无 Odin 时自动回退的默认桥接实现 `473640f`
- 新增 `OdinBridgeLocator`，自动查找 Odin 桥接或回退至默认实现 `473640f`
- 新增 `OdinInspectorBridge`（OdinWrapper/Editor/Bridge/），Odin 可用时提供编辑器侧桥接实现 `473640f`

### OdinWrapper

#### Added
- 新增 `OdinWrapper/Editor/AttributeProcessors/` 目录，包含 5 个 OdinAttributeProcessor：`AesirInspectorLanguageSettingsProcessor`、`AesirInspectorResetProcessor`、`BilingualDisplayAsStringProcessor`、`BilingualHeaderProcessor`、`HorizontalSeparateProcessor` `473640f`

#### Changed
- `Editor/AttributeOverviewPro/` 移动至 `OdinWrapper/Editor/AttributeOverviewPro/` `473640f`
- `Editor/Drawers/Bilingual/` 移动至 `OdinWrapper/Editor/Drawers/` `473640f`
- `Editor/ExtensionManager/` 移动至 `OdinWrapper/Editor/ExtensionManager/` `473640f`
- `Editor/MiniTools/` 移动至 `OdinWrapper/Editor/MiniTools/` `473640f`
- `Editor/ScriptDocGenerator/` 移动至 `OdinWrapper/Editor/ScriptDocGenerator/` `473640f`
- `Editor/Core/Windows/` 移动至 `OdinWrapper/Editor/Windows/` `473640f`
- `Runtime/Bilingual/Attributes/` 下 6 个 Bilingual 特性移动至 `OdinWrapper/Runtime/Attributes/` `473640f`
- `Editor/Core/AesirCodeHighlighter.cs` 移动至 `OdinWrapper/Runtime/OdinCodeHighlighter.cs` `473640f`
- `OdinSyntaxHighlighterSO` 重命名为 `OdinSyntaxHighlighterPanelSO` `473640f`

### Bilingualism

#### Changed
- `Runtime/Bilingual/` 重命名为 `Runtime/Bilingualism/` `473640f`
- `AesirInspectorLanguageSettingsSO` 精简，移除 Odin 依赖逻辑，由 `AesirInspectorLanguageSettingsProcessor` 接管 `473640f`

#### Removed
- 移除 `DisplayAsStringBilingualConfigAttribute`，由 `BilingualDisplayAsStringControl` + Processor 替代 `473640f`
- 移除 `ShowIfChineseAttribute`、`ShowIfEnglishAttribute`，由 Processor 替代 `473640f`
- 移除 `DisplayAsStringBilingualWidget`、`HeaderBilingualWidget`，由对应 Control 替代 `473640f`

### InspectorControls

#### Added
- 新增 `BilingualDisplayAsStringControl`，替代原 `DisplayAsStringBilingualWidget` `473640f`
- 新增 `BilingualHeaderControl`，替代原 `HeaderBilingualWidget` `473640f`

#### Changed
- `Runtime/InspectorWidgets/` 重命名为 `Runtime/InspectorControls/`，Widget 统一改名为 Control `473640f`
- `HorizontalSeparateWidget` 重命名为 `HorizontalSeparateControl` `473640f`

### Core

#### Changed
- `IAesirInspectorReset` 精简接口定义，重置逻辑移至 `AesirInspectorResetProcessor` `473640f`
- `AesirInspectorLogger` 从 `Runtime/Core/` 移动至 `Runtime/Logger/` `473640f`
- `AesirInspectorLoggerSettings` 从 `Runtime/Core/` 移动至 `Runtime/Logger/` `473640f`
- `SummaryAttribute` 从 `Runtime/Attributes/Docs/` 移动至 `Runtime/Attributes/`，扁平化目录 `473640f`

#### Removed
- 移除 `ShowEnablePropertyAttribute` 废弃特性 `473640f`

### Utilities

#### Changed
- `ReflectionUtility` 大幅增强，新增反射工具方法 `473640f`

#### Removed
- 移除 `OdinInspectorSafeEditorUtility`，由 OdinBridge 模式替代 `473640f`

### ScriptDocGenerator

#### Changed
- 所有 AnalysisData 类（ConstructorData、EventData、FieldData、MemberData、MethodData、ParameterData、ParameterDirection、PropertyData、TypeData）移除 Odin 特性依赖 `473640f`

### Samples

#### Changed
- `Samples~/` 移动至 `Samples/`（Plugin Config Solutions、RuntimeInitializeLoadType），示例目录对用户可见 `473640f`

#### Removed
- 移除 Codely Skills Library 示例（custom-package-creator） `473640f`

### Tests

#### Changed
- `Runestone.AesirInspector.Tests` asmdef 移除 `ODIN_INSPECTOR` 编译约束 `473640f`
- `Runestone.AesirInspector.Editor.Tests` asmdef 调整程序集引用 `473640f`
- 多个测试文件调整代码格式与区域重排，移除未使用的 using 引用 `473640f`

### Code Style

#### Changed
- 更新 `AESIR_INSPECTOR_CODE_STYLE.cs` 代码风格指南，适配新的程序集架构与命名规范 `473640f`

---

## [0.3.1] - 2026-04-27

### Core

#### Added
- 新增 `AesirInspectorLoggerSettings` ScriptableObject，支持通过 `enableInfoLog`（默认 false）和 `enableWarningLog`（默认 true）控制日志输出 `45a4837`

#### Changed
- `AesirInspectorLogger` 从 Utilities 迁移至 Core 目录，Info/Warning 方法集成 `AesirInspectorLoggerSettings` 开关检查，移除 `MethodImpl` 特性 `45a4837`
- `AesirInspectorWebLinks` 重命名 `GitWebsite` 为 `GitUrl`，`OdinInspectorDocsUrl` 链接由 documentation 改为 tutorials `45a4837`
- `IAesirInspectorReset` 右键菜单标签由 "Aesir Toolkit Reset" 改为 "Aesir Inspector Reset" `45a4837`
- `AesirInspectorMenuItems` 菜单路径重构：`ToolsMenuRoot` 拆分为 `ToolsAesirRoot`（Tools/Aesir）与 `ToolsAesirInspectorRoot`（Tools/Aesir/Inspector），新增各菜单项优先级常量 `cf6126c`
- `AesirCodeHighlighter` 移除 `#if UNITY_EDITOR && ODIN_INSPECTOR_3_3` 宏包裹，using 语句移至命名空间外部 `cf6126c`

#### Removed
- 移除全项目 `#if ODIN_INSPECTOR_3_3` 预处理指令，Odin Inspector 作为硬依赖 `cf6126c`

### Bilingual

#### Changed
- `AesirInspectorLanguageSettings` 重命名为 `AesirInspectorLanguageSettingsSO`，符合 ScriptableObject 命名规范 `cf6126c`
- `DisplayAsStringBilingualWidgetConfigAttribute` 重命名为 `DisplayAsStringBilingualConfigAttribute`，移除 Widget 中间词 `cf6126c`
- `BilingualData` 从 `Runtime/Bilingual/Attributes/` 移动至 `Runtime/Bilingual/` `cf6126c`
- `HeaderBilingualWidget` 的 `_chineseIntroduction`、`_englishIntroduction` 字段标记为 readonly，条件编译由 `#if ODIN_INSPECTOR_3_3` 改为 `#if UNITY_EDITOR` `45a4837` `cf6126c`
- `BilingualBoxGroupAttribute`、`BilingualButtonAttribute` 移除 `#region Internal` `cf6126c`
- `BilingualTitleGroupAttribute` 的 `TitleAlignment` 属性移出 `#if ODIN_INSPECTOR_3_3` 宏包裹 `cf6126c`

#### Removed
- 移除所有 Bilingual 属性与 Drawer 中的 `#if ODIN_INSPECTOR_3_3` 宏 `cf6126c`

### AttributeOverviewPro

#### Changed
- `Editor/AttributeOverview/` 整个目录重命名为 `Editor/AttributeOverviewPro/` `cf6126c`
- 内部 `Data/` 目录下 `AttributeExamplePreviewItem`、`ParameterValue`、`ResolvedStringParameterValue` 移动至 `Core/` 子目录 `cf6126c`
- `AssetListExampleForCustomFilterMethodSO` 重命名为 `AssetListExampleWithCustomFilterMethodSO` `cf6126c`

### Utilities

#### Changed
- `OdinInspectorSafeEditorUtility` 中 `new T[0]` 替换为 `Array.Empty<T>()`，`new Type[1]` 替换为 `new[]` `45a4837`
- `PathSafeEditorUtility.EnsureDirectoryExists` 添加 `[Conditional("UNITY_EDITOR")]` `45a4837`

#### Removed
- 移除所有 Utilities 类中的 `#region Public Methods` 和 `#region` 模式 `45a4837`

### MiniTools

#### Changed
- `AssemblyFilterExample` 重命名为 `FilterOutAesirInspectorAssembly` `cf6126c`

#### Removed
- 移除 MiniTools 模块中的 `#if ODIN_INSPECTOR_3_3` 宏 `cf6126c`

### ScriptDocGenerator

#### Changed
- 所有 AnalysisData 类中 Odin 特性移至 XML 注释之前 `cf6126c`

#### Removed
- 移除 ScriptDocGenerator 模块中的 `#if ODIN_INSPECTOR_3_3` 宏 `cf6126c`

### Code Style

#### Changed
- `HorizontalSeparateWidget` 的 `_darkLineHeight`、`_lightLineHeight`、`_spaceAfter`、`_spaceBefore` 字段标记为 readonly，`DarkLineColor`、`LightLineColor` 属性标记为 static `cf6126c`

#### Removed
- 移除 `#region Internal` 模式，更新代码风格指南与示例代码 `45a4837` `cf6126c`

### Samples

#### Changed
- PluginConfig 示例目录重命名 `58fdbce`

### Docs

#### Added
- 新增 `ATTRIBUTE_OVERVIEW_PRO_GUIDE.md` AttributeOverviewPro 模块编码指南，涵盖 Data-Panel-Example 三件套、单例 SO 模式、OdinAttributeProcessor 注入、GUITable 缓存、双语系统、命名速查等 `cf6126c`
- 新增 `SCRIPT_DOC_GENERATOR_GUIDE.md` ScriptDocGenerator 模块编码规范，涵盖架构分层、单例、重置、事件通信、文件输出等 `cf6126c`
- 新增 `UTILITIES_GUIDE.md` Utilities 编码指南文档 `45a4837`

#### Changed
- `AESIR_INSPECTOR_CODE_STYLE_GUIDE.md` 移除 #region Internal 规则，简化 Odin Inspector 集成规范 `45a4837` `cf6126c`

---

## [0.3.0] - 2026-04-25

### Core

#### Added
- 新增菜单路径与优先级统一管理类 `AesirInspectorMenuItems`，统一管理 Tools 菜单和 Assets 上下文菜单 `77f3b1b`
- 新增 Getting Started 窗口，展示版本号、功能列表和文档链接 `77f3b1b`
- 新增 Preferences 偏好设置窗口，集成语言设置 `77f3b1b`
- 新增 `AesirInspectorVersion` 版本信息静态类 `77f3b1b`
- 新增 `IAesirInspectorReset` 重置接口及 `AesirInspectorResetAttributeProcessor`，为实现该接口的类自动添加右键重置菜单 `77f3b1b`
- 新增代码语法高亮器 `AesirCodeHighlighter` `77f3b1b`

#### Changed
- 静默安装检测日志输出（注释掉 `Debug.Log`） `77f3b1b`
- 扩展 `AesirInspectorPaths` 路径常量，新增 AttributeOverview 和 MiniTools 路径 `77f3b1b`
- 扩展 `AesirInspectorWebLinks` 链接常量，新增 GitHub 仓库、许可证、更新日志和 Odin Inspector 文档链接 `77f3b1b`

### Bilingual

#### Added
- 新增 `ShowEnablePropertyAttribute` 复合特性 `2ac8573`
- 新增 `HorizontalSeparateWidget` 水平分隔线 Inspector 组件 `2ac8573`

#### Changed
- 重构 `HeaderBilingualWidget` `2ac8573`

### Utilities

#### Added
- 新增 `AesirInspectorLogger` 日志工具类 `2ac8573`
- 新增 `PathUtility`、`PathSafeEditorUtility` 路径工具类 `2ac8573`
- 新增 `ReflectionUtility` 反射工具类 `2ac8573`
- 新增 `RegexUtility` 正则表达式工具类 `2ac8573`
- 新增 `HierarchyUtility`、`HierarchySafeEditorUtility` Hierarchy 工具类 `2ac8573`
- 新增 `MonoScriptSafeEditorUtility` MonoScript 工具类 `2ac8573`
- 新增 `PlayerLoopUtility` PlayerLoop 工具类 `2ac8573`
- 新增 `PredefinedAssemblyUtility` 预定义程序集工具类 `2ac8573`
- 新增 `ProjectSafeEditorUtility` 项目安全编辑器工具类 `2ac8573`

#### Changed
- 扩展 `ScriptableObjectSafeEditorUtility`，新增大量 ScriptableObject 编辑器操作方法 `2ac8573`
- 扩展 `OdinInspectorSafeEditorUtility` 和 `UrlUtility` `2ac8573`

### MiniTools

#### Added
- 新增 `AesirInspectorMiniToolsWindow` MiniTools 主窗口 `b7068eb`
- 新增 MenuItemViewer 菜单项检查器，支持 `IAssemblyFilter` 程序集过滤和 `ISearchFilterable` 搜索 `b7068eb`
- 新增 OdinSyntaxHighlighter 语法高亮处理器面板，委托 `AesirCodeHighlighter` 实现 `b7068eb`
- 新增 QuickCreateSO 右键快捷生成 ScriptableObject 工具，支持单选和多选批量创建 `b7068eb`

### ScriptDocGenerator

#### Added
- 新增文档生成器窗口和可视化面板 `ScriptableObject` 单例 `c2f2e75`
- 新增文档生成器逻辑控制类 `ScriptDocGeneratorController` `c2f2e75`
- 新增 Assets 上下文菜单项，支持添加脚本到 TargetType 或 TemporaryTypes `c2f2e75`
- 新增中文 Scripting API 配置和文档生成器设置 `c2f2e75`
- 新增完整的类型分析数据模型层：`MemberData`、`FieldData`、`PropertyData`、`MethodData`、`ConstructorData`、`EventData`、`ParameterData`、`TypeData` 及对应接口 `c2f2e75`
- 新增类型分析器静态扩展 `TypeAnalyzerStaticExtensions` 和工具类 `TypeAnalyzerUtility` `c2f2e75`
- 新增 `AccessModifierType`、`TypeCategory`、`ParameterDirection` 枚举 `c2f2e75`
- 新增 `DefaultAnalysisDataFactory`、`DefaultAttributeFilter`、`DerivedMemberDataComparer` 核心工具 `c2f2e75`
- 新增 `ReferenceLinkURLAttribute` 引用链接特性 `c2f2e75`

### AttributeOverview

#### Added
- 新增特性概览窗口 `AttributeOverviewWindow` 和数据库 `AttributeOverviewDatabaseSO` `0e53a40`
- 新增面板抽象框架：泛型基类 `AttributeOverviewPanelSO<T>`、`AbstractAttributePanelSO` 及 Odin AttributeProcessor 自动配置 `0e53a40`
- 新增 AssetList、AssetsOnly、CustomValueDrawer 三个内置特性面板 `0e53a40`
- 新增 `AesirExampleAttribute`、`AttributeCategoryAttribute` 特性标记 `0e53a40`
- 新增特性数据模型 `AbstractAttributeData`、`ParameterValue`、`ResolvedStringParameterValue`、`AttributeExamplePreviewItem` `0e53a40`
- 新增 `AesirAttributeCategory` 分类枚举和 `OdinInspectorDocumentationLinks` 文档链接常量 `0e53a40`
- 新增特性概览编辑器工具类和用法示例 `0e53a40`

### SummaryTool

#### Added
- 新增 XML Summary 注释处理工具 `XmlSummaryTool`，支持 Sync/Replace/Remove 操作 `2ac8573`
- 新增 `XmlCodePart` XML 代码段解析类 `2ac8573`
- 新增 SummaryTool Assets 上下文菜单项 `2ac8573`

### ExtensionManager

#### Added
- 新增扩展包管理器窗口 `ExtensionPackageManagerWindow`，支持 Git URL 安装 `2ac8573`
- 新增 `ExtensionPackageCard` 扩展包卡片数据类 `2ac8573`
- 新增 `PackageManagerEditorUtility` Package Manager 编辑器工具类 `2ac8573`

### Samples

#### Added
- 新增 PluginConfigSolutions 示例模块，演示 ScriptableSingleton 在 Preferences 和 Project 中的使用方式 `2ac8573`
- 新增 RuntimeInitializeLoadType 示例模块，演示五个初始化时机的执行顺序与最佳实践 `2ac8573`

### Tests

#### Added
- 新增 ScriptDocGenerator 完整单元测试，覆盖构造方法、事件、字段、方法、属性、类型数据及成员继承 `1cf6d6d`
- 新增 SummaryTool XML 注释处理测试 `1cf6d6d`
- 新增 UnityEngine.Object 运算符重载 Runtime 测试 `1cf6d6d`

#### Changed
- 为两个测试 asmdef 添加 `ODIN_INSPECTOR` 编译约束 `1cf6d6d`

---

## [0.2.1] - 2026-04-23

### Added

- 新增 Aesir Inspector 安装方式检测功能 `b7de538`

---

## [0.2.0] - 2026-04-23

### Added

- 实现双语 Inspector 系统与核心基础设施 `a2c750b`
- 新增 Codely Skills Library 示例，包含 custom-package-creator 技能 `9422695`

---

## [0.1.0] - 2026-04-22

### Added

- Initial release.

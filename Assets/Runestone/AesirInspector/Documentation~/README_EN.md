# Aesir Inspector

[中文](../README.md) | [![license](https://img.shields.io/badge/license-MIT-green.svg)](../LICENSE.md)
[![Version](https://img.shields.io/badge/version-0.18.0-blue.svg)](../CHANGELOG.md)
[![Install via Git URL](https://img.shields.io/badge/UPM-Git%20URL-blueviolet.svg)](#installation)

> 📦 **This package is published from its own repository, [AesirInspector](https://github.com/yuumixcode/AesirInspector)** (package folder `Assets/Runestone/AesirInspector`). It does **not** depend on any other Aesir package and can be installed on its own.
>
> ⚠️ **Hard dependency on [Odin Inspector](https://odininspector.com/)**: this package requires Odin Inspector to compile and run properly; without it the assemblies are skipped entirely by the `ODIN_INSPECTOR` define constraint (no errors) and the features are unavailable. Make sure Odin Inspector 3.3.x+ is installed in your project.
>
> Related packages (in the [AesirFramework](https://github.com/yuumixcode/AesirFramework) monorepo, independent of this package):
> - **[Aesir Architecture](https://github.com/yuumixcode/AesirFramework)** (standalone)
> - **[Aesir Modules](https://github.com/yuumixcode/AesirFramework)** (depends on Architecture)

`Aesir Inspector` is a Unity editor extension library designed to provide bilingual Inspector UI, safe editor tooling, and more. It builds on Odin Inspector for enhanced Inspector rendering and styling.

> 📤 **The Script Doc Generator and Summary Tool have moved out of this package**: since 0.15.0 they live in the standalone in-repo tool [`Assets/ScriptDocGenerator`](../../../ScriptDocGenerator/README.md) and no longer belong to this package.

## Who Is This For

- **Editor tool developers**: developers building custom Inspector tools who need bilingual (Chinese/English) UI display support.
- **Cross-region / cross-locale teams**: teams with diverse language backgrounds that need both Chinese and English shown in the Inspector panel to reduce communication cost.
- **Unity editor users**: developers who want safe editor utilities and other practical editor features.
- **Odin Inspector users**: developers who already use Odin Inspector and want richer attribute decorators and an enhanced Inspector experience.
- **Code standards advocates**: teams that want consistent code style and documentation standards to improve maintainability.

## Installation

### Install via Git URL

1. Open the Unity Package Manager window.
2. Click the `+` button in the top-left corner and choose `Add package from git URL...`.
3. Enter the following URL:

   ```
   https://github.com/yuumixcode/AesirInspector.git?path=/Assets/Runestone/AesirInspector
   ```

### Install via manifest.json

Add the following to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "cn.runestone.aesir-inspector": "https://github.com/yuumixcode/AesirInspector.git?path=/Assets/Runestone/AesirInspector"
  }
}
```

### Installation Mode Detection

Aesir Inspector automatically detects how it was installed (UPM / Assets folder) at editor load time and exposes static properties through `AesirInspectorInstallationChecker`:

- `InstallMode`: current install mode (`Upm` / `AssetFolder` / `Unknown`).
- `IsUpm`: whether the package was installed via UPM.
- `IsAssetFolder`: whether the package lives inside the Assets folder (Asset Store import or Git submodule).

### Importing Samples

The package ships importable samples under `Samples~`. Select `Aesir Inspector` in the Package Manager, expand the **Samples** group and click `Import` on a sample (it lands in `Assets/Samples/Aesir Inspector/<version>/`):

| Sample | Description |
|--------|-------------|
| **Plugin Config Solutions** | Using `ScriptableSingleton` in Preferences and Project — best practices for persisting editor configuration |
| **RuntimeInitializeLoadType** | Execution order and best practices for the five `RuntimeInitializeOnLoadMethod` timings |

## Requirements

- **Unity**: 2022.3 or newer.
- **Odin Inspector**: 3.3.x or newer (**hard dependency**; without it the assemblies are skipped and its features are unavailable).

## Core Features

### 1. Attribute Overview Ultra

A searchable tree menu that shows all registered Odin Inspector and Aesir Inspector attribute panels, with live previews and sample code for each attribute.

- **Category browsing**: browse by Essentials / Buttons / Collections / Groups / Conditionals and more.
- **Search**: fuzzy search to quickly locate an attribute.
- **Live preview**: selecting an attribute shows its effect and parameter configuration in the right panel.
- **Code preview**: selecting an attribute also shows the corresponding sample source code.
- **Zero asset pollution**: panels and examples are in-memory instances; user debug state persists via the `UltraStateStore`, no sub-assets are generated in the Project.
- Open via `Tools → Aesir → Inspector → Attribute Overview Ultra`.

### 2. Mini Tools

A collection of handy editor utilities, opened from `Tools → Aesir → Inspector → Mini Tools`.

| Tool | Description |
|------|------|
| **MenuItem Viewer** | Collects and lists every `[MenuItem]` in the project, filterable by assembly and searchable — handy for planning menu structure |
| **Syntax Highlighter** | A visual panel over Odin's built-in syntax highlighter; paste source to test highlighting and export rich-text markup |
| **Quick Create SO** | Right-click a MonoScript in the Project window to quickly create a ScriptableObject asset; supports multi-select batch creation |

## Infrastructure

### 3. Bilingual Attributes

A complete set of bilingual property decorators and Inspector controls that display Chinese and English side by side in the Inspector. Built for:

- **Editor tool development**: when your own editor tools need Inspector UI in both Chinese and English so users of either language understand every parameter.
- **Team collaboration**: bilingual display lowers communication cost for cross-region teams sharing a project, avoiding mistakes caused by language differences.

Available decorators and controls:

- `[BilingualTitle]`
- `[BilingualButton]`
- `[BilingualInfoBox]`
- `[BilingualText]`
- `BilingualDisplayAsStringControl` — bilingual read-only text control
- `BilingualHeaderControl` — bilingual header control
- `HorizontalSeparateControl` — horizontal separator control

### 4. Odin Integration

Odin Inspector is a hard dependency — the package uses Sirenix (Odin) APIs directly for all of its enhanced capabilities:

- Bilingual attributes, Inspector controls, attribute drawers, and processors are built directly on Odin's attribute/drawer system.
- Attribute Overview Ultra is built on Odin's menu editor window and editor window infrastructure.
- Without Odin Inspector the assemblies are skipped entirely (no errors) and the features are unavailable; install Odin 3.3.x+ from [odininspector.com](https://odininspector.com/) first.

### 5. Safe Editor Utilities

Safe wrappers around Unity Editor APIs so editor-only code is stripped automatically in builds.

| Utility | Description |
|-------|------|
| `ScriptableObjectSafeEditorUtility` | More reliable ScriptableObject asset creation and management |
| `MonoScriptSafeEditorUtility` | Find and select MonoScript assets by script name |
| `PathUtility` | Path string tools: Unity path normalization, sub-path extraction, path combining |
| `PathSafeEditorUtility` | Safe creation that guarantees folders exist under Assets |
| `HierarchySafeEditorUtility` | Get a GameObject's absolute path in the Hierarchy |
| `HierarchyUtility` | Transform hierarchy path operations: full path, relative path, deep child lookup |
| `ProjectSafeEditorUtility` | Ping and select any project asset (folders supported) |
| `UrlUtility` | Convenient URL opening and external link handling |
| `ReflectionUtility` | Assembly and namespace reflection helpers |
| `PredefinedAssemblyUtility` | Predefined assembly type detection and interface implementation lookup |
| `PlayerLoopUtility` | Custom Unity PlayerLoop: insert/remove subsystems, print the PlayerLoop structure |
| `RegexUtility` | Regex tools: namespace/class name normalization, email/URL validation |
| `AesirInspectorDebug` | Unified logging (Info/Warning/Error, optional prefix), stripped automatically in builds; configurable via `AesirInspectorDebugSettings` |

### 6. Code Style and Standards

This project treats code style as being as important as features. Strict coding standards and examples are built in to keep team collaboration consistent and maintainable:

- **Style guide**: see `Runtime/CodeStyle/AesirInspectorCodeStyle.cs`.
- **Philosophy**: good code style is not optional — it is the foundation of project quality. All contributors are expected to follow it.

## Usage Example

```csharp
using Runestone.AesirInspector;
using Sirenix.OdinInspector;
using UnityEngine;

public class ExampleMonoBehaviour : MonoBehaviour
{
    [BilingualTitle("玩家属性", "Player Stats")]
    [SerializeField]
    private int health;

    [BilingualButton("重置属性", "Reset Stats")]
    private void ResetStats()
    {
        health = 100;
    }
}
```

## License

Released under the MIT license. See [LICENSE.md](../LICENSE.md) for details.

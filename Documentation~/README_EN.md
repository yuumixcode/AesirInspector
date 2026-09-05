# Aesir Inspector

[中文](../README.md) | [![license](https://img.shields.io/badge/license-MIT-green.svg)](../LICENSE.md)
[![Version](https://img.shields.io/badge/version-0.14.0-blue.svg)](../CHANGELOG.md)
[![Install via Git URL](https://img.shields.io/badge/UPM-Git%20URL-blueviolet.svg)](#installation)

> 📦 **This package is part of the [Unity-Aesir-Packages](https://github.com/yuumixcode/Unity-Aesir-Packages) monorepo**. This package does **not** depend on other Aesir sub-packages (installable independently).
>
> ⚠️ **Hard dependency on [Odin Inspector](https://odininspector.com/)**: this package requires Odin Inspector to compile and run properly. Make sure Odin Inspector 3.3.x+ is installed in your project.
>
> Related packages:
> - **[Aesir Architecture](https://github.com/yuumixcode/Unity-Aesir-Packages)** (standalone)
> - **[Aesir Modules](https://github.com/yuumixcode/Unity-Aesir-Packages)** (depends on Architecture)

`Aesir Inspector` is a Unity editor extension library designed to provide bilingual Inspector UI, safe editor tooling, a script documentation generator, and more. It builds on Odin Inspector for enhanced Inspector rendering and styling.

## Who Is This For

- **Editor tool developers**: developers building custom Inspector tools who need bilingual (Chinese/English) UI display support.
- **Cross-region / cross-locale teams**: teams with diverse language backgrounds that need both Chinese and English shown in the Inspector panel to reduce communication cost.
- **Unity editor users**: developers who want safe editor utilities, a documentation generator, Summary sync tools, etc.
- **Odin Inspector users**: developers who already use Odin Inspector and want richer attribute decorators and an enhanced Inspector experience.
- **Code standards advocates**: teams that want consistent code style and documentation standards to improve maintainability.

## Installation

### Install via Git URL

1. Open the Unity Package Manager window.
2. Click the `+` button in the top-left corner and choose `Add package from git URL...`.
3. Enter the following URL:

   ```
   https://github.com/yuumixcode/AesirInspector.git?path=Assets/Runestone/AesirInspector
   ```

### Install via manifest.json

Add the following to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "cn.runestone.aesir-inspector": "https://github.com/yuumixcode/AesirInspector.git?path=Assets/Runestone/AesirInspector"
  }
}
```

### Installation Mode Detection

Aesir Inspector automatically detects how it was installed (UPM / Assets folder) at editor load time and exposes static properties through `AesirInspectorInstallationChecker`:

- `InstallMode`: current install mode (`Upm` / `AssetFolder` / `Unknown`).
- `IsUpm`: whether the package was installed via UPM.
- `IsAssetFolder`: whether the package lives inside the Assets folder (Asset Store import or Git submodule).

## Requirements

- **Unity**: 2022.3 or newer.
- **Odin Inspector**: 3.3.x or newer (**hard dependency**; the package will not compile without it).

## Core Features

### 1. Attribute Overview Pro

A searchable tree menu that shows all registered Odin Inspector and Aesir Inspector attribute panels, with live previews and sample code for each attribute.

- **Category browsing**: browse by Essentials / Buttons / Collections / Groups / Conditionals and more.
- **Search**: fuzzy search to quickly locate an attribute.
- **Live preview**: selecting an attribute shows its effect and parameter configuration in the right panel.
- **Code preview**: selecting an attribute also shows the corresponding sample source code.
- Open via `Tools → Aesir → Inspector → Attribute Overview Pro`.

### 2. Script Doc Generator

Analyzes C# type information via reflection to generate structured API documentation, with incremental generation and personal extension support.

#### When to use it

When your team, open-source project, or personal project needs API documentation — not just auto-generated type signatures, but personalized notes per API (usage examples, caveats, business context) — Script Doc Generator is built for exactly that. It generates the accurate API signature part; you write the part only a human can write. The two never interfere, and updates are incremental.

#### Key advantages

- **🔒 Fully offline**: based on C# reflection — no network, no external APIs, no third-party services. Works in air-gapped environments, intranets, and confidential projects.
- **⚡ Zero waiting**: reflection analysis completes in milliseconds. Select a type and the doc appears. No progress bars.
- **🎮 Unity-native**: runs as a native editor extension, operated directly in the Inspector. No window switching, no external toolchain — documentation lives where you write code.
- **✏️ Incremental generation**: regenerating a doc preserves handwritten content after the `## 额外说明` (Additional Notes) marker, as well as any existing front matter (YAML/TOML headers). Generated signatures and human notes never overwrite each other.
- **🤖 AI-friendly**: generates Markdown by default with a clear structure and explicit semantics — ready for AI API Q&A knowledge bases (RAG, embeddings, etc.).
- **🔧 Configurable and extensible**: multiple options and extension points adapt the output to your project (see below).

#### Feature details

- **Type analysis**: full signatures and attributes for classes, structs, interfaces, enums, delegates, records, including generic constraints, base-class inheritance, and interface implementations.
- **Field analysis**: all primitive C# types, collection types, delegate types, special types (abstract/dynamic/interface/nullable), plus const/static default values, access modifiers, compound keywords (const/static readonly/readonly), Unity built-in types, and attribute annotations.
- **Property analysis**: asymmetric getter/setter access modifiers (e.g. `public get / private set`), static properties, default value initializers.
- **Method analysis**: generic methods, parameter defaults, `params`, `async`, operator overloads, extension methods.
- **Inheritance analysis**: identifies virtual/abstract/override methods and interface implementations, and traces where members come from in the inheritance chain.
- **Helpers**: member ordering (`DerivedMemberDataComparer`), overload markers, constructor signature generation, event signature generation.

#### Configuration options

| Option | Description |
|-------|------|
| Documentation output path | Custom target folder for generated docs, drag-and-drop supported |
| Folder per namespace | When enabled, creates subdirectories per namespace, e.g. `Runestone.AesirInspector` → `Runestone/AesirInspector/` |
| Custom file extension | Defaults to `.md`; switch to `.mdx`, `.txt`, or any extension |
| Incremental marker | When enabled, inserts a `## 额外说明` (Additional Notes) section at the end; regeneration preserves handwritten content after it |
| Type source mode | Single type / multiple types / whole assembly — three granularities |
| TypesCacheSO | Saves a Type list as a reusable asset so you don't reselect every time |

#### Extension points

| Interface | Description |
|------|------|
| `DocGeneratorSettingsSO` | Inherit this abstract class and implement `GetGeneratedDoc(ITypeData)` to customize doc format and content. The built-in `CnScriptingAPISettingsSO` (Chinese API Markdown generator) is the reference implementation |
| `IAnalysisDataFactory` | Replace the entire type-analysis factory with custom member parsing |
| `IAttributeFilter` | Custom attribute filter controlling which attributes appear in generated docs |

#### Unit test coverage

Script Doc Generator currently ships with **153 unit tests** covering signature generation across data types. Coverage keeps growing with each iteration:

| Test module | Tests | Description |
|---------|-------|------|
| **FieldData** · signatures | 41 | Signature generation for primitives, collections, delegates, special types (abstract/dynamic/interface/nullable) |
| **FieldData** · default values | 32 | const and static field default values, including decimal edge cases |
| **FieldData** · modifiers | 10 | Compound keywords (const/static readonly/readonly) and all 6 access modifiers |
| **FieldData** · Unity | 7 | Unity built-in types (GameObject/Transform/Rigidbody, etc.) and attributes (SerializeField/Range/ColorUsage/Obsolete) |
| **PropertyData** | 13 | Static property defaults, asymmetric getter/setter modifier combinations |
| **MethodData** · general | 11 | Generic methods, default parameters, `params`, `async`, static methods |
| **MethodData** · inheritance | 5 | Inheritance analysis for virtual/abstract/override methods and interface implementations |
| **MethodData** · operators | 8 | Arithmetic operator overloads, implicit/explicit conversion operators |
| **MethodData** · extension | 1 | Extension method signatures with the `[Ext]` marker |
| **ConstructorData** | 1 | Constructor signatures, including base-class constructor calls |
| **EventData** | 6 | Action/Func/Predicate/Comparison delegate events and static events |
| **TypeData** | 14 | class/struct/interface/enum/delegate/record/static/sealed/generic declarations, with attributes and generic constraints |
| **MemberData** · inheritance | 4 | `IsFromInheritance` marking for fields/properties/events/methods inherited from base classes |

### 3. Summary Tool

Right-click menu shortcuts that keep XML `<summary>` comments and `[Summary]` attributes in sync in C# scripts.

#### When to use it

When your team requires public members to carry both an XML doc comment and a `[Summary]` attribute, manually maintaining two copies of the same text in different formats is tedious and error-prone. Summary Tool extracts the summary from the XML comment and generates the matching `[Summary]` attribute automatically, keeping both in sync.

#### Key advantages

- **⚡ Right-click and go**: select a script in the Project window, right-click, done — no extra windows.
- **🔄 Three modes**: Sync, Replace, and Remove cover all routine maintenance.
- **📦 Batch processing**: multi-select scripts and process them in one go.
- **🧠 Smart imports**: automatically adds `using Runestone.AesirInspector;` after processing.
- **🏗️ Macro aware**: recognizes `#if` and other preprocessor directives, making sure `[Summary]` is inserted inside conditional-compilation blocks.

#### How it works

`XmlSummaryTool` processes source in three phases: **Parse → Group → Output**.

**1. Parse**: scans the source line by line, locates the first `///` comment, marks everything before it as the **Header** (usings, namespace, etc.), and feeds the rest into grouping.

**2. Group**: starting from the first `///`, alternately extracts **XML comment blocks** (consecutive `///` lines) and **code blocks** (non-`///` lines) into an `XmlCodePart` list. Each `XmlCodePart` consists of `xml` (the comment part) and `code` (the code part).

**3. Output**: depending on the selected mode, each `XmlCodePart` is handled differently:

| Mode | Output combination | Description |
|------|---------|------|
| **Sync** | `xml` + `leading directives` + `[Summary]` + `code after removing first [Summary]` | Keeps the XML comment and adds/updates `[Summary]` after preprocessor directives; an existing `[Summary]` is replaced with the XML content |
| **Replace** | `xml without summary` + `leading directives` + `[Summary]` + `code after removing first [Summary]` | Removes the `<summary>` tags and substitutes a `[Summary]` attribute; existing `[Summary]` content syncs to the XML text |
| **Remove** | `xml` + `leading directives` + `code after removing all [Summary]` | Only removes all `[Summary]` attributes, keeping the XML comments |

**Macro awareness**: when a code block starts with `#if`, `#elif`, `#else`, or similar preprocessor directives, the `[Summary]` attribute is inserted after those directives (i.e. inside the conditional block), not before. For example:

```csharp
// Input
/// <summary>Editor-only method.</summary>
#if UNITY_EDITOR
[Summary("old content")]
public void Reset() { }
#endif

// Sync output — [Summary] inside #if
/// <summary>Editor-only method.</summary>
#if UNITY_EDITOR
[Summary("Editor-only method.")]
public void Reset() { }
#endif
```

Finally, the output phase checks whether the Header already contains `using Runestone.AesirInspector;` and adds it if missing.

### 4. Mini Tools

A collection of handy editor utilities, opened from `Tools → Aesir → Inspector → Mini Tools`.

| Tool | Description |
|------|------|
| **MenuItem Viewer** | Collects and lists every `[MenuItem]` in the project, filterable by assembly and searchable — handy for planning menu structure |
| **Syntax Highlighter** | A visual panel over Odin's built-in syntax highlighter; paste source to test highlighting and export rich-text markup |
| **Quick Create SO** | Right-click a MonoScript in the Project window to quickly create a ScriptableObject asset; supports multi-select batch creation |

### 5. Extension Package Manager

Quickly install recommended Aesir-family and other popular open-source Unity Packages via Git URL.

- **One-click install/remove**: card-style UI shows the install state of recommended packages; click to install or remove.
- **Auto detection**: the window detects installed package states on open and refreshes after install/remove.
- Open via `Tools → Aesir → Inspector → Extension Package Manager`.

## Infrastructure

### 6. Bilingual Attributes

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

### 7. Odin Integration

Odin Inspector is a hard dependency — the package uses Sirenix (Odin) APIs directly for all of its enhanced capabilities:

- Bilingual attributes, Inspector controls, attribute drawers, and processors are built directly on Odin's attribute/drawer system.
- Attribute Overview Pro and the Extension Package Manager are built on Odin's menu editor window and editor window infrastructure.
- The package will not compile without Odin Inspector; install Odin 3.3.x+ from [odininspector.com](https://odininspector.com/) first.

### 8. Safe Editor Utilities

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

### 9. Custom Attributes

| Attribute | Description |
|------|------|
| `[Summary]` | Comment attribute, equivalent to the `<summary>` part of an XML doc comment; readable at runtime via `GetSummary()` |

### 10. Code Style and Standards

This project treats code style as being as important as features. Strict coding standards and examples are built in to keep team collaboration consistent and maintainable:

- **Style guide**: see `Runtime/Unity/CodeStyle/AesirInspectorCodeStyle.cs`.
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

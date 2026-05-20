# Aesir Inspector — AI Core Context

> 本文件是 AI Agent 的核心上下文文档，等同于 `AGENTS.md`。所有代码生成、重构、审查操作**必须**遵循本文档中的规范。

Aesir Inspector (`cn.runlab.aesir-inspector`) 是一个 Unity/Tuanjie 编辑器扩展库，提供双语 Inspector UI、安全编辑器工具集、脚本文档生成器、XML Summary 同步工具等功能。**可选集成 Odin Inspector**。

- **Version**: 0.4.0-pre.1 | **License**: MIT
- **Engine**: Tuanjie (Unity 2022.3 fork) | **Language**: C# / .NET Standard 2.1
- **Repo**: https://github.com/yuumixcode/aesir-inspector

## Assemblies

| Assembly | Odin 依赖 |
|----------|-----------|
| `RunLab.AesirInspector` | 无 |
| `RunLab.AesirInspector.Editor` | 无 |
| `RunLab.AesirInspector.OdinIntegration` | `ODIN_INSPECTOR` |
| `RunLab.AesirInspector.OdinIntegration.Editor` | `ODIN_INSPECTOR` |
| `RunLab.AesirInspector.Tests` | 无 |
| `RunLab.AesirInspector.Editor.Tests` | 无 |

## Directory Structure

```
Aesir Inspector/
├── Runtime/
│   ├── Unity/                     # Core runtime (RunLab.AesirInspector)
│   │   ├── Attributes/            # [Summary]
│   │   ├── Core/                  # Version, Paths, WebLinks, IAesirInspectorReset
│   │   ├── Inspector/             # Inspector 显示模型
│   │   ├── Localization/          # 本地化数据与语言设置
│   │   ├── Logging/               # 日志系统
│   │   ├── OdinBridge/            # IOdinBridge, DefaultOdinBridge, OdinBridgeLocator
│   │   ├── ScriptDocGenerator/    # 文档生成器运行时模型
│   │   └── Utilities/             # SafeEditorUtility 工具集
│   └── Odin Integration/          # Odin runtime (ODIN_INSPECTOR)
│       ├── Attributes/            # 双语特性
│       └── OdinCodeHighlighter.cs
├── Editor/
│   ├── Unity/                     # Core editor (RunLab.AesirInspector.Editor)
│   │   ├── Core/                  # 安装检测、菜单管理
│   │   ├── MiniTools/             # QuickCreateSO
│   │   └── SummaryTool/           # XML Summary 同步
│   └── Odin Integration/          # Odin editor (ODIN_INSPECTOR)
│       ├── AttributeOverviewPro/  # 特性总览窗口
│       ├── AttributeProcessors/   # OdinAttributeProcessor
│       ├── Bridge/                 # OdinInspectorBridge
│       ├── Drawers/               # 双语 Drawer
│       ├── ExtensionManager/      # 扩展包管理器
│       ├── MiniTools/              # MenuItem Viewer, Syntax Highlighter
│       ├── ScriptDocGenerator/    # 文档生成器编辑器逻辑
│       └── Windows/                # Getting Started, Preferences
├── Tests/Editor/ & Tests/Runtime/
├── Samples~/
└── Documentation~/               # aesir-inspector.md (用户) + development.md (开发者)
```

## Key Rules

- **严禁**对 `UnityEngine.Object` 派生类使用 `?.` / `??`
- 私有方法对应公开方法时，增加 `Internal_` 前缀
- `#if UNITY_EDITOR` 包裹编辑器专用代码
- Odin 依赖代码**必须**放在 `Odin Integration/` 子目录
- 核心程序集**不允许**直接引用 Odin API — 通过 `IOdinBridge` 桥接
- 版本号需在 `package.json` 和 `AesirInspectorVersion.cs` 两处同步

## Coding Conventions

### 注释规范（自文档化代码 / 无注释范式）

采用**自文档化代码（Self-documenting Code）**原则：通过清晰、准确的命名（变量名、函数名、类名）和合理的代码结构来传达意图，而非依赖注释。

遵循**无注释范式（No-comment Paradigm）**：代码本身应足够清晰，注释只用于解释"为什么"这样做，而不是"做了什么"。

| 规则 | 说明 |
|------|------|
| **禁止 XML 注释** | 不使用 `/// <summary>`、`/// <param>`、`/// <returns>` 等 XML 文档注释 |
| **复杂逻辑用 `[Summary]`** | 仅在命名无法充分表达意图的复杂地方使用 `[Summary("...")]` 解释"为什么" |
| **命名即文档** | 方法、字段、属性的命名应清晰表达意图，无需额外注释 |
| **类必须 `[Summary]`** | 所有类（class / struct / interface）必须具备 `[Summary("...")]`，解释"为什么" |
| **唯一例外** | `AesirInspectorCodeStyle.cs` 作为风格示例文件，同时保留 XML 注释和 `[Summary]` |

#### 免除规范的模块

以下模块为展示/示例用途，不适用通用注释规范，使用 `//` 单行/多行注释进行特殊性补充即可：

- `Runtime/Unity/CodeStyle/` — 代码风格示例文件
- `Editor/Odin Integration/AttributeOverviewPro/Data/` — 属性数据类
- `Editor/Odin Integration/AttributeOverviewPro/AttributePanels/` — Panel SO 定义
- `Editor/Odin Integration/AttributeOverviewPro/UsageExamples/` — 示例 SO

```csharp
// ✅ 自文档化：命名清晰传达意图，无需注释
public int MaxRetryCount { get; }
public void ApplyDamage(float amount) { }

// ✅ 无注释范式：仅用 [Summary] 解释"为什么"这样设计，而非"做了什么"
[Summary("后者覆盖前者，用于多配置源优先级合并")]
public void MergeConfigSources(IReadOnlyList<ConfigSource> sources) { }

// ❌ 禁止 XML 注释
/// <summary>
/// 应用伤害值
/// </summary>
public void ApplyDamage(float amount) { }
```

### 命名规范

| 标识符 | 规则 | 示例 |
|--------|------|------|
| 类、接口 | `PascalCase`，接口 `I` 前缀 | `PlayerManager`, `IDamageable` |
| 私有非序列化字段 | `_camelCase` | `_health` |
| 序列化字段 `[SerializeField]` | `camelCase` | `moveSpeed` |
| 常量 / 静态只读 | `PascalCase` | `MaxScore` |

### Enum 规范

- 普通：含 `None = 0`，显式赋值
- Flags：`[Flags]`，值为 `1 << n`，复合用 `|`

### 事件规范

| 角色 | 命名 | 示例 |
|------|------|------|
| 事件 | 无 `On` 前缀 | `DoorOpened` |
| 订阅方法 | `On` + 事件名 | `OnDoorOpened` |
| 触发方法 | `Raise` + 事件名 | `RaiseDoorOpened` |

### Utility 命名

| 类别 | 命名规则 | 目录 |
|------|----------|------|
| Runtime | `XxxUtility` | `Runtime/Unity/Utilities/` |
| Editor 安全封装 | `XxxSafeEditorUtility` | `Runtime/Unity/Utilities/` |
| Editor-Only | `XxxEditorUtility` | `Editor/Unity/` |

### Odin Inspector 规范

- Odin 依赖代码**必须**放在 `Odin Integration/` 子目录
- Processor：`internal sealed`，与目标类**同文件**定义，无需 `[Summary]`
- Drawer：继承 `OdinAttributeDrawer`，独立存于 `Drawers/` 目录，无需 `[Summary]`
- Processor 需 `nameof` 引用私有成员时，定义为**嵌套类**

## Menu Paths

- `Aesir Inspector → Getting Started` / `Attribute Overview Pro` / `Mini Tools` / `Extension Package Manager`
- `Assets 上下文菜单` — Quick Create SO, Summary Tool, Script Doc Generator

## Testing

Unity Test Runner → Edit Mode / Play Mode。测试程序集无 `ODIN_INSPECTOR` 约束。

## Documentation

- 用户文档 → `Documentation~/aesir-inspector.md`
- 开发者指南（架构、约定、模块、ADR、任务指南）→ `Documentation~/development.md`

# 贡献指南

感谢你对 Aesir Inspector 的关注！我们欢迎任何形式的贡献，包括但不限于 Bug 报告、功能建议、文档改进和代码贡献。

## 目录

- [行为准则](#行为准则)
- [如何贡献](#如何贡献)
- [开发环境搭建](#开发环境搭建)
- [项目结构](#项目结构)
- [编码规范](#编码规范)
- [提交 Pull Request](#提交-pull-request)
- [问题与帮助](#问题与帮助)

## 行为准则

本项目采用 [Contributor Covenant](https://www.contributor-covenant.org/version/2/1/code_of_conduct/) 行为准则。参与本项目即表示你同意遵守其条款。请以尊重和建设性的方式对待每一位社区成员。

## 如何贡献

### 报告 Bug

1. 在 [Issues](https://github.com/yuumixcode/AesirInspector/issues) 中搜索是否已有相同问题。
2. 如果没有，[创建新 Issue](https://github.com/yuumixcode/AesirInspector/issues/new)，包含以下信息：
   - **复现步骤**：详细描述如何触发该 Bug。
   - **预期行为**：你期望发生什么。
   - **实际行为**：实际发生了什么。
   - **环境信息**：Unity 版本、Odin Inspector 版本、操作系统。
   - **截图/日志**：如有可能，附上截图或错误日志。

### 建议功能

1. 在 [Issues](https://github.com/yuumixcode/AesirInspector/issues) 中搜索是否已有类似建议。
2. 如果没有，创建新 Issue 并使用 `Feature Request` 标签，包含以下信息：
   - **使用场景**：描述你希望解决的具体问题。
   - **建议方案**：描述你期望的解决方案。
   - **备选方案**：你考虑过的其他方案。

### 贡献代码

1. Fork 本仓库。
2. 基于 `Assets/Runestone/AesirInspector/` 进行开发。
3. 遵循下方 [编码规范](#编码规范)。
4. 提交 Pull Request 到 `main` 分支。

## 开发环境搭建

### 前置要求

- **Unity Editor** 2022.3.62f3c1 或更高版本
- **Git**：用于版本控制
- **Odin Inspector** 3.3.x 或更高（硬依赖）

### 克隆项目

```bash
git clone git@github.com:yuumixcode/AesirInspector.git
```

本仓库本身即一个 Unity 工程项目，克隆后直接用 Unity 打开仓库根目录即可。包源码位于 `Assets/Runestone/AesirInspector/`。

### Odin Inspector

Odin Inspector 是硬依赖，未安装时包无法编译。请先通过 Odin 官方安装器为项目安装 Odin 3.3.x+。

## 项目结构

```
AesirInspector/                        # 仓库根目录 = Unity 工程根目录
├── Assets/
│   └── Runestone/
│       └── AesirInspector/            # Aesir Inspector 包
│           ├── Runtime/               # 运行时程序集 (Runestone.AesirInspector)
│           │   ├── Common/            # 版本、路径、设置、接口等基础类型
│           │   ├── Debug/             # AesirInspectorDebug 日志系统
│           │   ├── ScriptDocGenerator/  # 文档生成器运行时模型
│           │   ├── Utilities/         # 安全编辑器工具集与 OdinCodeHighlighter
│           │   ├── Attributes/        # 双语特性
│           │   ├── Inspector/         # 双语 Inspector Control
│           │   ├── Localization/      # 本地化数据与语言设置
│           │   └── CodeStyle/         # 代码风格指南（可编译示例）
│           ├── Editor/                # 编辑器程序集 (Runestone.AesirInspector.Editor)
│           │   ├── Core/              # 安装检测、菜单管理
│           │   ├── Common/            # 模块资产标记
│           │   ├── MiniTools/         # QuickCreateSO, MenuItem Viewer, Syntax Highlighter
│           │   ├── AttributeOverviewPro/  # 特性总览窗口
│           │   ├── AttributeProcessors/   # OdinAttributeProcessor
│           │   ├── Drawers/           # 双语 Drawer
│           │   ├── ExtensionManager/  # 扩展包管理器
│           │   ├── ScriptDocGenerator/    # 文档生成器与 Summary 工具
│           │   └── Windows/           # Getting Started, Preferences
│           ├── Tests/
│           │   ├── Editor/            # 编辑器模式测试
│           │   └── Runtime/           # 运行时模式测试
│           ├── Samples~/              # 使用示例
│           ├── Documentation~/        # 用户文档与开发者指南
│           └── package.json
├── Packages/                          # 工程依赖清单
├── ProjectSettings/                   # Unity 工程设置
└── ...
```

### 程序集说明

| 程序集 | 说明 |
|--------|------|
| `Runestone.AesirInspector` | 运行时（Odin 硬依赖，直接使用 Sirenix API） |
| `Runestone.AesirInspector.Editor` | 编辑器（额外使用 Sirenix.OdinInspector.Editor） |

## 编码规范

请务必在提交代码前阅读并遵循以下规范。详细规范参见 `Runtime/CodeStyle/AesirInspectorCodeStyle.cs` 和 `Documentation~/development.md`（位于 `Assets/Runestone/AesirInspector/` 下）。

### 注释规范

本项目采用**自文档化代码**和**无注释范式**：

- **禁止 XML 注释**：不使用 `/// <summary>`、`/// <param>` 等 XML 文档注释。
- **命名即文档**：通过清晰的命名传达意图，无需额外注释。
- **`[Summary]` 仅用于复杂逻辑**：只在命名无法充分表达意图时使用 `[Summary("...")]`，解释"为什么"而非"做了什么"。

```csharp
// ✅ 自文档化：命名清晰，无需注释
public int MaxRetryCount { get; }
public void ApplyDamage(float amount) { }

// ✅ [Summary] 解释"为什么"
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

### Unity/C# 关键规则

- **严禁**对 `UnityEngine.Object` 派生类使用 `?.` / `??`。
- 私有方法对应公开方法时，增加 `Internal_` 前缀。
- `#if UNITY_EDITOR` 包裹编辑器专用代码。
- Odin Inspector 为硬依赖，可直接使用 Sirenix API，无需条件编译守卫。

### 事件规范

| 角色 | 命名 | 示例 |
|------|------|------|
| 事件 | 无 `On` 前缀 | `DoorOpened` |
| 订阅方法 | `On` + 事件名 | `OnDoorOpened` |
| 触发方法 | `Raise` + 事件名 | `RaiseDoorOpened` |

### Enum 规范

- 普通：含 `None = 0`，显式赋值。
- Flags：`[Flags]`，值为 `1 << n`，复合用 `|`。

### Utility 命名

| 类别 | 命名规则 | 目录 |
|------|----------|------|
| Runtime | `XxxUtility` | `Runtime/Utilities/` |
| Editor 安全封装 | `XxxSafeEditorUtility` | `Runtime/Utilities/` |
| Editor-Only | `XxxEditorUtility` | `Editor/` |

## 提交 Pull Request

### 流程

1. 确保存在对应的 Issue（Bug 修复或功能建议），如没有请先创建。
2. Fork 仓库并从 `main` 创建特性分支：
   ```bash
   git checkout -b feature/your-feature-name
   # 或
   git checkout -b fix/your-bug-fix-name
   ```
3. 编写代码并确保遵循编码规范。
4. 添加必要的单元测试（测试位于 `Tests/Editor/` 和 `Tests/Runtime/`）。
5. 确保所有现有测试通过。
6. 提交更改，提交信息使用现在时态，简洁描述变更：
   ```
   Add bilingual attribute processor for Button
   Fix null reference in installation checker
   ```
7. 推送到你的 Fork 并创建 Pull Request。
8. PR 描述中引用相关 Issue（如 `Closes #123`）。

### 分支命名

| 类型 | 格式 | 示例 |
|------|------|------|
| 功能 | `feature/<name>` | `feature/bilingual-toggle` |
| 修复 | `fix/<name>` | `fix/installation-checker-null-ref` |
| 文档 | `docs/<name>` | `docs/update-contributing-guide` |

### PR 检查清单

提交 PR 前，请确认：

- [ ] 代码遵循项目编码规范
- [ ] 未引入 XML 文档注释（使用 `[Summary]` 或自文档化命名替代）
- [ ] 未对 `UnityEngine.Object` 派生类使用 `?.` / `??`
- [ ] 编辑器专用代码已用 `#if UNITY_EDITOR` 包裹
- [ ] 已添加必要的单元测试
- [ ] 所有测试通过
- [ ] 提交信息简洁且使用现在时态

## 问题与帮助

- **Bug 报告 & 功能建议**：[GitHub Issues](https://github.com/yuumixcode/AesirInspector/issues)
- **讨论 & 提问**：[GitHub Discussions](https://github.com/yuumixcode/AesirInspector/discussions)
- **邮件**：zeriying@gmail.com

---

感谢你的贡献！每一次提交都让 Aesir Inspector 变得更好。

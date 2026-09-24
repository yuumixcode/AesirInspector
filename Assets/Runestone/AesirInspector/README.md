# Aesir Inspector

[English](Documentation~/README_EN.md) | [![license](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE.md)
[![Version](https://img.shields.io/badge/version-0.14.0-blue.svg)](CHANGELOG.md)
[![Install via Git URL](https://img.shields.io/badge/UPM-Git%20URL-blueviolet.svg)](#安装说明)

> 📦 **本包是 [Unity-Aesir-Packages](https://github.com/yuumixcode/Unity-Aesir-Packages) monorepo 的一部分**。本包**不依赖**其他 Aesir 子包（独立可装）。
>
> ⚠️ **强依赖 [Odin Inspector](https://odininspector.com/)**：本包需要 Odin Inspector 才能正常编译和运行。请确保项目中已安装 Odin Inspector 3.3.x+。
>
> 关联包：
> - **[Aesir Architecture](https://github.com/yuumixcode/Unity-Aesir-Packages)**（独立）
> - **[Aesir Modules](https://github.com/yuumixcode/Unity-Aesir-Packages)**（依赖 Architecture）

`Aesir Inspector` 是一个 Unity 编辑器扩展库，旨在提供双语 Inspector UI、安全编辑器工具集等功能。基于 Odin Inspector 提供增强的 Inspector 渲染和样式优化。

> 📤 **脚本文档生成器（Script Doc Generator）与 Summary 工具已迁移**：自 0.15.0 起迁移至本仓库的独立工具 [`Assets/ScriptDocGenerator`](../../ScriptDocGenerator/README.md)，不再属于本包。

## 适用人群

- **编辑器工具开发者**：正在开发自定义 Inspector 工具，需要双语（中/英）UI 显示支持的开发者。
- **跨国/跨地区协作团队**：团队成员语言背景不同，需要在 Inspector 面板中同时展示中英文信息以降低沟通成本。
- **Unity 编辑器用户**：希望获得安全编辑器工具等实用功能的开发者。
- **Odin Inspector 用户**：已有 Odin Inspector 并希望获得更丰富的属性装饰器与增强 Inspector 体验的开发者。
- **代码规范倡导者**：希望团队遵循统一的代码风格与注释标准，提升项目可维护性。

## 安装说明

### 通过 Git URL 安装

1. 打开 Unity Package Manager 窗口。
2. 点击左上角的 `+` 按钮，选择 `Add package from git URL...`。
3. 输入以下地址：
   ```
   https://github.com/yuumixcode/AesirInspector.git?path=Assets/Runestone/AesirInspector
   ```

### 通过 manifest.json 安装

在项目的 `Packages/manifest.json` 文件中添加：

```json
{
  "dependencies": {
    "cn.runestone.aesir-inspector": "https://github.com/yuumixcode/AesirInspector.git?path=Assets/Runestone/AesirInspector"
  }
}
```

### 安装方式检测

Aesir Inspector 会在编辑器加载时自动检测安装方式（UPM / Assets 目录），并通过 `AesirInspectorInstallationChecker` 暴露静态属性：

- `InstallMode`：当前安装方式（`Upm` / `AssetFolder` / `Unknown`）。
- `IsUpm`：是否通过 UPM 安装。
- `IsAssetFolder`：是否安装在 Assets 目录中（Asset Store 导入或 Git 子模块）。

## 环境依赖

- **Unity**: 2022.3 或更高版本。
- **Odin Inspector**: 3.3.x 或更高版本（**硬依赖**；未安装时本包无法编译）。

## 核心功能

### 1. 特性总览 (Attribute Overview Ultra)

以可搜索的树形菜单展示所有已注册的 Odin Inspector 与 Aesir Inspector 特性面板，每个特性提供实时预览与示例代码。

- **分类浏览**：按 Essentials / Buttons / Collections / Groups / Conditionals 等分类浏览特性。
- **搜索定位**：支持模糊搜索，快速找到目标特性。
- **实时预览**：选中特性即可在右侧面板查看效果与参数配置。
- **代码预览**：选中特性即可查看对应的示例源代码，快速了解用法。
- **零资产污染**：面板与示例均为内存实例，用户调试状态经状态银行（UltraStateBank）持久化，Project 中不生成任何子资产。
- 通过 `Tools → Aesir → Inspector → Attribute Overview Ultra` 菜单打开。

### 2. 迷你工具集 (Mini Tools)

整合常用编辑器小工具，通过 `Tools → Aesir → Inspector → Mini Tools` 菜单打开统一窗口。

| 工具 | 说明 |
|------|------|
| **MenuItem Viewer** | 搜集并展示项目中所有 `[MenuItem]` 菜单项信息，支持按程序集过滤、搜索，便于规划菜单结构 |
| **Syntax Highlighter** | 基于 Odin 内置语法高亮处理器的可视化面板，输入源码即可测试高亮效果并输出富文本标记 |
| **Quick Create SO** | 在 Project 窗口右键 MonoScript 即可快速生成 ScriptableObject 资源文件，支持多选批量创建 |

### 3. 扩展包管理器 (Extension Package Manager)

快捷安装推荐的 Aesir 系列和其他常用开源 Unity Packages，基于 Git URL 方式。

- **一键安装/移除**：卡片式 UI 展示推荐包的安装状态，点击即可安装或移除。
- **自动检测**：打开窗口时自动检测已安装包的状态，安装/移除后实时刷新。
- 通过 `Tools → Aesir → Inspector → Extension Package Manager` 菜单打开。

## 基础设施

### 4. 双语 UI 特性 (Bilingual Attributes)

提供了一套完整的双语属性装饰器与 Inspector Control，支持在 Inspector 面板中同时显示中文和英文信息。主要面向以下场景：

- **编辑器工具开发**：当你在开发其他编辑器工具时，希望 Inspector 界面支持中英双语显示，让不同语言背景的用户都能直观理解各项参数与操作。
- **团队协作**：跨地区、跨语言的团队在共享项目时，双语显示可有效降低沟通成本，避免因语言差异导致的误操作。

可用装饰器与 Control：

- `[BilingualTitle]`
- `[BilingualButton]`
- `[BilingualInfoBox]`
- `[BilingualText]`
- `BilingualDisplayAsStringControl` 双语只读文本显示控件
- `BilingualHeaderControl` 双语头部信息控件
- `HorizontalSeparateControl` 水平分隔线控件

### 5. Odin 集成

Odin Inspector 为硬依赖，本包直接使用 Sirenix（Odin）API 提供全部增强能力：

- 双语特性、Inspector Control、Attribute Drawer 与 Processor 直接基于 Odin Attribute/Drawer 体系实现。
- 特性总览（Attribute Overview Ultra）与扩展包管理器基于 Odin MenuEditorWindow / EditorWindow 构建。
- 未安装 Odin Inspector 时本包无法编译，请先通过 [odininspector.com](https://odininspector.com/) 安装 Odin 3.3.x+。

### 6. 安全编辑器工具 (Safe Editor Utilities)

针对 Unity Editor API 进行了安全封装，确保编辑器专用代码在打包后自动剔除。

| 工具类 | 说明 |
|-------|------|
| `ScriptableObjectSafeEditorUtility` | 提供更可靠的 ScriptableObject 资产创建与管理 |
| `MonoScriptSafeEditorUtility` | 根据脚本名称查找、选择 MonoScript 资源 |
| `PathUtility` | 路径字符串工具：Unity 路径规范化、子路径提取、路径合并 |
| `PathSafeEditorUtility` | 确保 Assets 目录下文件夹存在的安全创建工具 |
| `HierarchySafeEditorUtility` | 获取 GameObject 在 Hierarchy 中的绝对路径 |
| `HierarchyUtility` | Transform 层级路径操作：完整路径、相对路径、深层子物体查找 |
| `ProjectSafeEditorUtility` | Ping 并选中项目中任意资源（支持文件夹路径） |
| `UrlUtility` | 便捷的 URL 打开与外部链接处理 |
| `ReflectionUtility` | 程序集与命名空间的反射操作工具 |
| `PredefinedAssemblyUtility` | 预定义程序集类型识别与接口实现类型查找 |
| `PlayerLoopUtility` | 自定义 Unity PlayerLoop：插入、移除子系统，打印 PlayerLoop 结构 |
| `RegexUtility` | 正则表达式工具：命名空间/类名规范化、邮箱/URL 校验 |
| `AesirInspectorDebug` | 统一日志输出（Info/Warning/Error，支持前缀），构建后自动剔除；可通过 `AesirInspectorDebugSettings` 配置 |

### 7. 代码风格与规范

本项目将代码风格视为与功能同等重要的组成部分。内置严格的代码编写标准与示例，确保团队协作中的代码一致性与可维护性：

- **风格指南**：详情请参阅 `Runtime/Unity/CodeStyle/AesirInspectorCodeStyle.cs`。
- **设计理念**：良好的代码风格不是可选项，而是项目质量的基石。所有贡献者均需遵循本规范。

## 使用示例

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

## 许可协议

本项目采用 MIT 协议开源。详情请参阅 [LICENSE.md](LICENSE.md)。

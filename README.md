# Aesir Inspector

[English](Assets/Runestone/AesirInspector/Documentation~/README_EN.md) | [![license](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE.md)
[![Version](https://img.shields.io/badge/version-0.14.0-blue.svg)](Assets/Runestone/AesirInspector/CHANGELOG.md)
[![Install via Git URL](https://img.shields.io/badge/UPM-Git%20URL-blueviolet.svg)](#通过-git-url-安装)

本仓库是一个 **Unity 工程项目仓库**，托管编辑器扩展包 **Aesir Inspector** —— 提供双语 Inspector 特性、脚本文档生成器、XML Summary 同步工具、安全编辑器工具集与扩展包管理器，基于 Odin Inspector（硬依赖）实现增强能力。

包本体位于 `Assets/Runestone/AesirInspector/`，是一个完整、自包含的 Unity Custom Package，因此本仓库同时支持三种使用方式：

| 使用方式 | 说明 |
|------|------|
| **直接开发** | 用 Unity Editor 打开仓库根目录即可开发与运行单元测试 |
| **Git URL 导入** | 任意 Unity 项目通过 UPM Git URL 安装本包（见下文） |
| **导出分发包** | 将包目录导出为 `.unitypackage`，或整目录复制分发 |

> ⚠️ **硬依赖 [Odin Inspector](https://odininspector.com/)**：需先安装 Odin 3.3.x+，未安装时本包无法编译。

## 仓库结构

```
AesirInspector/                        # 仓库根目录 = Unity 工程根目录
├── Assets/
│   └── Runestone/
│       └── AesirInspector/            # Aesir Inspector 自定义包
│           ├── Editor/                # 编辑器程序集（Runestone.AesirInspector.Editor）
│           ├── Runtime/               # 运行时程序集（Runestone.AesirInspector）
│           ├── Tests/                 # 单元测试（Editor / Runtime）
│           ├── Samples~/              # 示例（UPM Samples 标签页按需导入）
│           ├── Documentation~/        # 包文档（英文 README / CHANGELOG、开发者指南等）
│           └── package.json           # UPM 包描述（cn.runestone.aesir-inspector）
├── Packages/                          # 工程依赖清单（manifest.json）
├── Scripts/                           # 导出与发布脚本（export-package.sh）
├── ProjectSettings/                   # Unity 工程设置
├── LICENSE.md                         # 仓库级许可（MIT）
├── CONTRIBUTING.md                    # 贡献指南
└── README.md
```

## 通过 Git URL 安装

1. 打开 Unity Package Manager 窗口。
2. 点击左上角 `+` 按钮，选择 `Add package from git URL...`。
3. 输入以下地址：

   ```
   https://github.com/yuumixcode/AesirInspector.git?path=Assets/Runestone/AesirInspector
   ```

或在项目的 `Packages/manifest.json` 中添加：

```json
{
  "dependencies": {
    "cn.runestone.aesir-inspector": "https://github.com/yuumixcode/AesirInspector.git?path=Assets/Runestone/AesirInspector"
  }
}
```

`Samples~` 与 `Documentation~` 为 UPM 隐藏目录，不会随包导入；示例可通过 Package Manager 中该包的 **Samples** 标签页按需导入。

## 从源码开发

1. 克隆本仓库：

   ```bash
   git clone git@github.com:yuumixcode/AesirInspector.git
   ```

2. 用 Unity（2022.3.62f3c1 或更高版本）打开仓库根目录。
3. 包源码位于 `Assets/Runestone/AesirInspector/`；单元测试位于 `Tests/Editor/` 与 `Tests/Runtime/`，通过 Test Runner 运行。

## 导出 .unitypackage

每次版本更新后运行导出脚本，生成 `Builds/AesirInspector-<version>.unitypackage`（纯 .NET 实现，无需启动 Unity，与 CI 同一方案）：

```bash
./Scripts/export-package.sh
```

- 首次运行自动安装 .NET 8 SDK 到 `~/.dotnet`（用户目录，免 sudo），并缓存导出工具（固定 commit）
- 版本号取自包内 `package.json`
- 发布 Release：推送 `v*` 标签（如 `git tag v0.14.1 && git push origin v0.14.1`），GitHub Actions 自动导出并创建 GitHub Release（Release Notes 取自包内 CHANGELOG）

## 包文档

| 文档 | 位置 |
|------|------|
| 包说明（中文） | [Assets/Runestone/AesirInspector/README.md](Assets/Runestone/AesirInspector/README.md) |
| 包说明（英文） | [Assets/Runestone/AesirInspector/Documentation~/README_EN.md](Assets/Runestone/AesirInspector/Documentation~/README_EN.md) |
| 变更日志 | [Assets/Runestone/AesirInspector/CHANGELOG.md](Assets/Runestone/AesirInspector/CHANGELOG.md) |
| 开发者指南 | [Assets/Runestone/AesirInspector/Documentation~/development.md](Assets/Runestone/AesirInspector/Documentation~/development.md) |
| 贡献指南 | [CONTRIBUTING.md](CONTRIBUTING.md) |

## 环境依赖

- **Unity**：2022.3.62f3c1 或更高版本
- **Odin Inspector**：3.3.x 或更高版本（硬依赖）

## 参考链接

本仓库引用的第三方开源项目：

| 项目 | 作者 | 许可 | 用途 |
|------|------|------|------|
| [JakePineOdinTools](https://github.com/JakePineGames/JakePineOdinTools) | Jake Pine | MIT | Odin 自动 Tooltip 与源码文件分析工具（`SourceFileAnalyzerUtility`）的来源，集成时已简化 |
| [public-unity-package-exporter](https://github.com/Guardingpearsoftware/public-unity-package-exporter) | Guarding Pear Software | MIT | `.unitypackage` 导出工具，供 `Scripts/export-package.sh` 与 CI 使用 |
| [Unity-Improved-Timers](https://github.com/adammyhre/Unity-Improved-Timers) | Git-Amend | MIT（附加条款） | 扩展包管理器中提供安装的第三方扩展包 |

包内第三方组件的许可声明另见 [Third Party Notices.md](Assets/Runestone/AesirInspector/Third%20Party%20Notices.md)。

## 许可协议

本项目采用 MIT 协议开源，详情请参阅 [LICENSE.md](LICENSE.md)。

# Odin Inspector 反编译与包化工作区

本工作区把 `Assets/Plugins/Sirenix/` 下的 **Odin Inspector 4.0.2.4**（Sirenix）
完整反编译为可读源码，并重打包为规范的 Unity Package（UPM）。

> **授权提醒**：Odin Inspector 是 Sirenix 的商业付费资产。
> 本工作区产出仅供**已持有有效许可证**的用户在本机使用与学习，**不得再分发**。
> 详见 `02-Package/com.sirenix.odin-inspector/LICENSE.md`。

---

## 产出速览

| 路径 | 内容 |
|---|---|
| `01-Decompiled/` | **反编译源码**：7 程序集 / 1,537 文件 / 228,373 行（纯净未改动） |
| `01-Patched/` | 应用最小修补后的源码副本（供编译验证使用） |
| `02-Package/com.sirenix.odin-inspector/` | **成品 UPM 包**（DLL 保真型，可直接用） |
| `02-BuildTest/` | 源码编译验证产物（`bin/` 重编译结果、`publicized/` 公开化参照） |
| `00-OriginalAssemblies/` | 原厂程序集副本（含 `.pdb` / `.xml`） |
| `docs/01-Odin架构与程序集分析.md` | 架构、依赖图、平台变体机制、硬编码约束 |
| `docs/02-包化方案与风险清单.md` | 包结构、asmdef 划分、编译实测、风险清单 |
| `tools/` | 全流程可复现脚本 |
| `report/` | 各步骤日志与统计 |

---

## 快速使用

```bash
# 一键复现全流程
tools/01-decompile.sh              # ① 反编译 7 个程序集
tools/02a-publicize-unity.sh       # ② 生成 Unity 公开化参照（源码编译需要）
tools/02-build-test.sh             # ③ 源码编译可行性验证
tools/03a-build-dll-package.sh     # ④ 构建成品 UPM 包
tools/04-verify-package-import.sh  # ⑤ Unity 批处理实测导入（在 /tmp 建临时工程）
```

**安装成品包**（必须作为嵌入式包）：

```bash
mv Assets/Plugins/Sirenix ~/Sirenix_backup          # 先移除原安装，避免重复程序集
cp -R OdinInspectorReverse/02-Package/com.sirenix.odin-inspector <工程>/Packages/
```

---

## 四个最关键的发现

### 1. Odin 把两个程序集编译了三份

`Sirenix.Utilities` 与 `Sirenix.Serialization` **引用了 `UnityEditor`**，
因此 Sirenix 把它们编译成 3 份同名程序集，交给 `PluginImporter` 按平台切换：

| 变体 | 目标平台 | 特征 |
|---|---|---|
| `Assemblies/` | 仅 Editor | 依赖 UnityEditor |
| `Assemblies/NoEditor/` | iOS/tvOS/PS4/XboxOne/WebGL/WSA 等 13 个 | 去 UnityEditor |
| `Assemblies/NoEmitAndNoEditor/` | Android/Linux/OSX 等 9 个 | 去 UnityEditor + 去 `Reflection.Emit` |

**这是"转源码包"的架构级障碍**：反编译只能还原**一个平台**的编译结果，
`#if` 预处理边界在 IL 中已完全消失，无法自动还原成"一份源码 + 条件编译"。

### 2. 一条 GUID 链锁死了 `.meta`

```
OdinPathLookup.asset
  └─ m_Script.guid = a4865f1ab4504ed8a368670db22f409c
                       │
      ┌────────────────┴────────────────┐
      ▼                                 ▼
Sirenix.OdinInspector.Editor.dll    该程序集的
  的 .meta guid          ==         [assembly: Guid]
```

三者完全一致。任何重编译或重新导入若改变 `.meta` GUID，Odin 立刻无法初始化。

### 3. 路径靠"GUID 反查 + 字符串倒推"自举

`SirenixAssetPaths` 没有假设安装位置，而是按 GUID 找到 `OdinPathLookup.asset`，
再从路径里找最后一个 `Sirenix/` 或 `/Odin Inspector/` 分段倒推出安装根目录，
**并校验该目录在磁盘上真实存在**。

由此得出包化的四条铁律：包内必须保留 `Odin Inspector/` 与 `Assemblies/` 两个目录名、
必须走嵌入式包（`Packages/` 下物理存在）、`OdinPathLookup.asset` 与 `.meta` 必须同在。

### 4. 成品包已通过 Unity 实测 ✅

在 `/tmp` 建独立临时工程，把成品包装进 `Packages/`，用 Unity 批处理模式实际导入，
并加探针主动触发 `SirenixAssetPaths` 静态构造。实测输出：

```
[ODIN-PROBE] SirenixPluginPath    = Packages/com.sirenix.odin-inspector/
[ODIN-PROBE] OdinPath             = Packages/com.sirenix.odin-inspector/Odin Inspector/
[ODIN-PROBE] SirenixAssembliesPath= Packages/com.sirenix.odin-inspector/Assemblies/
[ODIN-PROBE] OdinEditorConfigsPath= Packages/com.sirenix.odin-inspector/Odin Inspector/Config/Editor/
[ODIN-PROBE] 已加载的 Sirenix 程序集 (7): Sirenix.OdinInspector.Attributes,
             Sirenix.OdinInspector.Editor, Sirenix.Reflection.Editor,
             Sirenix.Serialization, Sirenix.Serialization.Config,
             Sirenix.Utilities, Sirenix.Utilities.Editor
```

**0 编译错误、无重复程序集、路径自举成功、7 个程序集全部加载。**

### 5. 源码编译可行，但不等价

搭建 Roslyn 编译验证台，按拓扑序重建全部 7 个程序集，迭代收敛结果：

| 程序集 | 文件数 | 最终错误 |
|---|---|---|
| `Sirenix.Utilities` | 52 | **0** ✅ |
| `Sirenix.OdinInspector.Attributes` | 168 | **0** ✅ |
| `Sirenix.Reflection.Editor` | 34 | **0** ✅ |
| `Sirenix.Serialization.Config` | 8 | **0** ✅ |
| `Sirenix.Serialization` | 212 | **0** ✅ |
| `Sirenix.Utilities.Editor` | 99 | **0** ✅ |
| `Sirenix.OdinInspector.Editor` | 964 | 15 ⚠️ |
| **合计** | **1,537** | **15** |

关键结论：

- ✅ **1,522 / 1,537 个文件（99.0%）可编译**，原厂反编译产物质量高度可信
- ✅ Odin 的真实编译基准是 **Unity 的 .NET Framework 4.8 API 档**，而非工程设置的 .NET Standard 2.1
- ✅ `Sirenix.Reflection.Editor` 用了 Unity 的 **internal 类型**
  （`GUILayoutEntry` / `GUILayoutGroup` / `ScrollViewState` / `UIElements.Panel`），
  必须用**公开化参照程序集**编译 —— 本工作区已用 Unity 自带的 `Unity.Cecil.dll` 实现了该工具
- ⚠️ 剩余 15 个错误是 ILSpy 产物缺陷（枚举字面量美化、丢失类型限定、LINQ `ref` 范围变量）
- ❌ **即使全部修完也无法等价替代原厂 DLL**：反编译只能还原 3 个平台变体中的**一个**，
  `#if UNITY_EDITOR` 边界在 IL 中已消失

**→ 因此推荐双轨**：DLL 保真包用于生产，反编译源码用于学习。

---

## 目录结构

```
OdinInspectorReverse/
├── README.md                        ← 本文件
├── 00-OriginalAssemblies/           原厂程序集 + pdb + xml
├── 01-Decompiled/                   反编译源码（纯净）
├── 01-Patched/                      最小修补后源码副本
├── 02-Package/
│   └── com.sirenix.odin-inspector/  成品 UPM 包
├── 02-BuildTest/
│   ├── bin/                         源码重编译输出的 DLL
│   ├── publicized/                  公开化的 Unity 参照程序集
│   └── publicize-build/             公开化工具
├── docs/
│   ├── 01-Odin架构与程序集分析.md
│   └── 02-包化方案与风险清单.md
├── report/                          日志与统计
└── tools/
    ├── 01-decompile.sh                 DLL → C# 反编译
    ├── 02a-publicize-unity.sh          Unity 参照程序集公开化
    ├── 02-build-test.sh                源码编译可行性验证（含最小修补）
    ├── 03a-build-dll-package.sh        构建 DLL 保真 UPM 包
    ├── 04-verify-package-import.sh     Unity 批处理导入实测
    └── publicize/PublicizeUnity.cs     公开化工具源码
```

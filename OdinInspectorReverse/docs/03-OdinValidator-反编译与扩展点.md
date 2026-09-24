# Odin Validator 反编译与分析（Odin 4.0.2.4 / Unity 2021.3.45f2c1）

> 由 `~/.workbuddy/skills/odin-inspector-decompile` 的五步流程产出。
> 技能沉淀在 `~/.codely-cli/skills/odin-validator/`（user 级）。

## 1. 侦察结论（`report/01-recon.txt`）

| 项 | 值 |
|---|---|
| 目标程序集 | `Assets/Plugins/Sirenix/Assemblies/Sirenix.OdinValidator.Editor.dll` |
| 大小 / 类型数 | 419,840 字节 / 291 类型 |
| 平台 | **仅 Editor**（PluginImporter：Editor enabled=1，其余 0） |
| GUID | `afbf832bc46149f5a291e87cab63e46d`（与 `AssemblyInfo` 内 `[assembly: Guid]` 一致） |
| 元数据 | 有 Portable PDB（无内嵌源码，可恢复局部变量名）、有强名称、未混淆 |
| 依赖 | 7 个 Sirenix 程序集全依赖；拓扑序**第 8 位**（最后） |
| Unity 依赖 | UnityEditor、CoreModule、IMGUIModule、JSONSerializeModule、TextRenderingModule、UIModule |

程序集总数由 7 → 8，拓扑序末尾追加 `Sirenix.OdinValidator.Editor`。

## 2. 反编译

```bash
ilspycmd <dll> -o 01-Decompiled/Sirenix.OdinValidator.Editor -p --nested-directories \
  --use-varnames-from-pdb -lv CSharp9_0 \
  -r <Unity>/Managed/UnityEngine -r <refs-dir-with-7-sirenix-dlls> --disable-updatecheck
```

坑：`ilspycmd -r` **只接受目录**，传 `.dll` 文件路径会报 "The directory ... does not exist"。
需先把 7 个依赖 DLL 软链到一个目录再传。

产物：**101 文件 / 20,319 行**。
质量扫描：`ISSUE:` 0、`ILSpy` 0、`<>c__DisplayClass` 0、`<Module>` 0；
8 处 `Invalid` 全为 `InvalidOperationException` / `InvalidLayerValidator`，良性。

## 3. 编译验证

| 参照集 | 错误数 | 说明 |
|---|---|---|
| 公开化 Unity + 源码重建的 OdinInspector.Editor（脚本默认） | 282 | 级联失败：`Sirenix.OdinInspector.Editor` 本身有 15 个历史 ILSpy 缺陷错误，未产出 DLL |
| 公开化 Unity + **原厂 7 个 Sirenix DLL** | 21 | 全部 `CS0115 Validate(ValidationResult) 找不到可重写方法` |
| **非公开化 Unity + 原厂 7 个 DLL** | **1** | 仅 `CS0122 FixIdentifier`（Odin internal，靠 `InternalsVisibleTo` 放行） |

**21 个 CS0115 是验证台伪错误**：`UnityEditor.CoreModule` 里存在**全局命名空间的内部 `ValidationResult`**，
公开化后与 `Sirenix.OdinInspector.Editor.Validation.ValidationResult` 撞车。
真实 Unity 编译下该类型不可见，不会冲突。→ 符合技能里"先怀疑验证台自身"的判断顺序。

日志：`report/build-validator-standalone.errors.txt`、`report/build-validator-nonpub.errors.txt`。

## 4. 架构要点

**两套程序集分工**（最关键的发现）：核心 API（`Validator`/`ValueValidator<T>`/`RootObjectValidator<T>`/
`AttributeValidator<,>`/`SceneValidator`/`GlobalValidator`/`ValidationResult`/`ResultItem`/`Fix`/
`DefaultValidatorLocator`）全在 **Odin Inspector** 的 `Sirenix.OdinInspector.Editor.dll` 里；
Validator 插件只有窗口、会话、工作队列、Profile、RuleConfig、自动化钩子、24 条内置规则与脚本模板。
跨程序集访问靠 `[assembly: InternalsVisibleTo("Sirenix.OdinValidator.Editor")]`。

详见 `~/.codely-cli/skills/odin-validator/references/architecture.md`。

## 5. 自定义规则的官方模板

`Sirenix/OdinValidator/Editor/ValidatorScriptTemplates.cs` 里有 11 个模板字符串，
对应 `Assets/Odin Validator/Create Rule|Create Validator/*` 九个菜单项。
关键设计：`{name}` / `{target}` 占位；`{target}` 默认由文件名去掉尾部 `Validator` 推得；
对值类型目标会切到 struct 版模板（`ExpressionUtility.TryParseTypeNameAsCSharpIdentifier(...).IsValueType`）。

模板全部以 `#if UNITY_EDITOR ... #endif` 包裹 —— 说明自定义验证器**必须**放在 Editor 程序集。

## 6. 已沉淀的技能

`~/.codely-cli/skills/odin-validator/`

- `SKILL.md` — 选型表（五类基类 + Rule/Standard 之别）、6 步工作流、8 条铁律
- `references/architecture.md` — 架构、验证管线、配置资产、编译验证结论
- `references/custom-validator-cookbook.md` — 五类写法、`ValidationResult`/`ResultItem` 全套 API、规则配置读写、常见坑
- `references/automation-and-ci.md` — 三类钩子、headless 判定、CI 两种做法、报告导出、26 个全局开关
- `assets/ValidatorTemplates.cs` — 7 个模板 + CI 入口，**已用 csc 对原厂程序集编译验证 0 错误**
  （验证副本在 `02-BuildTest/skill-templates/`）



## Codely Structured Memories

### User

### Feedback

### Project
- [2026-09-05 16:24:42] [project] AesirInspector assembly scheme (since 0.14.0, 2026-09-05): exactly two assemblies — Runtime `Runestone.AesirInspector` (namespace Runestone.AesirInspector) and Editor `Runestone.AesirInspector.Editor` (namespace Runestone.AesirInspector.Editor); code uses Sirenix APIs directly without #if guards, but ALL asmdefs (主两程序集、Tests、Samples 共 7 个) carry `"defineConstraints": ["ODIN_INSPECTOR"]` — 无 Odin 的项目导入包后程序集整体跳过编译、零报错（用户铁律，2026-09-05 修复于 commit 64ffd88）。**Why:** 0.14.0 merged the former Unity/OdinInspector split assemblies and briefly lost the constraints; pre-0.14 OdinInspector asmdefs were the proven pattern. ODIN_INSPECTOR 全局宏由 Odin 自身维护（写入激活平台的 ScriptingDefineSymbols）。**How to apply:** new code uses only these two namespaces/asmdefs; 新增 asmdef 必须带 ODIN_INSPECTOR 约束；pre-0.14 code needs the migration table in CHANGELOG 0.14.0.

- [2026-09-05 16:42:53] [project] AesirInspector 包导出方案（2026-09-05 统一）：本地与 CI 唯一方案 = .NET 工具 Guardingpearsoftware/public-unity-package-exporter（MIT，固定 commit 91d0bcb，按 SHA fetch）。本地用 Scripts/export-package.sh（首次运行自动装 .NET 8 到 ~/.dotnet 免 sudo，工具缓存在 ~/.cache/aesir-inspector，GitHub HTTPS 不通自动回退 SSH），CI 用 .github/workflows/export-package.yml（dispatch=Artifact，v* 标签=Release，Release Notes 优先取包内 CHANGELOG 版本段落）。Unity 导出脚本 AesirInspectorPackageExporter.cs 已删除（commit 2ff6aab）。**Why:** Unity batchmode 需 Tuanjie 许可证激活且慢；已实测 .NET 工具与 Unity 导出逐字节一致（原 430 条目，删导出脚本后 429）。**How to apply:** 修改包内容无需改脚本/工作流；上游 main 前进致旧 SHA 不可 fetch 时更新两处 TOOL_COMMIT；排除项（Library/**、**/.*、**/Samples~/**、**/Documentation~/**）必须保留——隐藏 ~ 目录无 .meta，工具对缺 .meta 回退逻辑有 bug（写全零 GUID 随机条目）；本地发版 = bump package.json 版本 + 推 v* 标签。

- [2026-09-05 16:44:15] [project] Codely Bridge 与 TJGenerators 已入库（2026-09-05，commit "chore(codely): 内嵌 Codely Bridge 包…"）：Bridge 以 embedded UPM 包形式放在 Packages/cn.tuanjie.codely.bridge（manifest 为普通版本条目，lock 为 source:embedded；embedded 解析优先级高于 manifest file: 引用，CLI 升级时改写 manifest 也不影响）；.codely.packages/ 仍是 CLI 缓存、被忽略。.gitignore 对 .codely-cli/ 采用白名单：默认忽略 .codely-cli/**，仅跟踪 extensions/TJGenerators/（用户拍板整个扩展原样入库，约 115MB，含未安装进工程的 cn.tuanjie.ai.generators Unity 包副本）；.codely-cli/settings.json 不跟踪——用户本机配置可能含个人 API Key（commit 745897b 停止跟踪，克隆机的 TJGenerators MCP 配置由扩展重装流程重建）；UnityInsight 索引、tool-outputs 日志、TMPChineseFont 扩展继续忽略。**Why:** 用户要求 fresh clone 与本地一模一样、Bridge 开箱即用，且 TJGenerators 扩展与 Bridge 强绑定。**How to apply:** 不要把 Packages/cn.tuanjie.codely.bridge、extensions/TJGenerators 当成本地杂物清理或忽略；settings.json 保持忽略、绝不提交（可能含 API Key）；新增 Codely 噪音目录默认已被 .codely-cli/** 覆盖，无需加规则。


### Reference


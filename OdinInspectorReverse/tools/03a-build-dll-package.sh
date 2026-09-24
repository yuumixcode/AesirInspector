#!/bin/bash
# =============================================================================
# Odin Inspector 反编译流水线 - Step 3a: 构建「DLL 保真型」UPM 包
# -----------------------------------------------------------------------------
# 设计约束（来自对反编译源码的分析，见 docs/）：
#   1. SirenixAssetPaths 静态构造会反查 OdinPathLookup.asset 的 GUID
#      (08379ccefc05200459f90a1c0711a340)，再从路径里找 "Sirenix/" 或
#      "/Odin Inspector/" 分段倒推 SirenixPluginPath。
#      => 包内必须保留名为 "Odin Inspector" 的目录，否则路径推导失败。
#   2. SirenixAssembliesPath = SirenixPluginPath + "Assemblies/"
#      => DLL 必须留在名为 "Assemblies" 的目录下。
#   3. OdinPathLookup.asset 以 {guid: a4865f1ab4504ed8a368670db22f409c} 绑定
#      Sirenix.OdinInspector.Editor.dll 的 .meta GUID
#      => 所有 .meta 必须逐字节保留，绝不能重新生成 GUID。
#   4. Runtime/Editor 变体（NoEditor / NoEmitAndNoEditor）靠 PluginImporter
#      按平台切换，同目录同名 DLL 共存 => 目录结构不可"整理"
# =============================================================================
set -o pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
SRC="/Users/yuumix/Projects/Unity/OdinInspectorSources/Assets/Plugins/Sirenix"
PKG="$ROOT/02-Package/com.sirenix.odin-inspector"

echo "源: $SRC"
echo "目标: $PKG"
rm -rf "$PKG"
mkdir -p "$PKG"

# --- 1. 原样搬运全部资产（含 .meta，GUID 与平台设置一并保留）-----------------
# rsync 保留目录结构；-a 保权限/时间，确保 .meta 与二进制逐字节一致
rsync -a "$SRC"/ "$PKG"/

echo "搬运完成，条目数: $(find "$PKG" -type f | wc -l | tr -d ' ')"

# --- 2. 生成 UPM 元数据 -----------------------------------------------------
cat > "$PKG/package.json" <<'JSON'
{
  "name": "com.sirenix.odin-inspector",
  "version": "4.0.2",
  "displayName": "Odin Inspector",
  "description": "Odin Inspector & Serializer (Sirenix) v4.0.2.4, repackaged as a UPM package from the original precompiled assemblies. Internal layout (Assemblies/ + Odin Inspector/) is preserved verbatim because SirenixAssetPaths derives its paths by GUID lookup, and the PluginImporter platform variants (NoEditor / NoEmitAndNoEditor) depend on the original .meta files.",
  "unity": "2021.3",
  "keywords": [
    "inspector",
    "editor",
    "serialization",
    "odin",
    "sirenix",
    "drawer"
  ],
  "author": {
    "name": "Sirenix",
    "url": "https://odininspector.com"
  },
  "documentationUrl": "https://odininspector.com/documentation",
  "changelogUrl": "https://odininspector.com/patch-notes",
  "licensesUrl": "https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041"
}
JSON

cat > "$PKG/README.md" <<'MD'
# Odin Inspector (UPM Repackaged)

**来源版本**：Odin Inspector 4.0.2.4（Sirenix）
**打包方式**：原厂预编译程序集 + 资产载荷，按 UPM 规范重排，`.meta` 逐字节保真。

## 为什么包内目录名是 `Assemblies/` 和 `Odin Inspector/`

这不是随意命名，而是**硬性约束**。`Sirenix.Utilities.SirenixAssetPaths` 的静态构造函数
按以下顺序解析安装路径：

1. 尝试 `Assets/Plugins/Sirenix/Odin Inspector/Assets/Editor/OdinPathLookup.asset`；
2. 失败则用 `AssetDatabase.GUIDToAssetPath("08379ccefc05200459f90a1c0711a340")`
   反查 `OdinPathLookup.asset` 的真实路径；
3. 再从该路径中找**最后一个** `Sirenix/` 或 `/Odin Inspector/` 分段，
   向前截断得到 `SirenixPluginPath`，并要求该目录在磁盘上真实存在；
4. 最后拼出 `OdinPath = SirenixPluginPath + "Odin Inspector/"`、
   `SirenixAssembliesPath = SirenixPluginPath + "Assemblies/"`。

因此：包根目录下**必须**保留 `Odin Inspector/` 与 `Assemblies/` 两个目录名，
否则 Odin 会回退到 `Assets/Plugins/Sirenix/` 并抛出路径错误。

## 安装

本包为**嵌入式包（embedded package）**，必须物理落在工程的 `Packages/` 目录下：

```bash
# 1. 备份现有安装（可选）
mv Assets/Plugins/Sirenix ~/Sirenix_backup

# 2. 放入 Packages/
cp -R <本包目录> <Unity工程>/Packages/com.sirenix.odin-inspector
```

或写入 `Packages/manifest.json`：

```json
{
  "dependencies": {
    "com.sirenix.odin-inspector": "file:../path/to/com.sirenix.odin-inspector"
  }
}
```

> **注意**：`file:` 引用与嵌入式包都会把包放在工程的 `Packages/` 或工程内路径，
> `DirectoryInfo` 才校验通过。若改用 `Library/PackageCache` 解析的远程包
> （git / registry），`DirectoryInfo("Packages/com.sirenix.odin-inspector")`
> 不存在，Odin 的路径推导会失败并回退到 `Assets/Plugins/Sirenix/`。

## 许可证

Odin Inspector 是 Sirenix 的商业付费资产。本包仅供**已持有有效许可证**的用户
在本机使用/研究，**不得再分发**。详见 `LICENSE.md`。
MD

cat > "$PKG/CHANGELOG.md" <<'MD'
# Changelog

## [4.0.2] - UPM Repackaging

### Changed
- 从 `Assets/Plugins/Sirenix/` 重排为 UPM 包结构。
- 新增 `package.json` / `README.md` / `CHANGELOG.md` / `LICENSE.md`。

### Preserved
- `Assemblies/` 与 `Odin Inspector/` 目录名（`SirenixAssetPaths` 路径推导依赖）。
- 全部 `.meta` 文件逐字节保留（GUID 与 PluginImporter 平台矩阵）。
- 程序集强名称/版本号（全部 1.0.0.0，无签名）。

### Upstream
- 对应 Odin Inspector **4.0.2.4** 原厂发行版。
MD

cat > "$PKG/LICENSE.md" <<'MD'
# 许可证与使用限制

## Odin Inspector（Sirenix）

本包内容为 **Sirenix 的商业付费资产 Odin Inspector 4.0.2.4** 的重打包形式。

- 版权归 **Sirenix ApS** 所有。
- 使用须遵守 Unity Asset Store EULA 与 Sirenix 的授权条款。
- **仅限已购买有效许可证的用户在本机安装使用。**
- **禁止再分发**：不得公开上传、转售、或以任何形式向未获授权者提供本包。

购买与授权信息：<https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041>

## Bootstrap Icons

Odin 内嵌了 Bootstrap 图标库（SDF 形式打包在 `SdfIconAtlas.png`）。

```
The MIT License (MIT)
Copyright (c) 2011-2018 Twitter, Inc.
Copyright (c) 2011-2018 The Bootstrap Authors
```

完整 MIT 文本见 `Odin Inspector/Assets/Editor/Bootstrap License.txt`。
MD

echo
echo "=== 生成结果 ==="
find "$PKG" -maxdepth 2 -type d | sort | sed "s|$PKG|.|"
echo
echo "顶层文件:"
ls -1 "$PKG"
echo
echo "总文件数: $(find "$PKG" -type f | wc -l | tr -d ' ')"

# --- 3. 校验 .meta GUID 是否逐字节保留 -------------------------------------
echo
echo "=== GUID 保真校验（原厂 vs 包内）==="
fail=0
for f in Assemblies/Sirenix.OdinInspector.Editor.dll.meta \
         "Odin Inspector/Assets/Editor/OdinPathLookup.asset.meta" \
         Assemblies/NoEditor/Sirenix.Utilities.dll.meta; do
  if [ -f "$SRC/$f" ] && [ -f "$PKG/$f" ]; then
    if diff -q "$SRC/$f" "$PKG/$f" >/dev/null; then
      printf "  [一致] %s\n" "$f"
    else
      printf "  [差异] %s\n" "$f"; fail=1
    fi
  else
    printf "  [缺失] %s\n" "$f"; fail=1
  fi
done
[ "$fail" -eq 0 ] && echo "  => 全部 .meta 逐字节一致 ✔" || echo "  => 存在不一致 ✘"

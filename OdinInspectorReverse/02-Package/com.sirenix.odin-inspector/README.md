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

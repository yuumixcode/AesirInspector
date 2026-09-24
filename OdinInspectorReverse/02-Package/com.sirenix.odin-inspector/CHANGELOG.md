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

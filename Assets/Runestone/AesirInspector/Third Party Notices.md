# Third Party Notices

本包参考了以下第三方项目，部分功能的设计与信息呈现方式来源于它们，特此署名。
The following third-party projects are referenced by this package, or have informed the design
of parts of it; attribution is listed below.

## Odin-Resolved-Parameters-Overview

- **项目 / Project**: https://github.com/schwapo/Odin-Resolved-Parameters-Overview
- **作者 / Author**: schwapo
- **许可 / License**: MIT

**用途 / Usage**：`Attribute Overview Pro` 的「参数（Parameters）」与「解析字符串参数（Resolved String
Parameters）」表格在信息组织与呈现方式上参考了该项目——它把每个 Odin 特性参数连同所属特性、说明、
解析器类型、解析结果、回退值、具名值信息与示例代码/预览列成一张总览表。本包在此基础上按 Odin 属性树
与包内语言切换机制自行实现，未直接分发该项目的代码。
`Attribute Overview Pro`'s parameter / resolved-string-parameter tables take their information
layout from that project, which lists every Odin attribute parameter together with its owning
attribute, description, resolver type, resolved value, fallback value, named-value information
and example code/preview. The implementation in this package is its own, built on Odin's
property tree and the package's language switching; no code from that project is distributed here.

## Historic attribution note

The JakePineOdinTools-derived `SourceFileAnalyzerUtility.cs` has moved out of this package
together with the Script Doc Generator tool (now maintained in the Aesir Modules package);
see that tool's own Third Party Notices for its attribution.

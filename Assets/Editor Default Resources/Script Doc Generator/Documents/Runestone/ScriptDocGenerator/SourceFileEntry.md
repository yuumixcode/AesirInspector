---
title: SourceFileEntry
description: "Runestone.ScriptDocGenerator.SourceFileEntry 的 API 文档"
---

# `SourceFileEntry`

<div class="api-meta" markdown="1">

- **种类:** `class`
- **命名空间:** `Runestone.ScriptDocGenerator`
- **程序集:** `Runestone.ScriptDocGenerator`

</div>

**继承链:** `System.Object` → `SourceFileEntry`

## 语法

``` csharp
[Serializable]
public class SourceFileEntry
```

源代码文件路径与内容的绑定容器。

## 构造方法

| 名称 | 描述 |
| :--- | :--- |
| [`SourceFileEntry(string, string[])`](#constructor-sourcefileentry-string-string) | — |
{: .api-summary-table }

### SourceFileEntry(string, string[]) {#constructor-sourcefileentry-string-string}

``` csharp
public SourceFileEntry(string filePath, string[] sourceLines)
```

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `filePath` | `string` |
| `sourceLines` | `string[]` |
{: .api-params-table }

## 字段

| 名称 | 描述 |
| :--- | :--- |
| [`filePath`](#field-filepath) | 相对路径（Assets/ 开头）。 |
| [`sourceLines`](#field-sourcelines) | 按行分割的源代码内容。 |
{: .api-summary-table }

### filePath {#field-filepath}

``` csharp
public string filePath;
```

相对路径（Assets/ 开头）。

### sourceLines {#field-sourcelines}

``` csharp
public string[] sourceLines;
```

按行分割的源代码内容。

## 方法

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |
{: .api-summary-table }

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。

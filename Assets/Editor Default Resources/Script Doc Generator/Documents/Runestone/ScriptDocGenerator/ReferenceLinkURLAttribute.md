---
title: ReferenceLinkURLAttribute
description: "Runestone.ScriptDocGenerator.ReferenceLinkURLAttribute 的 API 文档"
---

# `ReferenceLinkURLAttribute`

<div class="api-meta" markdown="1">

- **种类:** `class`
- **命名空间:** `Runestone.ScriptDocGenerator`
- **程序集:** `Runestone.ScriptDocGenerator`

</div>

**继承链:** `System.Object` → `System.Attribute` → `ReferenceLinkURLAttribute`

**实现接口:** `System.Runtime.InteropServices._Attribute`

## 语法

``` csharp
[AttributeUsage]
public class ReferenceLinkURLAttribute : System.Attribute, 
System.Runtime.InteropServices._Attribute
```

## 构造方法

| 名称 | 描述 |
| :--- | :--- |
| [`ReferenceLinkURLAttribute(string)`](#constructor-referencelinkurlattribute-string) | — |
{: .api-summary-table }

### ReferenceLinkURLAttribute(string) {#constructor-referencelinkurlattribute-string}

``` csharp
public ReferenceLinkURLAttribute(string webUrl)
```

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `webUrl` | `string` |
{: .api-params-table }

## 字段

| 名称 | 描述 |
| :--- | :--- |
| [`WebUrl`](#field-weburl) | — |
{: .api-summary-table }

### WebUrl {#field-weburl}

``` csharp
public readonly string WebUrl;
```

## 属性

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `TypeId` | — | `Attribute` |
{: .api-summary-table }

## 方法

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Equals(object)` | — | `Attribute` |
| `GetHashCode()` | — | `Attribute` |
| `IsDefaultAttribute()` | — | `Attribute` |
| `Match(object)` | — | `Attribute` |
| `ToString()` | — | `object` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |
{: .api-summary-table }

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。

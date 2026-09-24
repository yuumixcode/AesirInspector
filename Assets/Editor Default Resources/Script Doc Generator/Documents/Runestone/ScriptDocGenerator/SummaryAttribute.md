---
title: SummaryAttribute
description: "Runestone.ScriptDocGenerator.SummaryAttribute 的 API 文档"
---

# `SummaryAttribute`

<div class="api-meta" markdown="1">

- **种类:** `class`
- **命名空间:** `Runestone.ScriptDocGenerator`
- **程序集:** `Runestone.ScriptDocGenerator`

</div>

**继承链:** `System.Object` → `System.Attribute` → `SummaryAttribute`

**实现接口:** `System.Runtime.InteropServices._Attribute`

## 语法

``` csharp
[AttributeUsage]
public class SummaryAttribute : System.Attribute, 
System.Runtime.InteropServices._Attribute
```

提供类似于 XML 文档 summary 部分的描述性元数据。

## 构造方法

| 名称 | 描述 |
| :--- | :--- |
| [`SummaryAttribute(string)`](#constructor-summaryattribute-string) | — |
{: .api-summary-table }

### SummaryAttribute(string) {#constructor-summaryattribute-string}

``` csharp
public SummaryAttribute(string summaryText)
```

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `summaryText` | `string` |
{: .api-params-table }

## 属性

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `TypeId` | — | `Attribute` |
{: .api-summary-table }

## 方法

**声明的方法**

| 名称 | 描述 |
| :--- | :--- |
| [`GetSummary()`](#method-getsummary) | — |
{: .api-summary-table }

**继承的方法**

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

### GetSummary() {#method-getsummary}

``` csharp
public string GetSummary()
```

**返回值**

| 类型 |
| :--- |
| `string` |
{: .api-returns-table }

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。

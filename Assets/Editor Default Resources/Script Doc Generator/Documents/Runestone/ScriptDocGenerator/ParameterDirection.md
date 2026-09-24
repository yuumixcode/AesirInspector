---
title: ParameterDirection
description: "Runestone.ScriptDocGenerator.ParameterDirection 的 API 文档"
---

# `ParameterDirection`

<div class="api-meta" markdown="1">

- **种类:** `enum`
- **命名空间:** `Runestone.ScriptDocGenerator`
- **程序集:** `Runestone.ScriptDocGenerator`

</div>

**继承链:** `System.Object` → `System.ValueType` → `System.Enum` → `ParameterDirection`

**实现接口:** `System.IFormattable`，`System.IComparable`，`System.IConvertible`

## 语法

``` csharp
public enum ParameterDirection : System.Enum, 
System.IFormattable, 
System.IComparable, 
System.IConvertible
```

参数方向枚举

## 字段

| 名称 | 描述 |
| :--- | :--- |
| [`In`](#field-in) | 输入参数 |
| [`Out`](#field-out) | 输出参数 |
| [`Ref`](#field-ref) | 引用参数 |
| [`RetVal`](#field-retval) | 返回值参数 |
{: .api-summary-table }

### In {#field-in}

``` csharp
public const ParameterDirection In;
```

输入参数

### Out {#field-out}

``` csharp
public const ParameterDirection Out;
```

输出参数

### Ref {#field-ref}

``` csharp
public const ParameterDirection Ref;
```

引用参数

### RetVal {#field-retval}

``` csharp
public const ParameterDirection RetVal;
```

返回值参数

## 方法

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `HasFlag(Enum)` | — | `Enum` |
| `Equals(object)` | — | `Enum` |
| `GetHashCode()` | — | `Enum` |
| `ToString()` | — | `Enum` |
| `ToString(string)` | — | `Enum` |
| `GetTypeCode()` | — | `Enum` |
| `CompareTo(object)` | — | `Enum` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |
| `ToString(IFormatProvider)` | — | `Enum` |
| `ToString(string, IFormatProvider)` | — | `Enum` |
{: .api-summary-table }

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。

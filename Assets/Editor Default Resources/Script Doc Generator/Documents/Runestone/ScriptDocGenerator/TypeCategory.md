---
title: TypeCategory
description: "Runestone.ScriptDocGenerator.TypeCategory 的 API 文档"
---

# `TypeCategory`

<div class="api-meta" markdown="1">

- **种类:** `enum`
- **命名空间:** `Runestone.ScriptDocGenerator`
- **程序集:** `Runestone.ScriptDocGenerator`

</div>

**继承链:** `System.Object` → `System.ValueType` → `System.Enum` → `TypeCategory`

**实现接口:** `System.IFormattable`，`System.IComparable`，`System.IConvertible`

## 语法

``` csharp
public enum TypeCategory : System.Enum, 
System.IFormattable, 
System.IComparable, 
System.IConvertible
```

类型种类枚举

## 字段

| 名称 | 描述 |
| :--- | :--- |
| [`Class`](#field-class) | 类 |
| [`Delegate`](#field-delegate) | 委托 |
| [`Enum`](#field-enum) | 枚举 |
| [`Interface`](#field-interface) | 接口 |
| [`Record`](#field-record) | 记录类型 |
| [`Struct`](#field-struct) | 结构体 |
| [`Unknown`](#field-unknown) | — |
{: .api-summary-table }

### Class {#field-class}

``` csharp
public const TypeCategory Class;
```

类

### Delegate {#field-delegate}

``` csharp
public const TypeCategory Delegate;
```

委托

### Enum {#field-enum}

``` csharp
public const TypeCategory Enum;
```

枚举

### Interface {#field-interface}

``` csharp
public const TypeCategory Interface;
```

接口

### Record {#field-record}

``` csharp
public const TypeCategory Record;
```

记录类型

### Struct {#field-struct}

``` csharp
public const TypeCategory Struct;
```

结构体

### Unknown {#field-unknown}

``` csharp
public const TypeCategory Unknown;
```

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

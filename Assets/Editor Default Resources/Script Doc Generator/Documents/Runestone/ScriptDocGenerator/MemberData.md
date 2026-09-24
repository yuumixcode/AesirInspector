---
title: MemberData
description: "Runestone.ScriptDocGenerator.MemberData 的 API 文档"
---

# `MemberData`

<div class="api-meta" markdown="1">

- **种类:** `abstract class`
- **命名空间:** `Runestone.ScriptDocGenerator`
- **程序集:** `Runestone.ScriptDocGenerator`

</div>

**继承链:** `System.Object` → `MemberData`

**实现接口:** `Runestone.ScriptDocGenerator.IMemberData`

## 语法

``` csharp
[Serializable]
public abstract class MemberData : Runestone.ScriptDocGenerator.IMemberData
```

解析成员数据的基类

## 字段

| 名称 | 描述 |
| :--- | :--- |
| [`DefaultAttributeFilter`](#field-defaultattributefilter) | — |
{: .api-summary-table }

### DefaultAttributeFilter {#field-defaultattributefilter}

``` csharp
public static readonly DefaultAttributeFilter DefaultAttributeFilter;
```

## 属性

| 名称 | 描述 |
| :--- | :--- |
| [`DeclaringType`](#property-declaringtype) | 声明此成员的类型 |
| [`ReflectedType`](#property-reflectedtype) | 通过反射获取该成员的类型 |
| [`IsFromInheritance`](#property-isfrominheritance) | 成员是否从继承中获取，这里的成员不包括 Type 类型 |
| [`IsObsolete`](#property-isobsolete) | 是否已过时 |
| [`AttributesDeclaration`](#property-attributesdeclaration) | 特性声明字符串 |
| [`DeclaringTypeFullName`](#property-declaringtypefullname) | 声明类型的完整名称，包括命名空间 |
| [`DeclaringTypeName`](#property-declaringtypename) | 声明类型的名称 |
| [`Name`](#property-name) | 成员名称 |
| [`ReflectedTypeFullName`](#property-reflectedtypefullname) | 通过反射获取该成员的类型的完整名称，包括命名空间 |
| [`ReflectedTypeName`](#property-reflectedtypename) | 通过反射获取该成员的类型名称 |
| [`SummaryAttributeValue`](#property-summaryattributevalue) | 注释 |
| [`SummaryResolver`](#property-summaryresolver) | Summary 解析委托。Editor 程序集在加载时注入源文件解析实现（基于 SourceScanner）， 从源代码的 XML /// <summary> 注释中读取成员摘要。 默认回退到 [Summary] 特性，保持向后兼容。 |
| [`ParamSummariesResolver`](#property-paramsummariesresolver) | 参数级注释解析委托（XML <param> 标签），键为参数名。 Editor 程序集在加载时注入源文件解析实现；默认无参数级注释（返回 null）。 |
| [`ReturnsSummaryResolver`](#property-returnssummaryresolver) | 返回值注释解析委托（XML <returns> 标签）。 Editor 程序集在加载时注入源文件解析实现；默认无返回值注释（返回 null）。 |
{: .api-summary-table }

### DeclaringType {#property-declaringtype}

``` csharp
public Type DeclaringType { get; }
```

声明此成员的类型

### ReflectedType {#property-reflectedtype}

``` csharp
public Type ReflectedType { get; }
```

通过反射获取该成员的类型

### IsFromInheritance {#property-isfrominheritance}

``` csharp
public bool IsFromInheritance { get; }
```

成员是否从继承中获取，这里的成员不包括 Type 类型

### IsObsolete {#property-isobsolete}

``` csharp
public bool IsObsolete { get; }
```

是否已过时

### AttributesDeclaration {#property-attributesdeclaration}

``` csharp
public string AttributesDeclaration { get; }
```

特性声明字符串

### DeclaringTypeFullName {#property-declaringtypefullname}

``` csharp
public string DeclaringTypeFullName { get; }
```

声明类型的完整名称，包括命名空间

### DeclaringTypeName {#property-declaringtypename}

``` csharp
public string DeclaringTypeName { get; }
```

声明类型的名称

### Name {#property-name}

``` csharp
public string Name { get; }
```

成员名称

### ReflectedTypeFullName {#property-reflectedtypefullname}

``` csharp
public string ReflectedTypeFullName { get; }
```

通过反射获取该成员的类型的完整名称，包括命名空间

### ReflectedTypeName {#property-reflectedtypename}

``` csharp
public string ReflectedTypeName { get; }
```

通过反射获取该成员的类型名称

### SummaryAttributeValue {#property-summaryattributevalue}

``` csharp
public string SummaryAttributeValue { get; }
```

注释

### SummaryResolver {#property-summaryresolver}

``` csharp
public static Func<MemberInfo, string> SummaryResolver { get; set; }
```

Summary 解析委托。Editor 程序集在加载时注入源文件解析实现（基于 SourceScanner）， 从源代码的 XML /// <summary> 注释中读取成员摘要。 默认回退到 [Summary] 特性，保持向后兼容。

### ParamSummariesResolver {#property-paramsummariesresolver}

``` csharp
public static Func<MethodInfo, IReadOnlyDictionary<string, string>> ParamSummariesResolver { get; set; }
```

参数级注释解析委托（XML <param> 标签），键为参数名。 Editor 程序集在加载时注入源文件解析实现；默认无参数级注释（返回 null）。

### ReturnsSummaryResolver {#property-returnssummaryresolver}

``` csharp
public static Func<MethodInfo, string> ReturnsSummaryResolver { get; set; }
```

返回值注释解析委托（XML <returns> 标签）。 Editor 程序集在加载时注入源文件解析实现；默认无返回值注释（返回 null）。

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

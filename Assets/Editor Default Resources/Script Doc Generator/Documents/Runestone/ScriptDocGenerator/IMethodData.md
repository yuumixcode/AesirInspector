---
title: IMethodData
description: "Runestone.ScriptDocGenerator.IMethodData 的 API 文档"
---

# `IMethodData`

<div class="api-meta" markdown="1">

- **种类:** `interface`
- **命名空间:** `Runestone.ScriptDocGenerator`
- **程序集:** `Runestone.ScriptDocGenerator`

</div>

**实现接口:** `Runestone.ScriptDocGenerator.IDerivedMemberData`

## 语法

``` csharp
public interface IMethodData : Runestone.ScriptDocGenerator.IDerivedMemberData
```

方法数据接口，继承自 IDerivedMemberData

## 属性

| 名称 | 描述 |
| :--- | :--- |
| [`ParamSummaries`](#property-paramsummaries) | 参数级注释字典（XML <param> 标签），键为参数名。无参数注释时为 null |
| [`ReturnType`](#property-returntype) | — |
| [`IsAbstract`](#property-isabstract) | — |
| [`IsAsync`](#property-isasync) | — |
| [`IsFromAncestor`](#property-isfromancestor) | — |
| [`IsFromInterfaceImplement`](#property-isfrominterfaceimplement) | — |
| [`IsOperator`](#property-isoperator) | — |
| [`IsOverloadMethodInDeclaringType`](#property-isoverloadmethodindeclaringtype) | — |
| [`IsOverride`](#property-isoverride) | — |
| [`IsVirtual`](#property-isvirtual) | — |
| [`ParametersDeclaration`](#property-parametersdeclaration) | — |
| [`ReturnTypeFullName`](#property-returntypefullname) | — |
| [`ReturnTypeName`](#property-returntypename) | — |
| [`ReturnsSummary`](#property-returnssummary) | — |
| [`SignatureWithoutParameters`](#property-signaturewithoutparameters) | — |
{: .api-summary-table }

### ParamSummaries {#property-paramsummaries}

``` csharp
public IReadOnlyDictionary<string, string> ParamSummaries { get; }
```

参数级注释字典（XML <param> 标签），键为参数名。无参数注释时为 null

### ReturnType {#property-returntype}

``` csharp
public Type ReturnType { get; }
```

### IsAbstract {#property-isabstract}

``` csharp
public bool IsAbstract { get; }
```

### IsAsync {#property-isasync}

``` csharp
public bool IsAsync { get; }
```

### IsFromAncestor {#property-isfromancestor}

``` csharp
public bool IsFromAncestor { get; }
```

### IsFromInterfaceImplement {#property-isfrominterfaceimplement}

``` csharp
public bool IsFromInterfaceImplement { get; }
```

### IsOperator {#property-isoperator}

``` csharp
public bool IsOperator { get; }
```

### IsOverloadMethodInDeclaringType {#property-isoverloadmethodindeclaringtype}

``` csharp
public bool IsOverloadMethodInDeclaringType { get; set; }
```

### IsOverride {#property-isoverride}

``` csharp
public bool IsOverride { get; }
```

### IsVirtual {#property-isvirtual}

``` csharp
public bool IsVirtual { get; }
```

### ParametersDeclaration {#property-parametersdeclaration}

``` csharp
public string ParametersDeclaration { get; }
```

### ReturnTypeFullName {#property-returntypefullname}

``` csharp
public string ReturnTypeFullName { get; }
```

### ReturnTypeName {#property-returntypename}

``` csharp
public string ReturnTypeName { get; }
```

### ReturnsSummary {#property-returnssummary}

``` csharp
public string ReturnsSummary { get; }
```

### SignatureWithoutParameters {#property-signaturewithoutparameters}

``` csharp
public string SignatureWithoutParameters { get; }
```

## 方法

| 名称 | 描述 |
| :--- | :--- |
| [`AddOverloadPrefix()`](#method-addoverloadprefix) | 添加 [Overload] 前缀 |
{: .api-summary-table }

### AddOverloadPrefix() {#method-addoverloadprefix}

``` csharp
public abstract void AddOverloadPrefix()
```

添加 [Overload] 前缀

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。

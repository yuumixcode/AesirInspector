---
title: IParameterData
description: "Runestone.ScriptDocGenerator.IParameterData 的 API 文档"
---

# `IParameterData`

<div class="api-meta" markdown="1">

- **种类:** `interface`
- **命名空间:** `Runestone.ScriptDocGenerator`
- **程序集:** `Runestone.ScriptDocGenerator`

</div>

## 语法

``` csharp
public interface IParameterData
```

参数信息解析数据接口

## 属性

| 名称 | 描述 |
| :--- | :--- |
| [`Direction`](#property-direction) | — |
| [`ParameterType`](#property-parametertype) | — |
| [`HasDefaultValue`](#property-hasdefaultvalue) | — |
| [`IsParams`](#property-isparams) | — |
| [`DefaultValue`](#property-defaultvalue) | — |
| [`Name`](#property-name) | — |
{: .api-summary-table }

### Direction {#property-direction}

``` csharp
public ParameterDirection Direction { get; }
```

### ParameterType {#property-parametertype}

``` csharp
public Type ParameterType { get; }
```

### HasDefaultValue {#property-hasdefaultvalue}

``` csharp
public bool HasDefaultValue { get; }
```

### IsParams {#property-isparams}

``` csharp
public bool IsParams { get; }
```

### DefaultValue {#property-defaultvalue}

``` csharp
public object DefaultValue { get; }
```

### Name {#property-name}

``` csharp
public string Name { get; }
```

## 方法

| 名称 | 描述 |
| :--- | :--- |
| [`GetFormattedString()`](#method-getformattedstring) | 生成格式化的参数字符串 |
{: .api-summary-table }

### GetFormattedString() {#method-getformattedstring}

``` csharp
public abstract string GetFormattedString()
```

生成格式化的参数字符串

**返回值**

| 类型 |
| :--- |
| `string` |
{: .api-returns-table }

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。

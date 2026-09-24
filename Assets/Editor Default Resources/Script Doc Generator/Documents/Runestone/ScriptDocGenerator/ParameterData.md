---
title: ParameterData
description: "Runestone.ScriptDocGenerator.ParameterData 的 API 文档"
---

# `ParameterData`

<div class="api-meta" markdown="1">

- **种类:** `class`
- **命名空间:** `Runestone.ScriptDocGenerator`
- **程序集:** `Runestone.ScriptDocGenerator`

</div>

**继承链:** `System.Object` → `ParameterData`

**实现接口:** `Runestone.ScriptDocGenerator.IParameterData`

## 语法

``` csharp
[Serializable]
public class ParameterData : Runestone.ScriptDocGenerator.IParameterData
```

参数信息解析数据

## 构造方法

| 名称 | 描述 |
| :--- | :--- |
| [`ParameterData(ParameterInfo)`](#constructor-parameterdata-parameterinfo) | 创建参数信息解析数据实例 |
{: .api-summary-table }

### ParameterData(ParameterInfo) {#constructor-parameterdata-parameterinfo}

``` csharp
public ParameterData(ParameterInfo parameterInfo)
```

创建参数信息解析数据实例

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `parameterInfo` | `ParameterInfo` |
{: .api-params-table }

## 属性

| 名称 | 描述 |
| :--- | :--- |
| [`Direction`](#property-direction) | 参数方向（in/out/ref） |
| [`ParameterType`](#property-parametertype) | 参数类型 |
| [`HasDefaultValue`](#property-hasdefaultvalue) | 是否有默认值 |
| [`IsParams`](#property-isparams) | 是否为 params 参数 |
| [`DefaultValue`](#property-defaultvalue) | 默认值 |
| [`Name`](#property-name) | 参数名称 |
{: .api-summary-table }

### Direction {#property-direction}

``` csharp
public ParameterDirection Direction { get; }
```

参数方向（in/out/ref）

### ParameterType {#property-parametertype}

``` csharp
public Type ParameterType { get; }
```

参数类型

### HasDefaultValue {#property-hasdefaultvalue}

``` csharp
public bool HasDefaultValue { get; }
```

是否有默认值

### IsParams {#property-isparams}

``` csharp
public bool IsParams { get; }
```

是否为 params 参数

### DefaultValue {#property-defaultvalue}

``` csharp
public object DefaultValue { get; }
```

默认值

### Name {#property-name}

``` csharp
public string Name { get; }
```

参数名称

## 方法

**声明的方法**

| 名称 | 描述 |
| :--- | :--- |
| [`GetFormattedString()`](#method-getformattedstring) | 生成格式化的参数字符串 |
{: .api-summary-table }

**继承的方法**

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |
{: .api-summary-table }

### GetFormattedString() {#method-getformattedstring}

``` csharp
public string GetFormattedString()
```

生成格式化的参数字符串

**返回值**

| 类型 |
| :--- |
| `string` |
{: .api-returns-table }

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。

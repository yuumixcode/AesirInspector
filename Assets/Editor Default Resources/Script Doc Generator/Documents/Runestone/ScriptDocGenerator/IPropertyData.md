---
title: IPropertyData
description: "Runestone.ScriptDocGenerator.IPropertyData 的 API 文档"
---

# `IPropertyData`

<div class="api-meta" markdown="1">

- **种类:** `interface`
- **命名空间:** `Runestone.ScriptDocGenerator`
- **程序集:** `Runestone.ScriptDocGenerator`

</div>

**实现接口:** `Runestone.ScriptDocGenerator.IDerivedMemberData`

## 语法

``` csharp
public interface IPropertyData : Runestone.ScriptDocGenerator.IDerivedMemberData
```

属性数据接口，继承自 IDerivedMemberData，包含属性特有的数据信息和方法，派生类的通用数据信息和方法

## 属性

| 名称 | 描述 |
| :--- | :--- |
| [`PropertyType`](#property-propertytype) | — |
| [`DefaultValue`](#property-defaultvalue) | — |
| [`PropertyTypeFullName`](#property-propertytypefullname) | — |
| [`PropertyTypeName`](#property-propertytypename) | — |
{: .api-summary-table }

### PropertyType {#property-propertytype}

``` csharp
public Type PropertyType { get; }
```

### DefaultValue {#property-defaultvalue}

``` csharp
public object DefaultValue { get; }
```

### PropertyTypeFullName {#property-propertytypefullname}

``` csharp
public string PropertyTypeFullName { get; }
```

### PropertyTypeName {#property-propertytypename}

``` csharp
public string PropertyTypeName { get; }
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。

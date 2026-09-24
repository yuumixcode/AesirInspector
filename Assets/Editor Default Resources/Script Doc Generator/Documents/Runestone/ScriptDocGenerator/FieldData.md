---
title: FieldData
description: "Runestone.ScriptDocGenerator.FieldData 的 API 文档"
---

# `FieldData`

<div class="api-meta" markdown="1">

- **种类:** `class`
- **命名空间:** `Runestone.ScriptDocGenerator`
- **程序集:** `Runestone.ScriptDocGenerator`

</div>

**继承链:** `System.Object` → `Runestone.ScriptDocGenerator.MemberData` → `FieldData`

**实现接口:** `Runestone.ScriptDocGenerator.IFieldData`，`Runestone.ScriptDocGenerator.IDerivedMemberData`，`Runestone.ScriptDocGenerator.IMemberData`

## 语法

``` csharp
[Serializable]
public class FieldData : Runestone.ScriptDocGenerator.MemberData, 
Runestone.ScriptDocGenerator.IFieldData, 
Runestone.ScriptDocGenerator.IDerivedMemberData, 
Runestone.ScriptDocGenerator.IMemberData
```

字段解析数据类，用于存储字段的解析数据

## 构造方法

| 名称 | 描述 |
| :--- | :--- |
| [`FieldData(FieldInfo, IAttributeFilter)`](#constructor-fielddata-fieldinfo-iattributefilter) | — |
{: .api-summary-table }

### FieldData(FieldInfo, IAttributeFilter) {#constructor-fielddata-fieldinfo-iattributefilter}

``` csharp
public FieldData(FieldInfo fieldInfo, IAttributeFilter filter = null)
```

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `fieldInfo` | `FieldInfo` |
| `filter` | `IAttributeFilter` |
{: .api-params-table }

## 属性

**声明的属性**

| 名称 | 描述 |
| :--- | :--- |
| [`AccessModifier`](#property-accessmodifier) | 访问修饰符类型 |
| [`MemberType`](#property-membertype) | 成员类型，指示该成员的类型 |
| [`FieldType`](#property-fieldtype) | 字段的类型 |
| [`IsConstant`](#property-isconstant) | 是否为常量字段 |
| [`IsDynamic`](#property-isdynamic) | 是否为动态类型字段 |
| [`IsReadOnly`](#property-isreadonly) | 是否为只读字段 |
| [`IsStatic`](#property-isstatic) | 指示该字段是否为静态字段 |
| [`DefaultValue`](#property-defaultvalue) | 字段的默认值，没有默认值返回 null |
| [`AccessModifierName`](#property-accessmodifiername) | 访问修饰符名称 |
| [`FieldTypeFullName`](#property-fieldtypefullname) | 字段类型的完整名称 |
| [`FieldTypeName`](#property-fieldtypename) | 字段类型的名称 |
| [`FullDeclarationWithAttributes`](#property-fulldeclarationwithattributes) | 完整字段声明，包含特性和签名，默认剔除 Summary 特性 |
| [`MemberTypeName`](#property-membertypename) | 成员类型名称 |
| [`Signature`](#property-signature) | 字段签名 |
{: .api-summary-table }

**继承的属性**

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `DeclaringType` | 声明此成员的类型 | `MemberData` |
| `ReflectedType` | 通过反射获取该成员的类型 | `MemberData` |
| `IsFromInheritance` | 成员是否从继承中获取，这里的成员不包括 Type 类型 | `MemberData` |
| `IsObsolete` | 是否已过时 | `MemberData` |
| `AttributesDeclaration` | 特性声明字符串 | `MemberData` |
| `DeclaringTypeFullName` | 声明类型的完整名称，包括命名空间 | `MemberData` |
| `DeclaringTypeName` | 声明类型的名称 | `MemberData` |
| `Name` | 成员名称 | `MemberData` |
| `ReflectedTypeFullName` | 通过反射获取该成员的类型的完整名称，包括命名空间 | `MemberData` |
| `ReflectedTypeName` | 通过反射获取该成员的类型名称 | `MemberData` |
| `SummaryAttributeValue` | 注释 | `MemberData` |
{: .api-summary-table }

### AccessModifier {#property-accessmodifier}

``` csharp
public AccessModifierType AccessModifier { get; }
```

访问修饰符类型

### MemberType {#property-membertype}

``` csharp
public MemberTypes MemberType { get; }
```

成员类型，指示该成员的类型

### FieldType {#property-fieldtype}

``` csharp
public Type FieldType { get; }
```

字段的类型

### IsConstant {#property-isconstant}

``` csharp
public bool IsConstant { get; }
```

是否为常量字段

### IsDynamic {#property-isdynamic}

``` csharp
public bool IsDynamic { get; }
```

是否为动态类型字段

### IsReadOnly {#property-isreadonly}

``` csharp
public bool IsReadOnly { get; }
```

是否为只读字段

### IsStatic {#property-isstatic}

``` csharp
public bool IsStatic { get; }
```

指示该字段是否为静态字段

### DefaultValue {#property-defaultvalue}

``` csharp
public object DefaultValue { get; }
```

字段的默认值，没有默认值返回 null

### AccessModifierName {#property-accessmodifiername}

``` csharp
public string AccessModifierName { get; }
```

访问修饰符名称

### FieldTypeFullName {#property-fieldtypefullname}

``` csharp
public string FieldTypeFullName { get; }
```

字段类型的完整名称

### FieldTypeName {#property-fieldtypename}

``` csharp
public string FieldTypeName { get; }
```

字段类型的名称

### FullDeclarationWithAttributes {#property-fulldeclarationwithattributes}

``` csharp
public string FullDeclarationWithAttributes { get; }
```

完整字段声明，包含特性和签名，默认剔除 Summary 特性

### MemberTypeName {#property-membertypename}

``` csharp
public string MemberTypeName { get; }
```

成员类型名称

### Signature {#property-signature}

``` csharp
public string Signature { get; private set; }
```

字段签名

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

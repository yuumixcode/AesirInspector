---
title: EventData
description: "Runestone.ScriptDocGenerator.EventData 的 API 文档"
---

# `EventData`

<div class="api-meta" markdown="1">

- **种类:** `class`
- **命名空间:** `Runestone.ScriptDocGenerator`
- **程序集:** `Runestone.ScriptDocGenerator`

</div>

**继承链:** `System.Object` → `Runestone.ScriptDocGenerator.MemberData` → `EventData`

**实现接口:** `Runestone.ScriptDocGenerator.IEventData`，`Runestone.ScriptDocGenerator.IDerivedMemberData`，`Runestone.ScriptDocGenerator.IMemberData`

## 语法

``` csharp
[Serializable]
public class EventData : Runestone.ScriptDocGenerator.MemberData, 
Runestone.ScriptDocGenerator.IEventData, 
Runestone.ScriptDocGenerator.IDerivedMemberData, 
Runestone.ScriptDocGenerator.IMemberData
```

事件解析数据类，用于存储事件的解析数据

## 构造方法

| 名称 | 描述 |
| :--- | :--- |
| [`EventData(EventInfo, IAttributeFilter)`](#constructor-eventdata-eventinfo-iattributefilter) | — |
{: .api-summary-table }

### EventData(EventInfo, IAttributeFilter) {#constructor-eventdata-eventinfo-iattributefilter}

``` csharp
public EventData(EventInfo eventInfo, IAttributeFilter filter = null)
```

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `eventInfo` | `EventInfo` |
| `filter` | `IAttributeFilter` |
{: .api-params-table }

## 属性

**声明的属性**

| 名称 | 描述 |
| :--- | :--- |
| [`AccessModifier`](#property-accessmodifier) | 访问修饰符类型 |
| [`MemberType`](#property-membertype) | 成员类型 |
| [`EventType`](#property-eventtype) | 事件类型 |
| [`IsStatic`](#property-isstatic) | 是否为静态事件 |
| [`AccessModifierName`](#property-accessmodifiername) | 访问修饰符名称 |
| [`EventTypeFullName`](#property-eventtypefullname) | 事件类型的完整名称，包括命名空间 |
| [`EventTypeName`](#property-eventtypename) | 事件类型名称 |
| [`FullDeclarationWithAttributes`](#property-fulldeclarationwithattributes) | 包含特性和签名的完整事件声明 |
| [`MemberTypeName`](#property-membertypename) | 成员类型名称 |
| [`Signature`](#property-signature) | 事件的完整签名 |
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

成员类型

### EventType {#property-eventtype}

``` csharp
public Type EventType { get; }
```

事件类型

### IsStatic {#property-isstatic}

``` csharp
public bool IsStatic { get; }
```

是否为静态事件

### AccessModifierName {#property-accessmodifiername}

``` csharp
public string AccessModifierName { get; }
```

访问修饰符名称

### EventTypeFullName {#property-eventtypefullname}

``` csharp
public string EventTypeFullName { get; }
```

事件类型的完整名称，包括命名空间

### EventTypeName {#property-eventtypename}

``` csharp
public string EventTypeName { get; }
```

事件类型名称

### FullDeclarationWithAttributes {#property-fulldeclarationwithattributes}

``` csharp
public string FullDeclarationWithAttributes { get; }
```

包含特性和签名的完整事件声明

### MemberTypeName {#property-membertypename}

``` csharp
public string MemberTypeName { get; }
```

成员类型名称

### Signature {#property-signature}

``` csharp
public string Signature { get; private set; }
```

事件的完整签名

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

---
title: TypeData
description: "Runestone.ScriptDocGenerator.TypeData 的 API 文档"
---

# `TypeData`

<div class="api-meta" markdown="1">

- **种类:** `class`
- **命名空间:** `Runestone.ScriptDocGenerator`
- **程序集:** `Runestone.ScriptDocGenerator`

</div>

**继承链:** `System.Object` → `Runestone.ScriptDocGenerator.MemberData` → `TypeData`

**实现接口:** `Runestone.ScriptDocGenerator.ITypeData`，`Runestone.ScriptDocGenerator.IDerivedMemberData`，`Runestone.ScriptDocGenerator.IMemberData`

## 语法

``` csharp
[Serializable]
public class TypeData : Runestone.ScriptDocGenerator.MemberData, 
Runestone.ScriptDocGenerator.ITypeData, 
Runestone.ScriptDocGenerator.IDerivedMemberData, 
Runestone.ScriptDocGenerator.IMemberData
```

类型解析数据类，存储类型的各种成员的解析数据

## 构造方法

| 名称 | 描述 |
| :--- | :--- |
| [`TypeData(Type, IAttributeFilter, IAnalysisDataFactory)`](#constructor-typedata-type-iattributefilter-ianalysisdatafactory) | — |
{: .api-summary-table }

### TypeData(Type, IAttributeFilter, IAnalysisDataFactory) {#constructor-typedata-type-iattributefilter-ianalysisdatafactory}

``` csharp
public TypeData(Type type, IAttributeFilter filter = null, IAnalysisDataFactory factory = null)
```

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `type` | `Type` |
| `filter` | `IAttributeFilter` |
| `factory` | `IAnalysisDataFactory` |
{: .api-params-table }

## 属性

**声明的属性**

| 名称 | 描述 |
| :--- | :--- |
| [`AccessModifier`](#property-accessmodifier) | 访问修饰符 |
| [`Assembly`](#property-assembly) | 类型所在的程序集 |
| [`DataFactory`](#property-datafactory) | 分析数据工厂实例对象 |
| [`RuntimeReflectedConstructorsData`](#property-runtimereflectedconstructorsdata) | 声明的构造方法解析数据数组，只包含公共构造函数，GetConstructors() 方法 |
| [`RuntimeReflectedEventsData`](#property-runtimereflectedeventsdata) | 声明的事件解析数据数组，GetRuntimeEvents() 方法 |
| [`RuntimeReflectedFieldsData`](#property-runtimereflectedfieldsdata) | 类型的字段解析数据数组，GetUserDefinedFields() 方法 |
| [`RuntimeReflectedMethodsData`](#property-runtimereflectedmethodsdata) | 声明的方法解析数据数组，GetRuntimeMethods() 方法 |
| [`RuntimeReflectedPropertiesData`](#property-runtimereflectedpropertiesdata) | 声明的属性解析数据数组，GetRuntimeProperties() 方法 |
| [`MemberType`](#property-membertype) | 成员类型 |
| [`TypeCategory`](#property-typecategory) | Type 种类 |
| [`IsAbstract`](#property-isabstract) | 是否为抽象类 |
| [`IsGenericType`](#property-isgenerictype) | 是否为泛型类型 |
| [`IsSealed`](#property-issealed) | 是否为密封类 |
| [`IsStatic`](#property-isstatic) | 是否为静态类型 |
| [`AccessModifierName`](#property-accessmodifiername) | 访问修饰符名称 |
| [`AssemblyName`](#property-assemblyname) | 程序集名称 |
| [`FullDeclarationWithAttributes`](#property-fulldeclarationwithattributes) | 完整类型声明 - 包含特性和签名 - 默认剔除 [Summary] 特性 |
| [`MemberTypeName`](#property-membertypename) | 成员类型名称 |
| [`NamespaceName`](#property-namespacename) | 命名空间名称 |
| [`Signature`](#property-signature) | 类型签名，不包含特性声明 |
| [`InheritanceChain`](#property-inheritancechain) | 继承链数组 |
| [`InterfaceArray`](#property-interfacearray) | 接口列表数组 |
| [`ReferenceWebLinkArray`](#property-referenceweblinkarray) | 引用链接数组 |
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

访问修饰符

### Assembly {#property-assembly}

``` csharp
public Assembly Assembly { get; }
```

类型所在的程序集

### DataFactory {#property-datafactory}

``` csharp
public IAnalysisDataFactory DataFactory { get; }
```

分析数据工厂实例对象

### RuntimeReflectedConstructorsData {#property-runtimereflectedconstructorsdata}

``` csharp
public IConstructorData[] RuntimeReflectedConstructorsData { get; }
```

声明的构造方法解析数据数组，只包含公共构造函数，GetConstructors() 方法

### RuntimeReflectedEventsData {#property-runtimereflectedeventsdata}

``` csharp
public IEventData[] RuntimeReflectedEventsData { get; }
```

声明的事件解析数据数组，GetRuntimeEvents() 方法

### RuntimeReflectedFieldsData {#property-runtimereflectedfieldsdata}

``` csharp
public IFieldData[] RuntimeReflectedFieldsData { get; }
```

类型的字段解析数据数组，GetUserDefinedFields() 方法

### RuntimeReflectedMethodsData {#property-runtimereflectedmethodsdata}

``` csharp
public IMethodData[] RuntimeReflectedMethodsData { get; }
```

声明的方法解析数据数组，GetRuntimeMethods() 方法

### RuntimeReflectedPropertiesData {#property-runtimereflectedpropertiesdata}

``` csharp
public IPropertyData[] RuntimeReflectedPropertiesData { get; }
```

声明的属性解析数据数组，GetRuntimeProperties() 方法

### MemberType {#property-membertype}

``` csharp
public MemberTypes MemberType { get; }
```

成员类型

### TypeCategory {#property-typecategory}

``` csharp
public TypeCategory TypeCategory { get; }
```

Type 种类

### IsAbstract {#property-isabstract}

``` csharp
public bool IsAbstract { get; }
```

是否为抽象类

### IsGenericType {#property-isgenerictype}

``` csharp
public bool IsGenericType { get; }
```

是否为泛型类型

### IsSealed {#property-issealed}

``` csharp
public bool IsSealed { get; }
```

是否为密封类

### IsStatic {#property-isstatic}

``` csharp
public bool IsStatic { get; }
```

是否为静态类型

### AccessModifierName {#property-accessmodifiername}

``` csharp
public string AccessModifierName { get; }
```

访问修饰符名称

### AssemblyName {#property-assemblyname}

``` csharp
public string AssemblyName { get; }
```

程序集名称

### FullDeclarationWithAttributes {#property-fulldeclarationwithattributes}

``` csharp
public string FullDeclarationWithAttributes { get; }
```

完整类型声明 - 包含特性和签名 - 默认剔除 [Summary] 特性

### MemberTypeName {#property-membertypename}

``` csharp
public string MemberTypeName { get; }
```

成员类型名称

### NamespaceName {#property-namespacename}

``` csharp
public string NamespaceName { get; }
```

命名空间名称

### Signature {#property-signature}

``` csharp
public string Signature { get; }
```

类型签名，不包含特性声明

### InheritanceChain {#property-inheritancechain}

``` csharp
public string[] InheritanceChain { get; }
```

继承链数组

### InterfaceArray {#property-interfacearray}

``` csharp
public string[] InterfaceArray { get; }
```

接口列表数组

### ReferenceWebLinkArray {#property-referenceweblinkarray}

``` csharp
public string[] ReferenceWebLinkArray { get; }
```

引用链接数组

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

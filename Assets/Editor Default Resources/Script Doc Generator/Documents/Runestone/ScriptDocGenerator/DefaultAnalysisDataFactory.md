---
title: DefaultAnalysisDataFactory
description: "Runestone.ScriptDocGenerator.DefaultAnalysisDataFactory 的 API 文档"
---

# `DefaultAnalysisDataFactory`

<div class="api-meta" markdown="1">

- **种类:** `class`
- **命名空间:** `Runestone.ScriptDocGenerator`
- **程序集:** `Runestone.ScriptDocGenerator`

</div>

**继承链:** `System.Object` → `DefaultAnalysisDataFactory`

**实现接口:** `Runestone.ScriptDocGenerator.IAnalysisDataFactory`

## 语法

``` csharp
[Serializable]
public class DefaultAnalysisDataFactory : Runestone.ScriptDocGenerator.IAnalysisDataFactory
```

Aesir Inspector 默认提供的解析数据工厂实现类

## 构造方法

| 名称 | 描述 |
| :--- | :--- |
| [`DefaultAnalysisDataFactory()`](#constructor-defaultanalysisdatafactory) | — |
{: .api-summary-table }

### DefaultAnalysisDataFactory() {#constructor-defaultanalysisdatafactory}

``` csharp
public DefaultAnalysisDataFactory()
```

## 方法

**声明的方法**

| 名称 | 描述 |
| :--- | :--- |
| [`CreateConstructorData(ConstructorInfo, IAttributeFilter)`](#method-createconstructordata-constructorinfo-iattributefilter) | 创建构造函数数据 |
| [`CreateEventData(EventInfo, IAttributeFilter)`](#method-createeventdata-eventinfo-iattributefilter) | 创建事件数据 |
| [`CreateFieldData(FieldInfo, IAttributeFilter)`](#method-createfielddata-fieldinfo-iattributefilter) | 创建字段数据 |
| [`CreateMethodData(MethodInfo, IAttributeFilter)`](#method-createmethoddata-methodinfo-iattributefilter) | 创建方法数据 |
| [`CreatePropertyData(PropertyInfo, IAttributeFilter)`](#method-createpropertydata-propertyinfo-iattributefilter) | 创建属性数据 |
| [`CreateTypeData(Type, IAnalysisDataFactory, IAttributeFilter)`](#method-createtypedata-type-ianalysisdatafactory-iattributefilter) | 创建类型数据 |
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

### CreateConstructorData(ConstructorInfo, IAttributeFilter) {#method-createconstructordata-constructorinfo-iattributefilter}

``` csharp
public IConstructorData CreateConstructorData(ConstructorInfo constructorInfo, IAttributeFilter filter = null)
```

创建构造函数数据

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `constructorInfo` | `ConstructorInfo` |
| `filter` | `IAttributeFilter` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `IConstructorData` |
{: .api-returns-table }

### CreateEventData(EventInfo, IAttributeFilter) {#method-createeventdata-eventinfo-iattributefilter}

``` csharp
public IEventData CreateEventData(EventInfo eventInfo, IAttributeFilter filter = null)
```

创建事件数据

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `eventInfo` | `EventInfo` |
| `filter` | `IAttributeFilter` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `IEventData` |
{: .api-returns-table }

### CreateFieldData(FieldInfo, IAttributeFilter) {#method-createfielddata-fieldinfo-iattributefilter}

``` csharp
public IFieldData CreateFieldData(FieldInfo fieldInfo, IAttributeFilter filter = null)
```

创建字段数据

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `fieldInfo` | `FieldInfo` |
| `filter` | `IAttributeFilter` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `IFieldData` |
{: .api-returns-table }

### CreateMethodData(MethodInfo, IAttributeFilter) {#method-createmethoddata-methodinfo-iattributefilter}

``` csharp
public IMethodData CreateMethodData(MethodInfo methodInfo, IAttributeFilter filter = null)
```

创建方法数据

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `methodInfo` | `MethodInfo` |
| `filter` | `IAttributeFilter` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `IMethodData` |
{: .api-returns-table }

### CreatePropertyData(PropertyInfo, IAttributeFilter) {#method-createpropertydata-propertyinfo-iattributefilter}

``` csharp
public IPropertyData CreatePropertyData(PropertyInfo propertyInfo, IAttributeFilter filter = null)
```

创建属性数据

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `propertyInfo` | `PropertyInfo` |
| `filter` | `IAttributeFilter` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `IPropertyData` |
{: .api-returns-table }

### CreateTypeData(Type, IAnalysisDataFactory, IAttributeFilter) {#method-createtypedata-type-ianalysisdatafactory-iattributefilter}

``` csharp
public ITypeData CreateTypeData(Type type, IAnalysisDataFactory factory = null, IAttributeFilter filter = null)
```

创建类型数据

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `type` | `Type` |
| `factory` | `IAnalysisDataFactory` |
| `filter` | `IAttributeFilter` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `ITypeData` |
{: .api-returns-table }

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。

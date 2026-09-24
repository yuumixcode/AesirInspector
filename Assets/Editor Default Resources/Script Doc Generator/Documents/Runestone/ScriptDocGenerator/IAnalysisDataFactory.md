---
title: IAnalysisDataFactory
description: "Runestone.ScriptDocGenerator.IAnalysisDataFactory 的 API 文档"
---

# `IAnalysisDataFactory`

<div class="api-meta" markdown="1">

- **种类:** `interface`
- **命名空间:** `Runestone.ScriptDocGenerator`
- **程序集:** `Runestone.ScriptDocGenerator`

</div>

## 语法

``` csharp
public interface IAnalysisDataFactory
```

解析数据工厂接口，自定义扩展解析数据工厂

## 方法

| 名称 | 描述 |
| :--- | :--- |
| [`CreateConstructorData(ConstructorInfo, IAttributeFilter)`](#method-createconstructordata-constructorinfo-iattributefilter) | 创建构造函数数据 |
| [`CreateEventData(EventInfo, IAttributeFilter)`](#method-createeventdata-eventinfo-iattributefilter) | — |
| [`CreateFieldData(FieldInfo, IAttributeFilter)`](#method-createfielddata-fieldinfo-iattributefilter) | — |
| [`CreateMethodData(MethodInfo, IAttributeFilter)`](#method-createmethoddata-methodinfo-iattributefilter) | — |
| [`CreatePropertyData(PropertyInfo, IAttributeFilter)`](#method-createpropertydata-propertyinfo-iattributefilter) | — |
| [`CreateTypeData(Type, IAnalysisDataFactory, IAttributeFilter)`](#method-createtypedata-type-ianalysisdatafactory-iattributefilter) | 创建类型数据 |
{: .api-summary-table }

### CreateConstructorData(ConstructorInfo, IAttributeFilter) {#method-createconstructordata-constructorinfo-iattributefilter}

``` csharp
public abstract IConstructorData CreateConstructorData(ConstructorInfo constructorInfo, IAttributeFilter filter = null)
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
public abstract IEventData CreateEventData(EventInfo eventInfo, IAttributeFilter filter = null)
```

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
public abstract IFieldData CreateFieldData(FieldInfo fieldInfo, IAttributeFilter filter = null)
```

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
public abstract IMethodData CreateMethodData(MethodInfo methodInfo, IAttributeFilter filter = null)
```

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
public abstract IPropertyData CreatePropertyData(PropertyInfo propertyInfo, IAttributeFilter filter = null)
```

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
public abstract ITypeData CreateTypeData(Type type, IAnalysisDataFactory factory = null, IAttributeFilter filter = null)
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

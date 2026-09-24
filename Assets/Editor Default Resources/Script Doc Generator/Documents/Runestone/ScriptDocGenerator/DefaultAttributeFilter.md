---
title: DefaultAttributeFilter
description: "Runestone.ScriptDocGenerator.DefaultAttributeFilter 的 API 文档"
---

# `DefaultAttributeFilter`

<div class="api-meta" markdown="1">

- **种类:** `class`
- **命名空间:** `Runestone.ScriptDocGenerator`
- **程序集:** `Runestone.ScriptDocGenerator`

</div>

**继承链:** `System.Object` → `DefaultAttributeFilter`

**实现接口:** `Runestone.ScriptDocGenerator.IAttributeFilter`

## 语法

``` csharp
public class DefaultAttributeFilter : Runestone.ScriptDocGenerator.IAttributeFilter
```

默认特性过滤器，构造函数中传入需要排除的 Attribute 类型

## 构造方法

| 名称 | 描述 |
| :--- | :--- |
| [`DefaultAttributeFilter(Type[])`](#constructor-defaultattributefilter-type) | 创建默认特性过滤器 |
{: .api-summary-table }

### DefaultAttributeFilter(Type[]) {#constructor-defaultattributefilter-type}

``` csharp
public DefaultAttributeFilter(Type[] excludeTypes)
```

创建默认特性过滤器

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `excludeTypes` | `Type[]` |
{: .api-params-table }

## 属性

| 名称 | 描述 |
| :--- | :--- |
| [`ExcludeTypes`](#property-excludetypes) | 排除的特性类型 |
{: .api-summary-table }

### ExcludeTypes {#property-excludetypes}

``` csharp
public Type[] ExcludeTypes { get; }
```

排除的特性类型

## 方法

**声明的方法**

| 名称 | 描述 |
| :--- | :--- |
| [`ShouldFilterOut(Type)`](#method-shouldfilterout-type) | 判断传入的特性类型是否应该被过滤掉 |
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

### ShouldFilterOut(Type) {#method-shouldfilterout-type}

``` csharp
public bool ShouldFilterOut(Type type)
```

判断传入的特性类型是否应该被过滤掉

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `type` | `Type` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `bool` |
{: .api-returns-table }

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。

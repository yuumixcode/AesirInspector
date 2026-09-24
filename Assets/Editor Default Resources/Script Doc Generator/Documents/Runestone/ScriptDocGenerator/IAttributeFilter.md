---
title: IAttributeFilter
description: "Runestone.ScriptDocGenerator.IAttributeFilter 的 API 文档"
---

# `IAttributeFilter`

<div class="api-meta" markdown="1">

- **种类:** `interface`
- **命名空间:** `Runestone.ScriptDocGenerator`
- **程序集:** `Runestone.ScriptDocGenerator`

</div>

## 语法

``` csharp
public interface IAttributeFilter
```

特性过滤器接口，用于过滤掉不需要的特性

## 属性

| 名称 | 描述 |
| :--- | :--- |
| [`ExcludeTypes`](#property-excludetypes) | — |
{: .api-summary-table }

### ExcludeTypes {#property-excludetypes}

``` csharp
public Type[] ExcludeTypes { get; }
```

## 方法

| 名称 | 描述 |
| :--- | :--- |
| [`ShouldFilterOut(Type)`](#method-shouldfilterout-type) | 判断传入的特性类型是否应该被过滤掉 |
{: .api-summary-table }

### ShouldFilterOut(Type) {#method-shouldfilterout-type}

``` csharp
public abstract bool ShouldFilterOut(Type type)
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

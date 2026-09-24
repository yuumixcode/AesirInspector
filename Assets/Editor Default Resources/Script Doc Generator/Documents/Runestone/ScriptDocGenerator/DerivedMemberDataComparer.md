---
title: DerivedMemberDataComparer
description: "Runestone.ScriptDocGenerator.DerivedMemberDataComparer 的 API 文档"
---

# `DerivedMemberDataComparer`

<div class="api-meta" markdown="1">

- **种类:** `class`
- **命名空间:** `Runestone.ScriptDocGenerator`
- **程序集:** `Runestone.ScriptDocGenerator`

</div>

**继承链:** `System.Object` → `DerivedMemberDataComparer`

**实现接口:** `System.Collections.Generic.IComparer<IDerivedMemberData>`

## 语法

``` csharp
public class DerivedMemberDataComparer : System.Collections.Generic.IComparer<IDerivedMemberData>
```

IDerivedMemberData 比较类

## 构造方法

| 名称 | 描述 |
| :--- | :--- |
| [`DerivedMemberDataComparer()`](#constructor-derivedmemberdatacomparer) | — |
{: .api-summary-table }

### DerivedMemberDataComparer() {#constructor-derivedmemberdatacomparer}

``` csharp
public DerivedMemberDataComparer()
```

## 方法

**声明的方法**

| 名称 | 描述 |
| :--- | :--- |
| [`Compare(IDerivedMemberData, IDerivedMemberData)`](#method-compare-iderivedmemberdata-iderivedmemberdata) | 比较两个继承 IDerivedMemberData 的数据类的实例，用于排序 |
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

### Compare(IDerivedMemberData, IDerivedMemberData) {#method-compare-iderivedmemberdata-iderivedmemberdata}

``` csharp
public int Compare(IDerivedMemberData x, IDerivedMemberData y)
```

比较两个继承 IDerivedMemberData 的数据类的实例，用于排序

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `x` | `IDerivedMemberData` |
| `y` | `IDerivedMemberData` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `int` |
{: .api-returns-table }

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。

---
title: ReflectionUtility
description: "Runestone.ScriptDocGenerator.ReflectionUtility 的 API 文档"
---

# `ReflectionUtility`

<div class="api-meta" markdown="1">

- **种类:** `static class`
- **命名空间:** `Runestone.ScriptDocGenerator`
- **程序集:** `Runestone.ScriptDocGenerator`

</div>

**继承链:** `System.Object` → `ReflectionUtility`

## 语法

``` csharp
public static class ReflectionUtility
```

反射工具类，提供程序集、命名空间及成员的反射操作方法

## 方法

**声明的方法**

| 名称 | 描述 |
| :--- | :--- |
| [`GetAssembliesOfNameContainString(string)`](#method-getassembliesofnamecontainstring-string) | 获取名称中包含指定字符串的所有程序集 |
| [`GetAttributes(ICustomAttributeProvider)`](#method-getattributes-icustomattributeprovider) | 获取成员上的指定类型特性。 |
| [`GetAttributes(ICustomAttributeProvider, bool)`](#method-getattributes-icustomattributeprovider-bool) | 获取成员上的指定类型特性。 |
| [`GetBaseClasses(Type, bool)`](#method-getbaseclasses-type-bool) | 获取类型的所有基类。 |
| [`GetBaseTypes(Type, bool)`](#method-getbasetypes-type-bool) | 获取类型的所有基类和接口。 |
| [`GetNamespacesInAssembly(Assembly)`](#method-getnamespacesinassembly-assembly) | 获取指定程序集中的所有命名空间 |
| [`GetReturnType(MemberInfo)`](#method-getreturntype-memberinfo) | 获取成员的返回类型（支持字段、属性、方法和事件）。 |
| [`IsExtensionMethod(MethodBase)`](#method-isextensionmethod-methodbase) | 判断方法是否为扩展方法。 |
| [`IsStatic(MemberInfo)`](#method-isstatic-memberinfo) | 判断成员是否为静态成员。 |
| [`GetMemberValue(MemberInfo, object)`](#method-getmembervalue-memberinfo-object) | 获取成员的值（支持字段和属性）。 |
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

### GetAssembliesOfNameContainString(string) {#method-getassembliesofnamecontainstring-string}

``` csharp
public static Assembly[] GetAssembliesOfNameContainString(string partOfAssemblyName)
```

获取名称中包含指定字符串的所有程序集

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `partOfAssemblyName` | `string` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `Assembly[]` |
{: .api-returns-table }

### GetAttributes(ICustomAttributeProvider) {#method-getattributes-icustomattributeprovider}

``` csharp
public static IEnumerable<T> GetAttributes<T>(ICustomAttributeProvider member)
```

获取成员上的指定类型特性。

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `member` | `ICustomAttributeProvider` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `IEnumerable<T>` |
{: .api-returns-table }

### GetAttributes(ICustomAttributeProvider, bool) {#method-getattributes-icustomattributeprovider-bool}

``` csharp
public static IEnumerable<T> GetAttributes<T>(ICustomAttributeProvider member, bool inherit)
```

获取成员上的指定类型特性。

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `member` | `ICustomAttributeProvider` |
| `inherit` | `bool` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `IEnumerable<T>` |
{: .api-returns-table }

### GetBaseClasses(Type, bool) {#method-getbaseclasses-type-bool}

``` csharp
[IteratorStateMachine]
public static IEnumerable<Type> GetBaseClasses(Type type, bool includeSelf = false)
```

获取类型的所有基类。

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `type` | `Type` |
| `includeSelf` | `bool` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `IEnumerable<Type>` |
{: .api-returns-table }

### GetBaseTypes(Type, bool) {#method-getbasetypes-type-bool}

``` csharp
public static IEnumerable<Type> GetBaseTypes(Type type, bool includeSelf = false)
```

获取类型的所有基类和接口。

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `type` | `Type` |
| `includeSelf` | `bool` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `IEnumerable<Type>` |
{: .api-returns-table }

### GetNamespacesInAssembly(Assembly) {#method-getnamespacesinassembly-assembly}

``` csharp
public static List<string> GetNamespacesInAssembly(Assembly assembly)
```

获取指定程序集中的所有命名空间

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `assembly` | `Assembly` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `List<string>` |
{: .api-returns-table }

### GetReturnType(MemberInfo) {#method-getreturntype-memberinfo}

``` csharp
public static Type GetReturnType(MemberInfo memberInfo)
```

获取成员的返回类型（支持字段、属性、方法和事件）。

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `memberInfo` | `MemberInfo` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `Type` |
{: .api-returns-table }

### IsExtensionMethod(MethodBase) {#method-isextensionmethod-methodbase}

``` csharp
public static bool IsExtensionMethod(MethodBase method)
```

判断方法是否为扩展方法。

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `method` | `MethodBase` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `bool` |
{: .api-returns-table }

### IsStatic(MemberInfo) {#method-isstatic-memberinfo}

``` csharp
public static bool IsStatic(MemberInfo member)
```

判断成员是否为静态成员。

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `member` | `MemberInfo` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `bool` |
{: .api-returns-table }

### GetMemberValue(MemberInfo, object) {#method-getmembervalue-memberinfo-object}

``` csharp
public static object GetMemberValue(MemberInfo member, object obj)
```

获取成员的值（支持字段和属性）。

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `member` | `MemberInfo` |
| `obj` | `object` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `object` |
{: .api-returns-table }

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。

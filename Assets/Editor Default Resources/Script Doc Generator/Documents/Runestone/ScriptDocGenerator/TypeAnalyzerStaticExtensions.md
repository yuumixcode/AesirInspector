---
title: TypeAnalyzerStaticExtensions
description: "Runestone.ScriptDocGenerator.TypeAnalyzerStaticExtensions 的 API 文档"
---

# `TypeAnalyzerStaticExtensions`

<div class="api-meta" markdown="1">

- **种类:** `static class`
- **命名空间:** `Runestone.ScriptDocGenerator`
- **程序集:** `Runestone.ScriptDocGenerator`

</div>

**继承链:** `System.Object` → `TypeAnalyzerStaticExtensions`

## 语法

``` csharp
[Extension]
public static class TypeAnalyzerStaticExtensions
```

类型分析器静态扩展类，统一管理类型分析器有关的静态扩展方法

## 方法

**声明的方法**

| 名称 | 描述 |
| :--- | :--- |
| [`GetEventAccessModifierType(EventInfo)`](#method-geteventaccessmodifiertype-eventinfo) | 获取事件的访问修饰符类型 |
| [`GetFieldAccessModifier(FieldInfo)`](#method-getfieldaccessmodifier-fieldinfo) | 获取字段访问修饰符 |
| [`GetMethodAccessModifierType(MethodBase)`](#method-getmethodaccessmodifiertype-methodbase) | 获取方法的访问修饰符类型 |
| [`GetPropertyAccessModifierType(PropertyInfo)`](#method-getpropertyaccessmodifiertype-propertyinfo) | 获取属性的访问修饰符类型 |
| [`GetTypeAccessModifier(Type)`](#method-gettypeaccessmodifier-type) | 获取类型的访问修饰符 |
| [`GetUserDefinedFields(Type)`](#method-getuserdefinedfields-type) | 获取开发者声明的字段，剔除自动属性生成的字段 |
| [`GetTypeCategory(Type)`](#method-gettypecategory-type) | 获取类型的种类 |
| [`IsAbstractOrInterface(Type)`](#method-isabstractorinterface-type) | 判断一个类型是否为抽象类或接口 |
| [`IsApiMember(IDerivedMemberData)`](#method-isapimember-iderivedmemberdata) | 判断是否为 API 成员，返回 true 表示是 API 成员，返回 false 表示不是。API 成员指的是公共成员或受保护成员。 |
| [`IsAsyncMethod(MethodBase)`](#method-isasyncmethod-methodbase) | 判断方法是否是异步方法 |
| [`IsDelegate(Type)`](#method-isdelegate-type) | 判断指定类型是否为委托类型 |
| [`IsDynamicField(FieldInfo)`](#method-isdynamicfield-fieldinfo) | 判断是否为动态字段 |
| [`IsFromInheritance(MemberInfo)`](#method-isfrominheritance-memberinfo) | 判断成员是否从继承中获取，这里的成员不包括 Type 类型 |
| [`IsFromInterfaceImplementMethod(MethodBase)`](#method-isfrominterfaceimplementmethod-methodbase) | 判断是否为接口的实现方法 |
| [`IsInheritedOverrideFromAncestor(MethodInfo, Type)`](#method-isinheritedoverridefromancestor-methodinfo-type) | 判断方法是否为从祖先类继承的重写方法，重写声明不是在当前类中 |
| [`IsOperatorMethod(MethodBase)`](#method-isoperatormethod-methodbase) | 判断方法是否是运算符方法 |
| [`IsOverrideMethod(MethodInfo)`](#method-isoverridemethod-methodinfo) | 方法是否具有 override 的特性 |
| [`IsRecord(Type)`](#method-isrecord-type) | 判断指定类型是否为 record（包括 record class 和 record struct） |
| [`IsRecordStruct(Type)`](#method-isrecordstruct-type) | 判断类型是否为 record struct（值类型 record） |
| [`IsReferenceTypeExcludeString(Type)`](#method-isreferencetypeexcludestring-type) | 判断一个类型是否为非字符串的引用类型（非值类型） |
| [`IsStaticProperty(PropertyInfo)`](#method-isstaticproperty-propertyinfo) | 判断是否为静态属性 |
| [`TryAsIMemberData(IDerivedMemberData, ref IMemberData)`](#method-tryasimemberdata-iderivedmemberdata-ref-imemberdata) | 将 IDerivedMemberData 转换为 IMemberData，转换成功返回 true，转换失败返回 false |
| [`TryGetFieldCustomDefaultValue(FieldInfo, ref Object)`](#method-trygetfieldcustomdefaultvalue-fieldinfo-ref-object) | 获取字段的自定义默认值，不能获取到值则返回 null。只获取静态字段和常量字段的默认值。 |
| [`TryGetPropertyCustomDefaultValue(PropertyInfo, ref Object)`](#method-trygetpropertycustomdefaultvalue-propertyinfo-ref-object) | 获取属性的自定义默认值，不能获取到值则返回 null，只获取静态属性的默认值。 |
| [`GetAttributesDeclarationWithMultiLine(MemberInfo, IAttributeFilter)`](#method-getattributesdeclarationwithmultiline-memberinfo-iattributefilter) | 获取特性声明字符串，多行显示 |
| [`GetMethodNameAndParameters(MethodBase)`](#method-getmethodnameandparameters-methodbase) | 获取方法名称和参数列表，不包含返回值和修饰符 |
| [`GetParametersNameWithDefaultValue(MethodBase)`](#method-getparametersnamewithdefaultvalue-methodbase) | 获取方法的参数签名，包含默认值 |
| [`GetReadableTypeName(Type, bool)`](#method-getreadabletypename-type-bool) | 将反射获取到的系统类型名称转换为人类可读的 C# 风格类型名称 |
| [`GetInheritanceChain(Type)`](#method-getinheritancechain-type) | 获取一个类型的继承链，不包括接口 |
| [`GetInterfaceArray(Type)`](#method-getinterfacearray-type) | 获取一个类型继承的所有接口 |
| [`GetReferenceLinks(Type)`](#method-getreferencelinks-type) | 获取一个数组，内容是所有的 ReferenceLinkURL 特性中的网页链接字符串 |
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

### GetEventAccessModifierType(EventInfo) {#method-geteventaccessmodifiertype-eventinfo}

``` csharp
[Extension]
[Ext] public static AccessModifierType GetEventAccessModifierType(this EventInfo eventInfo)
```

获取事件的访问修饰符类型

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `eventInfo` | `EventInfo` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `AccessModifierType` |
{: .api-returns-table }

### GetFieldAccessModifier(FieldInfo) {#method-getfieldaccessmodifier-fieldinfo}

``` csharp
[Extension]
[Ext] public static AccessModifierType GetFieldAccessModifier(this FieldInfo fieldInfo)
```

获取字段访问修饰符

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `fieldInfo` | `FieldInfo` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `AccessModifierType` |
{: .api-returns-table }

### GetMethodAccessModifierType(MethodBase) {#method-getmethodaccessmodifiertype-methodbase}

``` csharp
[Extension]
[Ext] public static AccessModifierType GetMethodAccessModifierType(this MethodBase method)
```

获取方法的访问修饰符类型

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `method` | `MethodBase` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `AccessModifierType` |
{: .api-returns-table }

### GetPropertyAccessModifierType(PropertyInfo) {#method-getpropertyaccessmodifiertype-propertyinfo}

``` csharp
[Extension]
[Ext] public static AccessModifierType GetPropertyAccessModifierType(this PropertyInfo propertyInfo)
```

获取属性的访问修饰符类型

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `propertyInfo` | `PropertyInfo` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `AccessModifierType` |
{: .api-returns-table }

### GetTypeAccessModifier(Type) {#method-gettypeaccessmodifier-type}

``` csharp
[Extension]
[Ext] public static AccessModifierType GetTypeAccessModifier(this Type type)
```

获取类型的访问修饰符

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `type` | `Type` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `AccessModifierType` |
{: .api-returns-table }

### GetUserDefinedFields(Type) {#method-getuserdefinedfields-type}

``` csharp
[Extension]
[Ext] public static FieldInfo[] GetUserDefinedFields(this Type type)
```

获取开发者声明的字段，剔除自动属性生成的字段

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `type` | `Type` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `FieldInfo[]` |
{: .api-returns-table }

### GetTypeCategory(Type) {#method-gettypecategory-type}

``` csharp
[Extension]
[Ext] public static TypeCategory GetTypeCategory(this Type type)
```

获取类型的种类

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `type` | `Type` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `TypeCategory` |
{: .api-returns-table }

### IsAbstractOrInterface(Type) {#method-isabstractorinterface-type}

``` csharp
[Extension]
[Ext] public static bool IsAbstractOrInterface(this Type type)
```

判断一个类型是否为抽象类或接口

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

### IsApiMember(IDerivedMemberData) {#method-isapimember-iderivedmemberdata}

``` csharp
[Extension]
[Ext] public static bool IsApiMember(this IDerivedMemberData derivedMemberData)
```

判断是否为 API 成员，返回 true 表示是 API 成员，返回 false 表示不是。API 成员指的是公共成员或受保护成员。

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `derivedMemberData` | `IDerivedMemberData` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `bool` |
{: .api-returns-table }

### IsAsyncMethod(MethodBase) {#method-isasyncmethod-methodbase}

``` csharp
[Extension]
[Ext] public static bool IsAsyncMethod(this MethodBase methodBase)
```

判断方法是否是异步方法

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `methodBase` | `MethodBase` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `bool` |
{: .api-returns-table }

### IsDelegate(Type) {#method-isdelegate-type}

``` csharp
[Extension]
[Ext] public static bool IsDelegate(this Type type)
```

判断指定类型是否为委托类型

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

### IsDynamicField(FieldInfo) {#method-isdynamicfield-fieldinfo}

``` csharp
[Extension]
[Ext] public static bool IsDynamicField(this FieldInfo fieldInfo)
```

判断是否为动态字段

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `fieldInfo` | `FieldInfo` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `bool` |
{: .api-returns-table }

### IsFromInheritance(MemberInfo) {#method-isfrominheritance-memberinfo}

``` csharp
[Extension]
[Ext] public static bool IsFromInheritance(this MemberInfo member)
```

判断成员是否从继承中获取，这里的成员不包括 Type 类型

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

### IsFromInterfaceImplementMethod(MethodBase) {#method-isfrominterfaceimplementmethod-methodbase}

``` csharp
[Extension]
[Ext] public static bool IsFromInterfaceImplementMethod(this MethodBase method)
```

判断是否为接口的实现方法

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

### IsInheritedOverrideFromAncestor(MethodInfo, Type) {#method-isinheritedoverridefromancestor-methodinfo-type}

``` csharp
[Extension]
[Ext] public static bool IsInheritedOverrideFromAncestor(this MethodInfo method, Type currentType)
```

判断方法是否为从祖先类继承的重写方法，重写声明不是在当前类中

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `method` | `MethodInfo` |
| `currentType` | `Type` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `bool` |
{: .api-returns-table }

### IsOperatorMethod(MethodBase) {#method-isoperatormethod-methodbase}

``` csharp
[Extension]
[Ext] public static bool IsOperatorMethod(this MethodBase methodInfo)
```

判断方法是否是运算符方法

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `methodInfo` | `MethodBase` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `bool` |
{: .api-returns-table }

### IsOverrideMethod(MethodInfo) {#method-isoverridemethod-methodinfo}

``` csharp
[Extension]
[Ext] public static bool IsOverrideMethod(this MethodInfo methodInfo)
```

方法是否具有 override 的特性

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `methodInfo` | `MethodInfo` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `bool` |
{: .api-returns-table }

### IsRecord(Type) {#method-isrecord-type}

``` csharp
[Extension]
[Ext] public static bool IsRecord(this Type type)
```

判断指定类型是否为 record（包括 record class 和 record struct）

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

### IsRecordStruct(Type) {#method-isrecordstruct-type}

``` csharp
[Extension]
[Ext] public static bool IsRecordStruct(this Type type)
```

判断类型是否为 record struct（值类型 record）

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

### IsReferenceTypeExcludeString(Type) {#method-isreferencetypeexcludestring-type}

``` csharp
[Extension]
[Ext] public static bool IsReferenceTypeExcludeString(this Type type)
```

判断一个类型是否为非字符串的引用类型（非值类型）

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

### IsStaticProperty(PropertyInfo) {#method-isstaticproperty-propertyinfo}

``` csharp
[Extension]
[Ext] public static bool IsStaticProperty(this PropertyInfo propertyInfo)
```

判断是否为静态属性

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `propertyInfo` | `PropertyInfo` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `bool` |
{: .api-returns-table }

### TryAsIMemberData(IDerivedMemberData, ref IMemberData) {#method-tryasimemberdata-iderivedmemberdata-ref-imemberdata}

``` csharp
[Extension]
[Ext] public static bool TryAsIMemberData(this IDerivedMemberData derivedMemberData, out ref IMemberData memberData)
```

将 IDerivedMemberData 转换为 IMemberData，转换成功返回 true，转换失败返回 false

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `derivedMemberData` | `IDerivedMemberData` |
| `memberData` | `ref IMemberData` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `bool` |
{: .api-returns-table }

### TryGetFieldCustomDefaultValue(FieldInfo, ref Object) {#method-trygetfieldcustomdefaultvalue-fieldinfo-ref-object}

``` csharp
[Extension]
[Ext] public static bool TryGetFieldCustomDefaultValue(this FieldInfo fieldInfo, out ref Object defaultValue)
```

获取字段的自定义默认值，不能获取到值则返回 null。只获取静态字段和常量字段的默认值。

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `fieldInfo` | `FieldInfo` |
| `defaultValue` | `ref Object` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `bool` |
{: .api-returns-table }

### TryGetPropertyCustomDefaultValue(PropertyInfo, ref Object) {#method-trygetpropertycustomdefaultvalue-propertyinfo-ref-object}

``` csharp
[Extension]
[Ext] public static bool TryGetPropertyCustomDefaultValue(this PropertyInfo propertyInfo, out ref Object defaultValue)
```

获取属性的自定义默认值，不能获取到值则返回 null，只获取静态属性的默认值。

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `propertyInfo` | `PropertyInfo` |
| `defaultValue` | `ref Object` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `bool` |
{: .api-returns-table }

### GetAttributesDeclarationWithMultiLine(MemberInfo, IAttributeFilter) {#method-getattributesdeclarationwithmultiline-memberinfo-iattributefilter}

``` csharp
[Extension]
[Ext] public static string GetAttributesDeclarationWithMultiLine(this MemberInfo member, IAttributeFilter filter = null)
```

获取特性声明字符串，多行显示

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `member` | `MemberInfo` |
| `filter` | `IAttributeFilter` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `string` |
{: .api-returns-table }

### GetMethodNameAndParameters(MethodBase) {#method-getmethodnameandparameters-methodbase}

``` csharp
[Extension]
[Ext] public static string GetMethodNameAndParameters(this MethodBase method)
```

获取方法名称和参数列表，不包含返回值和修饰符

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `method` | `MethodBase` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `string` |
{: .api-returns-table }

### GetParametersNameWithDefaultValue(MethodBase) {#method-getparametersnamewithdefaultvalue-methodbase}

``` csharp
[Extension]
[Ext] public static string GetParametersNameWithDefaultValue(this MethodBase method)
```

获取方法的参数签名，包含默认值

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `method` | `MethodBase` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `string` |
{: .api-returns-table }

### GetReadableTypeName(Type, bool) {#method-getreadabletypename-type-bool}

``` csharp
[Extension]
[Ext] public static string GetReadableTypeName(this Type type, bool useFullName = false)
```

将反射获取到的系统类型名称转换为人类可读的 C# 风格类型名称

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `type` | `Type` |
| `useFullName` | `bool` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `string` |
{: .api-returns-table }

### GetInheritanceChain(Type) {#method-getinheritancechain-type}

``` csharp
[Extension]
[Ext] public static string[] GetInheritanceChain(this Type type)
```

获取一个类型的继承链，不包括接口

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `type` | `Type` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `string[]` |
{: .api-returns-table }

### GetInterfaceArray(Type) {#method-getinterfacearray-type}

``` csharp
[Extension]
[Ext] public static string[] GetInterfaceArray(this Type type)
```

获取一个类型继承的所有接口

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `type` | `Type` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `string[]` |
{: .api-returns-table }

### GetReferenceLinks(Type) {#method-getreferencelinks-type}

``` csharp
[Extension]
[Ext] public static string[] GetReferenceLinks(this Type type)
```

获取一个数组，内容是所有的 ReferenceLinkURL 特性中的网页链接字符串

**参数**

| 名称 | 类型 |
| :--- | :--- |
| `type` | `Type` |
{: .api-params-table }

**返回值**

| 类型 |
| :--- |
| `string[]` |
{: .api-returns-table }

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。

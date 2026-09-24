---
title: IConstructorData
description: "Runestone.ScriptDocGenerator.IConstructorData 的 API 文档"
---

# `IConstructorData`

<div class="api-meta" markdown="1">

- **种类:** `interface`
- **命名空间:** `Runestone.ScriptDocGenerator`
- **程序集:** `Runestone.ScriptDocGenerator`

</div>

**实现接口:** `Runestone.ScriptDocGenerator.IDerivedMemberData`

## 语法

``` csharp
public interface IConstructorData : Runestone.ScriptDocGenerator.IDerivedMemberData
```

构造方法数据接口，继承自 IDerivedMemberData

## 属性

| 名称 | 描述 |
| :--- | :--- |
| [`ParametersDeclaration`](#property-parametersdeclaration) | — |
| [`SignatureWithoutParameters`](#property-signaturewithoutparameters) | — |
{: .api-summary-table }

### ParametersDeclaration {#property-parametersdeclaration}

``` csharp
public string ParametersDeclaration { get; }
```

### SignatureWithoutParameters {#property-signaturewithoutparameters}

``` csharp
public string SignatureWithoutParameters { get; }
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。

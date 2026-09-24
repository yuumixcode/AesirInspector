---
title: IEventData
description: "Runestone.ScriptDocGenerator.IEventData 的 API 文档"
---

# `IEventData`

<div class="api-meta" markdown="1">

- **种类:** `interface`
- **命名空间:** `Runestone.ScriptDocGenerator`
- **程序集:** `Runestone.ScriptDocGenerator`

</div>

**实现接口:** `Runestone.ScriptDocGenerator.IDerivedMemberData`

## 语法

``` csharp
public interface IEventData : Runestone.ScriptDocGenerator.IDerivedMemberData
```

事件数据接口，继承自 IDerivedMemberData

## 属性

| 名称 | 描述 |
| :--- | :--- |
| [`EventType`](#property-eventtype) | — |
| [`EventTypeFullName`](#property-eventtypefullname) | — |
| [`EventTypeName`](#property-eventtypename) | — |
{: .api-summary-table }

### EventType {#property-eventtype}

``` csharp
public Type EventType { get; }
```

### EventTypeFullName {#property-eventtypefullname}

``` csharp
public string EventTypeFullName { get; }
```

### EventTypeName {#property-eventtypename}

``` csharp
public string EventTypeName { get; }
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。

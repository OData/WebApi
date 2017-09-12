---
layout: post
title: "6.5 Customize unsupported types"
description: ""
category: "6. Customization"
---

ODataLib has a lot of its primitive types mapping to C# built-in types, for example, `System.String` maps to `Edm.String`, `System.Guid` maps to `Edm.Guid`.
Web API OData adds supporting for some unsupported types in ODataLib, for example: `unsigned int`, `unsigned long`, etc.
 
The mapping list for the unsupported types are:

C# Type       | Edm Type      |  Nullable|
------------------------------|-------------|---------|
System.Xml.Linq.XElement      | Edm.String  ||
System.Binary                 | Edm.Binary  ||
System.UInt16                 | Edm.Int32   | true/false|
System.UInt32                 | Edm.Int64   | true/false|
System.UInt64                 | Edm.Int64   | true/false|
char[]                        | Edm.String  | |
char                          | Edm.String  |true/false|
System.DataTime               | Edm.DateTimeOffset  |true/false|


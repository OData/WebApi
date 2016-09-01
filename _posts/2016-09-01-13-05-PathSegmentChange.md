---
layout: post
title : "13.5 Use ODL path classes "
description: "Path change"
category: "13. 6.x Features "
---

Since [Web API OData V6.0.0 beta](https://www.nuget.org/packages/Microsoft.AspNet.OData/6.0.0-beta2), Web API OData uses the ODL path classes directly.

In Web API OData V5.x, there are a lot of path segment classes defined to wrapper the corresponding path segment classes defined in ODL.
for example:

```C#
* BatchPathSegment
* BoundActionPathSegment
* BoundFunctionPathSegment
* KeyValuePathSegment
* ...
```

Where, 

1. Some segments are just used to wrapper the corresponding segments defined in ODL, such as BatchPathSegment.
2. Some segments are used to do some conversions, such as BoundFunctionPathSegment, KeyValuePathSegment.

**However**, ODL defines the same segments and Web API OData needn't to do such conversion. So, all segment classes defined in Web API OData are removed in v6.x.

Web API OData will directly use the path segment classes defined in ODL as follows:

```C#

* BatchReferenceSegment
* BatchSegment
* CountSegment
* DynamicPathSegment
* EntitySetSegment
* KeySegment
* MetadataSegment
* NavigationPropertyLinkSegment
* NavigationPropertySegment
* ODataPathSegment
* OperationImportSegment
* OperationSegment
* PathTemplateSegment
* PropertySegment
* SingletonSegment
* TypeSegment
* ValueSegment

```

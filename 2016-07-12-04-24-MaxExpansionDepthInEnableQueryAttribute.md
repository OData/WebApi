---
title : "4.24 MaxExpansionDepth in EnableQueryAttribute"
layout: post
category: "4. OData features"
---

Since [Web API OData V5.9.1](http://www.nuget.org/packages/Microsoft.AspNet.OData/5.9.1), it corrected the behavior of MaxExpansionDepth of EnableQueryAtrribute. MaxExpansionDepth means the max expansion depth for the $expand query option.

When MaxExpansionDepth  value is 0, it means the check is disabled, but if you use $level=max at the same time, the expand depth will be a default value : `2`, to avoid the dead loop.

Let's see some samples about this behavior.

`$expand=Manager($levels=max)` will be the same as 
`$expand=Manager($expand=Manager)`

`$expand=Manager($levels=3)` will be the same as 
`$expand=Manager($expand=Manager($expand=Manager))`

Related Issue [#731](https://github.com/OData/WebApi/issues/731).

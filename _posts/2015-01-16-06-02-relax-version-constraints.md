---
layout: post
title: "6.2 Relax version constraints"
description: ""
category: "6. Customization"
permalink: "/version-constraint-relax-flag"
---

For both Web API OData V3 and V4, a flag `IsRelaxedMatch` is introduced to relax the version constraint. With `IsRelaxedMatch = true`, ODataVersionConstraint will allow OData request to contain both V3 and V4 max version headers (V3: `MaxDataServiceVersion`, V4: `OData-MaxVersion`). Otherwise, the service will return response with status code 400. The default value of `IsRelaxdMatch` is false.

{% highlight csharp %}
public class ODataVersionConstraint : IHttpRouteConstraint
{
  ......
  public bool IsRelaxedMatch { get; set; }
  ......
}
{% endhighlight %}

To set this flag, API HasRelaxedODataVersionConstraint() under ODataRoute can be used as following:
{% highlight csharp %}
ODataRoute odataRoute = new ODataRoute(routePrefix: null, pathConstraint: null).HasRelaxedODataVersionConstraint();
{% endhighlight %}

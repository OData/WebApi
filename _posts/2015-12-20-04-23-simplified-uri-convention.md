---
layout: post
title: "4.23 OData Simplified Uri convention"
description: ""
category: "4. OData features"
---

OData v4 Web API [5.8 RC](https://www.nuget.org/packages/Microsoft.AspNet.OData/5.8.0-rc) 
intruduces a new OData Simplefied Uri convention that supports key-as-segment and default OData Uri convention side-by-side.

{% highlight text %}
~/odata/Customers/0
~/odata/Customers(0)
{% endhighlight %}

To enable the ODataSimplified Uri convention, in `WebApiConfig.cs`:
{% highlight csharp %}
// ...
var model = builder.GetEdmModel();

// Set Uri convention to ODataSimplified
config.SetUrlConventions(ODataUrlConventions.ODataSimplified);
config.MapODataServiceRoute("odata", "odata", model);
{% endhighlight %}

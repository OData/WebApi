---
layout: post
title: "4.21 Set namespace for operations in model builder"
description: ""
category: "4. OData features"
---

Since OData Web API V5.7, it allows to <strong>set a custom namespace for individual function and action in model builder</strong>.

### Set namespace for function and action in model builder

{% highlight csharp %}
ODataModelBuilder builder = new ODataModelBuilder();
builder.Namespace = "Default";
builder.ContainerName = "DefaultContainer";
ActionConfiguration action = builder.Action("MyAction");
action.Namespace = "MyNamespace";
FunctionConfiguration function = builder.Function("MyFunction");
function.Namespace = "MyNamespace";
{% endhighlight %}

The setting works for <strong>ODataConventionModelBuilder</strong> as well.

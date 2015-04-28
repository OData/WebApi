---
layout: post
title: "3.3 Attribute Routing"
description: "Routing Conventions"
category: "3. Routing"
---

Same as Web API, Web API OData supports a new type of routing called **attribute routing**. It uses two *Attributes* to find **controller** and **action**. One is `ODataPrefixAttribute`, the other is `ODataRouteAttribute`.

You can use **attribute routing** to define more complex routes and put more control over the routing. Most important, it can extend the coverage of convention routing. For example, you can easily use attribute routing to route the following Uri:

{% highlight csharp %}
~/odata/Customers(1)/Orders/Price
{% endhighlight %}

In Web API OData, **attribute routing** is combined with **convention routing** by default.

### Enabling Attribute Routing

`ODataRoutingConventions` provides two methods to register routing conventions:

{% highlight csharp %}
public static IList<IODataRoutingConvention> CreateDefaultWithAttributeRouting(HttpConfiguration configuration, IEdmModel model)

public static IList<IODataRoutingConvention> CreateDefault()
{% endhighlight %}

As the name implies, the first one creates a mutable list of the default OData routing conventions with attribute routing enabled, while the second one only includes convention routing.

In fact, when you call the basic `MapODataServiceRoute`, it enables the attribute routing by default as:
{% highlight csharp %}
public static ODataRoute MapODataServiceRoute(this HttpConfiguration configuration, string routeName, string routePrefix, IEdmModel model, ODataBatchHandler batchHandler)
{
    return MapODataServiceRoute(configuration, routeName, routePrefix, model, new DefaultODataPathHandler(),
        ODataRoutingConventions.CreateDefaultWithAttributeRouting(configuration, model), batchHandler);
}
{% endhighlight %}

However, you can call other version of `MapODataServiceRoute` to custom your own routing conventions. For example:
{% highlight csharp %}
public static ODataRoute MapODataServiceRoute(this HttpConfiguration configuration, string routeName, string routePrefix, IEdmModel model, IODataPathHandler pathHandler, IEnumerable<IODataRoutingConvention> routingConventions)
{% endhighlight %}

### ODataRouteAttribute

`ODataRouteAttribute` is an attribute that can, and only can be placed on an action of an OData controller to specify the OData URLs that the action handles.

Here is an example of an action defined using an `ODataRouteAttribute`:

{% highlight csharp %}
public class MyController : ODataController
{
    [HttpGet]
    [ODataRoute("Customers({id})/Address/City")]
    public string GetCityOfACustomer([FromODataUri]int id)
    {
        ......
    }
}
{% endhighlight %}

With this attribute, Web API OData tries to match the request Uri with `Customers({id})/Address/City` routing template to  `GetCityOfACustomer()` function in `MyController`. For example, the following request Uri will invoke `GetCityOfACustomer`:

{% highlight csharp %}
~/odata/Customers(1)/Address/City
~/odata/Customers(2)/Address/City
~/odata/Customers(301)/Address/City
{% endhighlight %}

For the above request Uri, `id` in the function will have `1`, `2` and `301` value.

However, for the following request Uri, it can't match to `GetCityOfACustomer()':
{% highlight csharp %}
~/odata/Customers
~/odata/Customers(1)/Address
{% endhighlight %}

Web API OData supports to put multiple `ODataRouteAttribute` on the same OData action. For example, 

{% highlight csharp %}
public class MyController : ODataController
{
    [HttpGet]
    [ODataRoute("Customers({id})/Address/City")]
    [ODataRoute("Products({id})/Address/City")]
    public string GetCityOfACustomer([FromODataUri]int id)
    {
        ......
    }
}
{% endhighlight %}

### ODataRoutePrefixAttribute

`ODataRoutePrefixAttribute` is an attribute that can, and only can be placed on an *OData controller* to specify the prefix that will be used for all actions of that controller.

`ODataRoutePrefixAttribute` is used to reduce the routing template in `ODataRouteAttribute` if all routing template in the controller start with the same prefix. For example:

{% highlight csharp %}
public class MyController : ODataController
{
    [ODataRoute("Customers({id})/Address")]
    public IHttpActionResult GetAddress(int id)
    {
        ......
    }

    [ODataRoute("Customers({id})/Address/City")]
    public IHttpActionResult GetCity(int id)
    {
        ......
    }

    [ODataRoute("Customers({id})/Order")]
    public IHttpActionResult GetOrder(int id)
    {
        ......
    }
}
{% endhighlight %}

Then you can use `ODataRoutePrefixAttribute` attribute on the controller to set a common prefix.

{% highlight csharp %}
[ODataRoutePrefix("Customers({id})")]
public class MyController : ODataController
{
    [ODataRoute("Address")]
    public IHttpActionResult GetAddress(int id)
    {
        ......
    }

    [ODataRoute("Address/City")]
    public IHttpActionResult GetCity(int id)
    {
        ......
    }

    [ODataRoute("/Order")]
    public IHttpActionResult GetOrder(int id)
    {
        ......
    }
}
{% endhighlight %}

Now, Web API OData supports to put multiple `ODataRoutePrefixAttribute` on the same OData controller. For example, 

{% highlight csharp %}
[ODataRoutePrefix("Customers({key})")]  
[ODataRoutePrefix("VipCustomer")]  
public class ODataControllerWithMultiplePrefixes : ODataController  
{
    ......  
}
{% endhighlight %}

### Route template

The route template is the route combined with `ODataRoutePrefixAttribute` and `ODataRouteAttribute`. So, for the following example:

{% highlight csharp %}
[ODataRoutePrefix("Customers")]  
public class MyController : ODataController  
{
    [ODataRoute("({id})/Address")]
    public IHttpActionResult GetAddress(int id)
    {
        ......
    }
}
{% endhighlight %}

The `GetAddress` matches to `Customers({id})/Address` route template. It's called key template because there's a template `{id}`. So far in Web API OData, it supports two kind of templates:

1. key template, for example: 

{% highlight csharp %}
[ODataRoute("({id})/Address")]
[ODataRoute("Clients({clientId})/MyOrders({orderId})/OrderLines")]
{% endhighlight %}    
   
2. function parameter template, for example: 

{% highlight csharp %}
[ODataRoute("Customers({id})/NS.MyFunction(city={city})")]
[ODataRoute("Customers/Default.BoundFunction(SimpleEnum={p1})")]
{% endhighlight %}    

Web API OData team also works to add the third template, that is the dynamic property template. It's planed to ship in next release.

You can refer to [this blog](http://www.asp.net/web-api/overview/web-api-routing-and-actions/attribute-routing-in-web-api-2) for attribute routing in Web API 2.

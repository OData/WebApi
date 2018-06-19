---
layout: post
title: "4.3 Nested $filter in $expand"
description: ""
category: "4. OData features"
---

[OData Web API](https://github.com/OData/WebApi) v[5.5](https://www.nuget.org/packages/Microsoft.AspNet.OData/5.5.0-beta) supports nested $filter in $expand, e.g.:
`.../Customers?$expand=Orders($filter=Id eq 10)`

POCO classes:
{% highlight csharp %}
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public IEnumerable<Order> Orders { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string Name { get; set; }
}
{% endhighlight %}

With Edm model built as follows:
{% highlight csharp %}
var builder = new ODataConventionModelBuilder(config);
builder.EntitySet<Customer>("Customers");
var model = builder.GetEdmModel();
{% endhighlight %}

To Map route,
- For Microsoft.AspNet.OData, e.g., in `WebApiConfig.cs`:
{% highlight csharp %}
config.MapODataServiceRoute("orest", "orest", model);
{% endhighlight %}

- For Microsoft.AsnNetCore.OData, e.g., in `Startup.Configure((IApplicationBuilder app, IHostingEnvironment env)` method:
{% highlight csharp %}
app.UseMvc(routeBuilder => 
    {
        routeBuilder.Select().Expand().Filter().OrderBy().MaxTop(null).Count();
        routeBuilder.MapODataServiceRoute("orest", "orest", model);
    });
{% endhighlight %}

Controller:
{% highlight csharp %}
public class CustomersController : ODataController
{
    private Customer[] _customers =
    {
        new Customer
        {
            Id = 0,
            Name = "abc",
            Orders = new[]
            {
                new Order { Id = 10, Name = "xyz" },
                new Order { Id = 11, Name = "def" },
            }
        }
    };

    [EnableQuery]
    public IHttpActionResult Get()
    {
        return Ok(_customers.AsQueryable());
    }
}
{% endhighlight %}

Request:
`http://localhost:port_number/orest/Customers?$expand=Orders($filter=Id eq 10)`

Response:
{% highlight json %}
{
    "@odata.context": "http://localhost:52953/orest/$metadata#Customers",
    "value": [
        {
            "Id": 0,
            "Name": "abc",
            "Orders": [
                {
                    "Id": 10,
                    "Name": "xyz"
                }
            ]
        }
    ]
}
{% endhighlight %}

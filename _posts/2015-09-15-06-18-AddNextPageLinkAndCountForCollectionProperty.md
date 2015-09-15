---
layout: post
title: "4.18 Add NextPageLink and $count for collection property"
description: ""
category: "4. OData Features"
---

In OData WebApi V5.7, it supports to add the NextPageLink and $count for collection property.

### Enable NextPageLink and $count

It's easy to enable the NextPageLink and $count for collection property in controller. Users can only put the [EnableQuery(PageSize=x)] on the action of the controller.
For example:
{% highlight csharp %}
[EnableQuery(PageSize = 2)]  
public IHttpActionResult GetColors(int key)  
{  
  IList<Color> colors = new[] {Color.Blue, Color.Green, Color.Red};  
  return Ok(colors);
}  
{% endhighlight %}

### Sample Requests & Response

Request: <strong>GET</strong> http://localhost/Customers(5)/Colors?$count=true

Response content:
{% highlight csharp %}
{  
  "@odata.context":"http://localhost/$metadata#Collection(NS.Color)",
  "@odata.count": 3,  
  "@odata.nextLink":"http://localhost/Customers(5)/Colors?$count=true&$skip=2",
  "value": [  
    ""Blue",  
    ""Green"  
  ]  
} 
{% endhighlight %}

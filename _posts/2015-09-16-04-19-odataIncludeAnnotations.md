---
layout: post
title: "4.19 Prefer odata.include-annotations"
description: ""
category: "4. OData features"
---

Since OData WebApi V5.6, it supports <strong>[odata.include-annotations](http://docs.oasis-open.org/odata/odata/v4.0/errata02/os/complete/part1-protocol/odata-v4.0-errata02-os-part1-protocol-complete.html#_Toc406398237)</strong>.

### odata.include-annotations

It supports the following four templates:

1. odata.include-annotations="*"  // all annotations
2. odata.include-annotations="-*"  // no annotations
3. odata.include-annotations="display.*" // only annotations under "display" namespace
4. odata.include-annotations="display.subject" // only annotation with term name "display.subject"

Let's have examples:

#### odata.include-annotations=*

We can use the following codes to request all annotations:
{% highlight csharp %}
HttpRequestMessage request = new HttpRequestMessage(...);
request.Headers.Add("Prefer", "odata.include-annotations=*");
HttpResponseMessage response = client.SendAsync(request).Result;
...
{% endhighlight %}

The response will have all annotations:

{% highlight csharp %}
{  
  "@odata.context":"http://localhost:8081/$metadata#People/$entity",
  "@odata.id":"http://localhost:8081/People(2)",
  "Entry.GuidAnnotation@odata.type":"#Guid",
  "@Entry.GuidAnnotation":"a6e07eac-ad49-4bf7-a06e-203ff4d4b0d8",
  "@Hello.World":"Hello World.",
  "PerId":2,
  "Property.BirthdayAnnotation@odata.type":"#DateTimeOffset",
  "@Property.BirthdayAnnotation":"2010-01-02T00:00:00+08:00",
  "Age":10,
  "MyGuid":"f99080c0-2f9e-472e-8c72-1a8ecd9f902d",
  "Name":"Asha",
  "FavoriteColor":"Red, Green",
  "Order":{  
    "OrderAmount":235342,"OrderName":"FirstOrder"  
  }  
}
{% endhighlight %}

#### odata.include-annotations=Entry.*

We can use the following codes to request specify annotations:

{% highlight csharp %}
HttpRequestMessage request = new HttpRequestMessage(...);
request.Headers.Add("Prefer", "odata.include-annotations=Entry.*");
HttpResponseMessage response = client.SendAsync(request).Result;
...
{% endhighlight %}

The response will only have annotations in "Entry" namespace:

{% highlight csharp %}
{  
  "@odata.context":"http://localhost:8081/$metadata#People/$entity",
  "@odata.id":"http://localhost:8081/People(2)",
  "Entry.GuidAnnotation@odata.type":"#Guid",
  "@Entry.GuidAnnotation":"a6e07eac-ad49-4bf7-a06e-203ff4d4b0d8",
  "PerId":2,
  "Age":10,
  "MyGuid":"f99080c0-2f9e-472e-8c72-1a8ecd9f902d",
  "Name":"Asha",
  "FavoriteColor":"Red, Green",
  "Order":{  
    "OrderAmount":235342,"OrderName":"FirstOrder"  
  }  
} 
{% endhighlight %}
  

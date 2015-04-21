---
title : "6.1 Custom URL parsing"
layout: post
category: "6. Customization"
permalink: "/case-insensitive"
---

Let's show how to extend the default OData Uri Parser behavior:

### Basic Case Insensitive Support
User can configure as below to support basic case-insensitive parser behavior.

{% highlight csharp %}
HttpConfiguration config = …
config.EnableCaseInsensitive(caseInsensitive: true);
config.MapODataServiceRoute("odata", "odata", edmModel);
{% endhighlight %}
**Note**: Case insensitive flag enables both for metadata and key-words, not only on path segment, but also on query option.

For example:

* ~/odata/$metaDaTa
* ~/odata/cusTomers
...

### Unqualified function/action call
User can configure as below to support basic unqualified function/action call. 

{% highlight csharp %}
HttpConfiguration config = …
config.EnableUnqualifiedNameCall(unqualifiedNameCall: true);
config.MapODataServiceRoute("odata", "odata", edmModel);
{% endhighlight %}

For example:

Original call:
* ~/odata/Customers(112)/Default.GetOrdersCount(factor=1)

Now, you can call as:
* ~/odata/Customers(112)/GetOrdersCount(factor=1)

#### Enum prefix free
User can configure as below to support basic string as enum parser behavior.

{% highlight csharp %}
HttpConfiguration config = …
config.EnableEnumPrefixFree(enumPrefixFree: true);
config.MapODataServiceRoute("odata", "odata", edmModel);
{% endhighlight %}

For example:

Origin call:
{% highlight csharp %}
* ~/odata/Customers/Default.GetCustomerByGender(gender=System.Web.OData.TestCommon.Models.Gender'Male')
{% endhighlight %}
Now, you can call as:
{% highlight csharp %}
* ~/odata/Customers/Default.GetCustomerByGender(gender='Male')
{% endhighlight %}
#### Advance Usage
User can configure as below to support case insensitive & unqualified function call & Enum Prefix free:

{% highlight csharp %}
HttpConfiguration config = …
config.EnableCaseInsensitive(caseInsensitive: true);
config.EnableUnqualifiedNameCall(unqualifiedNameCall: true);
config.EnableEnumPrefixFree(enumPrefixFree: true);

config.MapODataServiceRoute("odata", "odata", edmModel);
{% endhighlight %}

Thanks.

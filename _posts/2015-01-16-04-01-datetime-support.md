---
title : "4.1 DateTime support"
layout: post
category: "4. OData features"
permalink: "/datetime-support"
---

This sample will introduce how to support *DateTime* type in Web API OData V4.

### Build **DateTime** Type
OData V4 doesn't include DateTime as primitive type. Web API OData V4 uses DateTimeOffset to represent the DateTime.
For example, if user defines a model as:
{% highlight csharp %}
public class Customer
{
    public int Id { get; set; }

    public DateTime Birthday { get; set; }
}
{% endhighlight %}

The metadata document for *Customer* entity type will be:
{% highlight xml %}
<EntityType Name="Customer">
    <Key>
        <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="Edm.Int32" Nullable="false" />
    <Property Name="Birthday" Type="Edm.DateTimeOffset" Nullable="false" />
</EntityType>
{% endhighlight %}

#### Time Zone Configuration
By Default, converting between DateTimeOffset and DateTime will lose the Time Zone information. Therefore, Web API provides a API to config the Time Zone information on server side. For example:

{% highlight csharp %}
HttpConfiguration configuration = ...
TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"); // -8:00
configuration.SetTimeZoneInfo(timeZoneInfo);
{% endhighlight %}

### $filter **DateTime** 

Since Web API OData 5.6, it supports to filter on DateTime type. For example:
{% highlight csharp %}
GET ~/Customers?$filter=Birthday lt cast(2015-04-01T04:11:31%2B08:00,Edm.DateTimeOffset)
GET ~/Customers?$filter=year(Birthday) eq 2010
{% endhighlight %}

### $orderby **DateTime** 

Since Web API OData 5.6, it supports to orderby on DateTime type. For example:
{% highlight csharp %}
GET ~/Customers?$orderby=Birthday
GET ~/Customers?$orderby=Birthday desc
{% endhighlight %}

Thanks.

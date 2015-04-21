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

Thanks.

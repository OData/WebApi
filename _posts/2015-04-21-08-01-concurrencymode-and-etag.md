---
layout: post
title: "8.1 ConcurrencyMode and ETag"
description: ""
category: "8. V1-3 specific features"
permalink: "/concurrency-and-etag"
---

In OData V3 protocol, concurrencyMode is a special facet that can be applied to any primitive Entity Data Model (EDM) type. Possible values are `None`, which is the default, and `Fixed`. When used on an EntityType property, `ConcurrencyMode` specifies that the value of that declared property should be used for optimistic concurrency checks. In the metadata, concurrencyMode will be shown as following:

{% highlight xml %}
<EntityType Name="Order">
    <Key>
        <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="Edm.Int32" Nullable="false" />
    <Property Name="ConcurrencyCheckRow" Type="Edm.String" Nullable="false" ConcurrencyMode="Fixed" /> />  
</EntityType>
{% endhighlight %}

There are two approaches to set concurrencyMode for a primitive property:
Using ConcurrencyCheck attribute:

{% highlight csharp %}
public class Order
{
    public int Id { get; set; }
    
    [ConcurrencyCheck]
    public string ConcurrencyCheckRow { get; set; }
}
{% endhighlight %}

Using API call

{% highlight csharp %}
ODataModelBuilder builder = new ODataModelBuilder();
builder.Entity<Order>().Property(c => c.ConcurrencyCheckRow).IsConcurrencyToken;
{% endhighlight %}

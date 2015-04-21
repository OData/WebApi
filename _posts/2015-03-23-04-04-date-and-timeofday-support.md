---
title : "4.4 Edm.Date and Edm.TimeOfDay"
layout: post
category: "4. OData features"
---

This sample introduces how to use the `Edm.Date` & `Edm.TimeOfDay` supported in Web API OData V5.5.

### Build Edm Model
[ODL V6.8](http://www.nuget.org/packages/Microsoft.OData.Core/6.8.0) introduces two new primitive types. One is `Edm.Date`, the other is `Edm.TimeOfDay`. Besides, it also introduces two new **struct** types to represent the CLR types of Edm.Date and Edm.TimeOfDay.
 So, developers can use the new CLR struct types to define their CLR model.
For example, if user defines a model as:

{% highlight csharp %}
using Microsoft.OData.Edm.Library;
public class Customer
{
    public int Id { get; set; }

    public DateTimeOffset Birthday { get; set; }
    
    public Date Publish { get; set; }
    
    public TimeOfDay CheckTime{ get; set;}
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
    <Property Name="Publish" Type="Edm.Date" Nullable="false"/>
    <Property Name="CheckTime" Type="Edm.TimeOfDay" Nullable="false"/>
</EntityType>
{% endhighlight %}

### Build-in Functions

Along with the `Edm.Date` & `Edm.TimeOfDay`, new date and time related built-in functions are supported in Web API OData V5.5.

Here's the list:

* Date
  - Edm.Int32 year(Edm.Date)
  - Edm.Int32 month(Edm.Date)
  - Edm.Int32 day(Edm.Date)

* TimeOfDay
  - Edm.Int32 hour(Edm.TimeOfDay)
  - Edm.Int32 minute(Edm.TimeOfDay)
  - Edm.Int32 second(Edm.TimeOfDay)
  - Edm.Decimal fractionalseconds(Edm.TimeOfDay)

* DateTimeOffset
  - Edm.Decimal fractionalseconds(Edm.DateTimeOffset)
  - Edm.Date date(Edm.DateTimeOffset)
  - Edm.TimeOfDay time(Edm.DateTimeOffset)

### Query examples 
Let's show some query request examples:

* Date
  - ~/odata/Customers?$filter=year(Publish) eq 2015
  - ~/odata/Customers?$filter=month(Publish) ne 11
  - ~/odata/Customers?$filter=day(Publish) lt 8

* TimeOfDay
  - ~/odata/Customers?$filter=hour(CheckTime) eq 2
  - ~/odata/Customers?$filter=minute(CheckTime) ge 11
  - ~/odata/Customers?$filter=second(CheckTime) lt 18
  - ~/odata/Customers?$filter=fractionalseconds(CheckTime) eq 0.04

* DateTimeOffset
  - ~/odata/Customers?$filter=fractionalseconds(Birthday) lt 0.04
  - ~/odata/Customers?$filter=date(Birthday) lt 2015-03-23
  - ~/odata/Customers?$filter=time(Birthday) eq 03:04:05.90100

Thanks.

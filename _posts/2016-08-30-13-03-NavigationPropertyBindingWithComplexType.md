---
layout: post
title : "13.3 Multiple Navigation property binding path"
description: "Navigation property binding with complex and containment"
category: "13. 6.x Features "
---

Since [Web API OData V6.0.0 beta](https://www.nuget.org/packages/Microsoft.AspNet.OData/6.0.0-beta2), It supports to configure multiple navigation property binding paths.

### Original navigation property binding

The following APIs are used to add navigation property binding in model builder.

```C#
1. HasManyBinding()
2. HasRequiredBinding()
3. HasOptionalBinding()
```

So, we can do as:

{% highlight csharp %}

public class Customer
{
  public int Id { get; set; }
  
  public Order SingleOrder {get;set;}  // single navigation property
  
  public IList<Order> Orders {get;set;} // collection navigation property
}

public class Order
{
  public int Id { get; set; }
}

ODataModelBuilder builder = new ODataModelBuilder();
var customers = builder.EntitySet<Cusomter>("Customers");
customers.HasManyBinding(c => c.Orders, "Orders");
customers.HasRequiredBinding(c => c.SingleOrder, "SingleOrders");

{% endhighlight %}

We can get the following result:

```xml
<EntityContainer Name="Container">
  <EntitySet Name="Customers" EntityType="System.Web.OData.Builder.Cusomter">
    <NavigationPropertyBinding Path="Orders" Target="Orders" />
    <NavigationPropertyBinding Path="SingleOrder" Target="SingleOrders" />
  </EntitySet>
  <EntitySet Name="Orders" EntityType="System.Web.OData.Builder.Order" />
  <EntitySet Name="SingleOrders" EntityType="System.Web.OData.Builder.Order" />
</EntityContainer>
```

Where, the binding path is straight-forward, just as the navigation property name.
Before 6.0.0, it doesn't support to:

1. Add the complex property into binding path
2. Add the type cast into binding path


### Multiple navigation property binding path in model builder

In [Web API OData V6.0.0 beta](https://www.nuget.org/packages/Microsoft.AspNet.OData/6.0.0-beta2), we add a new generic type class to configure the multiple binding path:

{% highlight csharp %}

BindingPathConfiguration{TStructuralType}

{% endhighlight %}

In this class, it provides the following APIs to add binding path:

{% highlight csharp %}

1. HasManyPath()
2. HasSinglePath()

{% endhighlight %}

It also provides the following APIs to bind the navigation property with multiple binding path to a targe navigation source.

{% highlight csharp %}

1. HasManyBinding()
2. HasRequiredBinding()
3. HasOptionalBinding()

{% endhighlight %}

So, the normal navigation property binding configuration flow is：

1. Call `Binding` from `NavigationSourceConfiguration{TEntityType}` to get an instance of BindingPathConfiguration{TEntityType}
2. Call `HasManyPath()/HasSinglePath` from BindingPathConfiguration{TEntityType}` to add a binding path
3. Repeat step-2 if necessary
4. Call `HasManyBinding()/HasRequiredBinding()/HasOptionalBinding()` to add the target navigation source.

Here's an example:

{% highlight csharp %}

builder.EntitySet<Customer>("Customers")
   .Binding
   .HasManyPath(c => c.Locations)
   .HasSinglePath(a => a.LocationInfo)
   .HasRequiredBinding(c => c.City, "Cities");
   
{% endhighlight %}

### Example

Let's have the following CLR classes as the model:

#### Entity types

{% highlight csharp %}

public class Cusomter
{
  public int Id { get; set; }

  public Animal Pet { get; set; }
}

public class VipCustomer : Cusomter
{
  public List<Address> VipLocations { get; set; }
}

public class City
{
  public int Id { get; set; }
}

{% endhighlight %}

#### Complex types

{% highlight csharp %}

public class Animal
{
}

public class Human : Animal
{
    public UsAddress HumanAddress { get; set; }
}

public class Horse : Animal
{
    public UsAddress HorseAddress { get; set; }
    public IList<UsAddress> HorseAddresses { get; set; }
}
public class Address
{
  public City City { get; set; }
}

public class UsAddress : Address
{
  public City SubCity { get; set; }
}

{% endhighlight %}

#### Add navigation property binding:

{% highlight csharp %}

customers.HasManyPath((VipCustomer v) => v.VipLocations).HasRequiredBinding(a => a.City, "A");
customers.HasManyPath((VipCustomer v) => v.VipLocations).HasRequiredBinding((UsAddress a) => a.SubCity, "B");

var pet = customers.HasRequiredPath(c => c.Pet);

pet.HasRequiredPath((Human p) => p.HumanAddress)
    .HasRequiredBinding(c => c.SubCity, "HumanCities");

pet.HasRequiredPath((Horse h) => h.HorseAddress).HasRequiredBinding(c => c.SubCity, "HorseCities");

pet.HasManyPath((Horse h) => h.HorseAddresses).HasRequiredBinding(c => c.SubCity, "HorseCities");
  
{% endhighlight %}

So, we can get the following target binding:

```xml
<Schema Namespace="Default" xmlns="http://docs.oasis-open.org/odata/ns/edm">
  <EntityContainer Name="Container">
    <EntitySet Name="Customers" EntityType="System.Web.OData.Builder.Cusomter">
      <NavigationPropertyBinding Path="Pet/System.Web.OData.Builder.Human/HumanAddress/SubCity" Target="HumanCities" />
      <NavigationPropertyBinding Path="Pet/System.Web.OData.Builder.Horse/HorseAddress/SubCity" Target="HorseCities" />
      <NavigationPropertyBinding Path="Pet/System.Web.OData.Builder.Horse/HorseAddresses/SubCity" Target="HorseCities" />
      <NavigationPropertyBinding Path="System.Web.OData.Builder.VipCustomer/VipLocations/System.Web.OData.Builder.UsAddress/SubCity" Target="B" />
      <NavigationPropertyBinding Path="System.Web.OData.Builder.VipCustomer/VipLocations/City" Target="A" />
    </EntitySet>
    <EntitySet Name="A" EntityType="System.Web.OData.Builder.City" />
    <EntitySet Name="B" EntityType="System.Web.OData.Builder.City" />
    <EntitySet Name="HumanCities" EntityType="System.Web.OData.Builder.City" />
    <EntitySet Name="HorseCities" EntityType="System.Web.OData.Builder.City" />
  </EntityContainer>
</Schema>
```

Where: As example shown:

1. It supports single property with multiple binding path.
2. It supports collection property with mutliple binding path.
3. It also supports mulitple binding path with inheritance type.

Besides, the `HasManyPath()/HasSinglePath()` has onverload methods to configure the containment binding path. 


### Multiple navigation property binding path in convention model builder

In convention model builder, it will automatically traverl all property binding paths to add a binding for the navigation proeprty.

There's an [unit test](https://github.com/OData/WebApi/blob/OData60/OData/test/UnitTest/System.Web.OData.Test/OData/MetadataControllerTest.cs#L1070-L1114) that you can refer to. 


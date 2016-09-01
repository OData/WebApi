---
layout: post
title: "13.2 Complex Type with Navigation Property"
description: "Build Navigation Property on Complex Type"
category: "13. 6.x Features "
---

Since [Web API OData V6.0.0 beta](https://www.nuget.org/packages/Microsoft.AspNet.OData/6.0.0-beta2), It supports to configure navigation property on complex type.

Let's have an example to illustrate how to configure navigation property on complex type:

### CLR Model

We use the following CRL classes as the CLR model:

```C#
public class Address
{
  public City CityInfo { get; set; }
  public IList<City> Cities { get; set}
}

public class City
{
  public int Id { get; set; }
}

```

Where:

* **Address** is a complex type.
* **City** is an entity type.

### Add navigation properties

The following APIs are used to add navigation properties for complex type:

```C#
1. HasMany()
2. HasRequired()
3. HasOptional()
```

So, we can do as:

```C#
ODataModelBuilder builder = new ODataModelBuilder();
builder.EntityType<City>().HasKey(c => c.Id);
var address = builder.ComplexType<Address>();
address.HasRequired(a => a.CityInfo);
address.HasMany(a => a.Cities);
```

We can get the following result:

```xml
<ComplexType Name="Address">
  <NavigationProperty Name="CityInfo" Type="ModelLibrary.City" Nullable="false" />"
  <NavigationProperty Name="Cities" Type="Collection(NS.City)" />"
</ComplexType>
<EntityType Name="City">
  <Key>
    <PropertyRef Name="Id" />
  </Key>
  <Property Name="Id" Type="Edm.Int32" Nullable="false" />
</EntityType>
```

### Add navigation properties in convention model builder

Convention model builder will automatically map the class type properties in complex type as navigation properties if the declaring type of such navigation property has key defined. 

So, as the above example, we can use the following codes to define a convention model:
```C#
ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
builder.ComplexType<Address>(); // just add a starting point
```

As result, We can get the following result:

```xml

<edmx:Edmx Version="4.0" xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx">
  <edmx:DataServices>
    <Schema Namespace="ModelLibrary" xmlns="http://docs.oasis-open.org/odata/ns/edm">
      <ComplexType Name="Address">
        <NavigationProperty Name="CityInfo" Type="ModelLibrary.City" />
        <NavigationProperty Name="Cities" Type="Collection(ModelLibrary.City)" />
      </ComplexType>
      <EntityType Name="City">
        <Key>
          <PropertyRef Name="Id" />
        </Key>
        <Property Name="Id" Type="Edm.Int32" Nullable="false" />
      </EntityType>
    </Schema>
    <Schema Namespace="Default" xmlns="http://docs.oasis-open.org/odata/ns/edm">
      <EntityContainer Name="Container" />
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>

```

---
layout: post
title: "2.3 Non-convention model builder"
description: "convention model builder"
category: "2. Defining the model"
---

To build an Edm model using non-convention model builder is to create an `IEdmModel` object by directly call fluent APIs of `ODataModelBuilder`. The developer should take all responsibility to add all Edm types, operations, associations, etc into the data model one by one.
Let's see how to build the Ccustomer-Order* business model by `ODataModelBuilder`.

### CLR Models

Non-convention model builder is based on CLR classes to build the Edm Model. The *Customer-Order* business CLR classes are present in abstract section.

### Enum Type

The following codes are used to add an Enum type **`Color`**:
{% highlight csharp %}
var color = builder.EnumType<Color>();
color.Member(Color.Red);
color.Member(Color.Blue);
color.Member(Color.Green);
{% endhighlight %}

It will generate the below metadata document:
{% highlight xml %}
<EnumType Name="Color">
   <Member Name="Red" Value="0" />
   <Member Name="Blue" Value="1" />
   <Member Name="Green" Value="2" />
</EnumType>
{% endhighlight %}

### Complex Type

#### Basic Complex Type
The following codes are used to add a complex type **`Address`**:
{% highlight csharp %}
var address = builder.ComplexType<Address>();
address.Property(a => a.Country);
address.Property(a => a.City);
{% endhighlight %}

It will generate the below metadata document:
{% highlight xml %}
<ComplexType Name="Address">
  <Property Name="Country" Type="Edm.String" />
  <Property Name="City" Type="Edm.String" />
</ComplexType>
{% endhighlight %}

#### Derived Complex type

The following codes are used to add a derived complex type **`SubAddress`**:
{% highlight csharp %}
var subAddress = builder.ComplexType<SubAddress>().DerivesFrom<Address>();
subAddress.Property(s => s.Street);
{% endhighlight %}

It will generate the below metadata document:
{% highlight xml %}
<ComplexType Name="SubAddress" BaseType="WebApiDocNS.Address">
  <Property Name="Street" Type="Edm.String" />
</ComplexType>
{% endhighlight %}

#### Abstract Complex type

The following codes are used to add an abstract complex type:
{% highlight csharp %}
builder.ComplexType<Address>().Abstract();
......
{% endhighlight %}

It will generate the below metadata document:
{% highlight xml %}
<ComplexType Name="Address" Abstract="true">
  ......
</ComplexType>
{% endhighlight %}

#### Open Complex type

In order to build an open complex type, you should change the CLR class by adding an `IDictionary<string, object>` property, the property name can be any name. For example:

{% highlight csharp %}
public class Address
{
    public string Country { get; set; }
    public string City { get; set; }
    public IDictionary<string, object> Dynamics { get; set; }
}
{% endhighlight %}

Then you can build the open complex type by call `HasDynamicProperties()`:
{% highlight csharp %}
var address = builder.ComplexType<Address>();
address.Property(a => a.Country);
address.Property(a => a.City);
address.HasDynamicProperties(a => a.Dynamics);
{% endhighlight %}

It will generate the below metadata document:
{% highlight xml %}
<ComplexType Name="Address" OpenType="true">
  <Property Name="Country" Type="Edm.String" />
  <Property Name="City" Type="Edm.String" />
</ComplexType>
{% endhighlight %}
You can find that the complex type **`Address`** only has two properties, while it has `OpenType="true"` attribute.

### Entity Type

#### Basic Entity Type

The following codes are used to add two entity types **`Customer` & `Order`**:
{% highlight csharp %}
var customer = builder.EntityType<Customer>();
customer.HasKey(c => c.CustomerId);
customer.ComplexProperty(c => c.Location);
customer.HasMany(c => c.Orders);

var order = builder.EntityType<Order>();
order.HasKey(o => o.OrderId);
order.Property(o => o.Token);
{% endhighlight %}

It will generate the below metadata document:
{% highlight xml %}
<EntityType Name="Customer">
    <Key>
        <PropertyRef Name="CustomerId" />
    </Key>
    <Property Name="CustomerId" Type="Edm.Int32" Nullable="false" />
    <Property Name="Location" Type="WebApiDocNS.Address" Nullable="false" />
    <NavigationProperty Name="Orders" Type="Collection(WebApiDocNS.Order)" />
</EntityType>
<EntityType Name="Order">
    <Key>
        <PropertyRef Name="OrderId" />
    </Key>
    <Property Name="OrderId" Type="Edm.Int32" Nullable="false" />
    <Property Name="Token" Type="Edm.Guid" Nullable="false" />
</EntityType>
{% endhighlight %}

#### Abstract Open type

The following codes are used to add an abstract entity type:
{% highlight csharp %}
builder.EntityType<Customer>().Abstract();
......
{% endhighlight %}

It will generate the below metadata document:
{% highlight xml %}
<EntityType Name="Customer" Abstract="true">
  ......
</EntityType>
{% endhighlight %}


#### Open Entity type

In order to build an open entity type, you should change the CLR class by adding an `IDictionary<string, object>` property, while the property name can be any name. For example:

{% highlight csharp %}
public class Customer
{
    public int CustomerId { get; set; }
    public Address Location { get; set; }
    public IList<Order> Orders { get; set; }
    public IDictionary<string, object> Dynamics { get; set; }
}
{% endhighlight %}

Then you can build the open entity type as:
{% highlight csharp %}
var customer = builder.EntityType<Customer>();
customer.HasKey(c => c.CustomerId);
customer.ComplexProperty(c => c.Location);
customer.HasMany(c => c.Orders);
customer.HasDynamicProperties(c => c.Dynamics);
{% endhighlight %}

It will generate the below metadata document:
{% highlight xml %}
<EntityType Name="Customer" OpenType="true">
    <Key>
        <PropertyRef Name="CustomerId" />
    </Key>
    <Property Name="CustomerId" Type="Edm.Int32" Nullable="false" />
    <Property Name="Location" Type="WebApiDocNS.Address" Nullable="false" />
    <NavigationProperty Name="Orders" Type="Collection(WebApiDocNS.Order)" />
</EntityType>
{% endhighlight %}
You can find that the entity type **`Customer`** only has three properties, while it has `OpenType="true"` attribute.

### Entity Container

Non-convention model builder will build the default entity container automatically. However, you should build your own entity sets as:
{% highlight csharp %}
builder.EntitySet<Customer>("Customers");
builder.EntitySet<Order>("Orders");
{% endhighlight %}

It will generate the below metadata document:
{% highlight xml %}
<Schema Namespace="Default" xmlns="http://docs.oasis-open.org/odata/ns/edm">
    <EntityContainer Name="Container">
        <EntitySet Name="Customers" EntityType="WebApiDocNS.Customer">
          <NavigationPropertyBinding Path="Orders" Target="Orders" />
        </EntitySet>
        <EntitySet Name="Orders" EntityType="WebApiDocNS.Order" />
    </EntityContainer>
</Schema>
{% endhighlight %}

Besides, you can call `Singleton<T>()` to add singleton into entity container.

### Function

It's very simple to build **function (bound & unbound)** in Web API OData. The following codes define two functions. The first is bind to **`Customer`**, the second is unbound.
{% highlight csharp %}
var function = customer.Function("BoundFunction").Returns<string>();
function.Parameter<int>("value");
function.Parameter<Address>("address");

function = builder.Function("UnBoundFunction").Returns<int>();
function.Parameter<Color>("color");
function.EntityParameter<Order>("order");
{% endhighlight %}

It will generate the below metadata document:
{% highlight xml %}
<Function Name="BoundFunction" IsBound="true">
   <Parameter Name="bindingParameter" Type="WebApiDocNS.Customer" />
   <Parameter Name="value" Type="Edm.Int32" Nullable="false" />
   <Parameter Name="address" Type="WebApiDocNS.Address" />
   <ReturnType Type="Edm.String" Unicode="false" />
</Function>
<Function Name="UnBoundFunction">
   <Parameter Name="color" Type="WebApiDocNS.Color" Nullable="false" />
   <Parameter Name="order" Type="WebApiDocNS.Order" />
   <ReturnType Type="Edm.Int32" Nullable="false" />
</Function>
{% endhighlight %}

Besides, Web API OData will automatically add function imports for all unbound functions. So, the metadata document should has:
{% highlight xml %}
<FunctionImport Name="UnBoundFunction" Function="Default.UnBoundFunction" IncludeInServiceDocument="true" />
{% endhighlight %}

### Action

Same as function, it's also very simple to build **action (bound & unbound)** in Web API OData. The following codes define two actions. The first is bind to collection of **`Customer`**, the second is unbound.
{% highlight csharp %}
var action = customer.Collection.Action("BoundAction");
action.Parameter<int>("value");
action.CollectionParameter<Address>("addresses");

action = builder.Action("UnBoundAction").Returns<int>();
action.Parameter<Color>("color");
action.CollectionEntityParameter<Order>("orders");
{% endhighlight %}

It will generate the below metadata document:
{% highlight xml %}
<Action Name="BoundAction" IsBound="true">
    <Parameter Name="bindingParameter" Type="Collection(WebApiDocNS.Customer)" />
    <Parameter Name="value" Type="Edm.Int32" Nullable="false" />
    <Parameter Name="addresses" Type="Collection(WebApiDocNS.Address)" />
</Action>
<Action Name="UnBoundAction">
    <Parameter Name="color" Type="WebApiDocNS.Color" Nullable="false" />
    <Parameter Name="orders" Type="Collection(WebApiDocNS.Order)" />
    <ReturnType Type="Edm.Int32" Nullable="false" />
</Action>
{% endhighlight %}

Same as function, Web API OData will automatically add action imports for all unbound actions. So, the metadata document should has:
{% highlight xml %}
 <ActionImport Name="UnBoundAction" Action="Default.UnBoundAction" />
{% endhighlight %}

### Summary

Let's put all codes together:
{% highlight csharp %}
public static IEdmModel GetEdmModel()
{
    var builder = new ODataModelBuilder();

    // enum type
    var color = builder.EnumType<Color>();
    color.Member(Color.Red);
    color.Member(Color.Blue);
    color.Member(Color.Green);

    // complex type
    // var address = builder.ComplexType<Address>().Abstract();
    var address = builder.ComplexType<Address>();
    address.Property(a => a.Country);
    address.Property(a => a.City);
    // address.HasDynamicProperties(a => a.Dynamics);

    var subAddress = builder.ComplexType<SubAddress>().DerivesFrom<Address>();
    subAddress.Property(s => s.Street);

    // entity type
    // var customer = builder.EntityType<Customer>().Abstract();
    var customer = builder.EntityType<Customer>();
    customer.HasKey(c => c.CustomerId);
    customer.ComplexProperty(c => c.Location);
    customer.HasMany(c => c.Orders);
    // customer.HasDynamicProperties(c => c.Dynamics);

    var order = builder.EntityType<Order>();
    order.HasKey(o => o.OrderId);
    order.Property(o => o.Token);

    // entity set
    builder.EntitySet<Customer>("Customers");
    builder.EntitySet<Order>("Orders");

    // function
    var function = customer.Function("BoundFunction").Returns<string>();
    function.Parameter<int>("value");
    function.Parameter<Address>("address");

    function = builder.Function("UnBoundFunction").Returns<int>();
    function.Parameter<Color>("color");
    function.EntityParameter<Order>("order");

    // action
    var action = customer.Collection.Action("BoundAction");
    action.Parameter<int>("value");
    action.CollectionParameter<Address>("addresses");

    action = builder.Action("UnBoundAction").Returns<int>();
    action.Parameter<Color>("color");
    action.CollectionEntityParameter<Order>("orders");

    return builder.GetEdmModel();
}
{% endhighlight %}

And the final XML will be:
{% highlight xml %}
<?xml version="1.0" encoding="utf-8"?>
<edmx:Edmx Version="4.0" xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx">
  <edmx:DataServices>
    <Schema Namespace="WebApiDocNS" xmlns="http://docs.oasis-open.org/odata/ns/edm">
      <ComplexType Name="Address">
        <Property Name="Country" Type="Edm.String" />
        <Property Name="City" Type="Edm.String" />
      </ComplexType>
      <ComplexType Name="SubAddress" BaseType="WebApiDocNS.Address">
        <Property Name="Street" Type="Edm.String" />
      </ComplexType>
      <EntityType Name="Customer" OpenType="true">
        <Key>
          <PropertyRef Name="CustomerId" />
        </Key>
        <Property Name="CustomerId" Type="Edm.Int32" Nullable="false" />
        <Property Name="Location" Type="WebApiDocNS.Address" Nullable="false" />
        <NavigationProperty Name="Orders" Type="Collection(WebApiDocNS.Order)" />
      </EntityType>
      <EntityType Name="Order">
        <Key>
          <PropertyRef Name="OrderId" />
        </Key>
        <Property Name="OrderId" Type="Edm.Int32" Nullable="false" />
        <Property Name="Token" Type="Edm.Guid" Nullable="false" />
      </EntityType>
      <EnumType Name="Color">
        <Member Name="Red" Value="0" />
        <Member Name="Blue" Value="1" />
        <Member Name="Green" Value="2" />
      </EnumType>
    </Schema>
    <Schema Namespace="Default" xmlns="http://docs.oasis-open.org/odata/ns/edm">
      <Function Name="BoundFunction" IsBound="true">
        <Parameter Name="bindingParameter" Type="WebApiDocNS.Customer" />
        <Parameter Name="value" Type="Edm.Int32" Nullable="false" />
        <Parameter Name="address" Type="WebApiDocNS.Address" />
        <ReturnType Type="Edm.String" Unicode="false" />
      </Function>
      <Function Name="UnBoundFunction">
        <Parameter Name="color" Type="WebApiDocNS.Color" Nullable="false" />
        <Parameter Name="order" Type="WebApiDocNS.Order" />
        <ReturnType Type="Edm.Int32" Nullable="false" />
      </Function>
      <Action Name="BoundAction" IsBound="true">
        <Parameter Name="bindingParameter" Type="Collection(WebApiDocNS.Customer)" />
        <Parameter Name="value" Type="Edm.Int32" Nullable="false" />
        <Parameter Name="addresses" Type="Collection(WebApiDocNS.Address)" />
      </Action>
      <Action Name="UnBoundAction">
        <Parameter Name="color" Type="WebApiDocNS.Color" Nullable="false" />
        <Parameter Name="orders" Type="Collection(WebApiDocNS.Order)" />
        <ReturnType Type="Edm.Int32" Nullable="false" />
      </Action>
      <EntityContainer Name="Container">
        <EntitySet Name="Customers" EntityType="WebApiDocNS.Customer">
          <NavigationPropertyBinding Path="Orders" Target="Orders" />
        </EntitySet>
        <EntitySet Name="Orders" EntityType="WebApiDocNS.Order" />
        <FunctionImport Name="UnBoundFunction" Function="Default.UnBoundFunction" IncludeInServiceDocument="true" />
        <ActionImport Name="UnBoundAction" Action="Default.UnBoundAction" />
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>
{% endhighlight %}

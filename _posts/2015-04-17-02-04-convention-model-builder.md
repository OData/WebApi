---
layout: post
title: "2.4 Convention model builder"
description: "convention model builder"
category: "2. Defining the model"
---

In the previous two sections, we walk you through the required aspects to build an Edm model by directly using **[ODatalib](https://www.nuget.org/packages/Microsoft.OData.Core/)** or leveraging `ODataModelBuilder` fluent API in WebApi OData. 

Obvious, there are many codes you should add to develop a simple *Customer-Order* business model. However, Web API OData also provides a simple method by using `ODataConventionModelBuilder` to do the same thing. It's called **convention model builder** and can extremely reduce your workload.

Convention model builder uses a set of pre-defined rules (called *conventions*) to help model builder identify Edm types, keys, associations, relationships, etc automatically, and build them into the final Edm data model.

In this section, we will go through all conventions used in convention model builder. First, let's see how to build the Edm model using `ODataConventionModelBuilder`.

### CLR Models

We also use the *Customer-Order* business model presented in abstract section.

### Build the Edm Model

The following codes can add all related entity types, complex types, enum type and the corresponding entity sets into the Edm model:
{% highlight csharp %}
public static IEdmModel GetConventionModel()
{
   var builder = new ODataConventionModelBuilder();
   builder.EntitySet<Customer>("Customers");
   builder.EntitySet<Order>("Orders");

   return builder.GetEdmModel();
}
{% endhighlight %}

It will generate the below metadata document:
{% highlight xml %}
<?xml version="1.0" encoding="utf-8"?>
<edmx:Edmx Version="4.0" xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx">
  <edmx:DataServices>
    <Schema Namespace="WebApiDocNS" xmlns="http://docs.oasis-open.org/odata/ns/edm">
      <EntityType Name="Customer" OpenType="true">
        <Key>
          <PropertyRef Name="CustomerId" />
        </Key>
        <Property Name="CustomerId" Type="Edm.Int32" Nullable="false" />
        <Property Name="Location" Type="WebApiDocNS.Address" />
        <NavigationProperty Name="Orders" Type="Collection(WebApiDocNS.Order)" />
      </EntityType>
      <EntityType Name="Order">
        <Key>
          <PropertyRef Name="OrderId" />
        </Key>
        <Property Name="OrderId" Type="Edm.Int32" Nullable="false" />
        <Property Name="Token" Type="Edm.Guid" Nullable="false" />
      </EntityType>
      <ComplexType Name="Address" OpenType="true">
        <Property Name="Country" Type="Edm.String" />
        <Property Name="City" Type="Edm.String" />
      </ComplexType>
      <ComplexType Name="SubAddress" BaseType="WebApiDocNS.Address" OpenType="true">
        <Property Name="Street" Type="Edm.String" />
      </ComplexType>
      <EntityType Name="VipCustomer" BaseType="WebApiDocNS.Customer" OpenType="true">
        <Property Name="FavoriteColor" Type="WebApiDocNS.Color" Nullable="false" />
      </EntityType>
      <EnumType Name="Color">
        <Member Name="Red" Value="0" />
        <Member Name="Blue" Value="1" />
        <Member Name="Green" Value="2" />
      </EnumType>
    </Schema>
    <Schema Namespace="Default" xmlns="http://docs.oasis-open.org/odata/ns/edm">
      <EntityContainer Name="Container">
        <EntitySet Name="Customers" EntityType="WebApiDocNS.Customer">
          <NavigationPropertyBinding Path="Orders" Target="Orders" />
        </EntitySet>
        <EntitySet Name="Orders" EntityType="WebApiDocNS.Order" />
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>
{% endhighlight %}

**Note**: We omit the **function/action** building because it's same as non-convention model builder.

### Conventions

Wow, how the convention model builder do that! Actually, convention model builder uses a set of **pre-defined** rules (called *conventions*) to achieve this. 
If you open the source code for [`ODataConventionModelBuilder`](https://github.com/OData/WebApi/blob/master/OData/src/System.Web.OData/OData/Builder/ODataConventionModelBuilder.cs), You can find the following codes at the beginning of the `ODataConventionModelBuilder` class:
{% highlight csharp %}
private static readonly List<IConvention> _conventions = new List<IConvention>
{
	// type and property conventions (ordering is important here).
	new AbstractTypeDiscoveryConvention(),
	new DataContractAttributeEdmTypeConvention(),
	new NotMappedAttributeConvention(), // NotMappedAttributeConvention has to run before EntityKeyConvention
	new DataMemberAttributeEdmPropertyConvention(),
	new RequiredAttributeEdmPropertyConvention(),
	new ConcurrencyCheckAttributeEdmPropertyConvention(),
	new TimestampAttributeEdmPropertyConvention(),
	new KeyAttributeEdmPropertyConvention(), // KeyAttributeEdmPropertyConvention has to run before EntityKeyConvention
	new EntityKeyConvention(),
	new ComplexTypeAttributeConvention(), // This has to run after Key conventions, basically overrules them if there is a ComplexTypeAttribute
	new IgnoreDataMemberAttributeEdmPropertyConvention(),
	new NotFilterableAttributeEdmPropertyConvention(),
	new NonFilterableAttributeEdmPropertyConvention(),
	new NotSortableAttributeEdmPropertyConvention(),
	new UnsortableAttributeEdmPropertyConvention(),
	new NotNavigableAttributeEdmPropertyConvention(),
	new NotExpandableAttributeEdmPropertyConvention(),
	new NotCountableAttributeEdmPropertyConvention(),

	// INavigationSourceConvention's
	new SelfLinksGenerationConvention(),
	new NavigationLinksGenerationConvention(),
	new AssociationSetDiscoveryConvention(),

	// IEdmFunctionImportConventions's
	new ActionLinkGenerationConvention(),
	new FunctionLinkGenerationConvention(),
};
{% endhighlight %}
Where lists the conventions wrapped in convention model builder. However, in `ODataConventionModelBuilder`, there are some conventions which can't be clearly listed. Let's walk you through these conventions one by one with some relevant attributes & annotations to illustrate the convention model builder.

#### Type Inheritance Identify Convention

Rule: Only derived types can be walked.

For example:
{% highlight csharp %}
public class Base
{
    public int Id { get; set; }
}

public class Derived : Base
{
}
{% endhighlight %}

By using convention builder:
{% highlight csharp %}
public static IEdmModel GetEdmModel()
{
    ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
    builder.EntityType<Base>();
    return builder.GetEdmModel()
}
{% endhighlight %}
It will generate the below entity type in the resulted EDM document:
{% highlight xml %}
<EntityType Name="Base">
  <Key>
    <PropertyRef Name="Id" />
  </Key>
  <Property Name="Id" Type="Edm.Int32" Nullable="false" />
</EntityType>
<EntityType Name="Derived" BaseType="WebApiDocNS.Base" />
{% endhighlight %}

If you change the model builder as:

{% highlight csharp %}
public static IEdmModel GetEdmModel()
{
    ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
    builder.EntityType<Derived>();
    return builder.GetEdmModel()
}
{% endhighlight %}
It will generate the below entity type in the resulted EDM document:
{% highlight xml %}
<EntityType Name="Derived">
  <Key>
    <PropertyRef Name="Id" />
  </Key>
  <Property Name="Id" Type="Edm.Int32" Nullable="false" />
</EntityType>
{% endhighlight %}
There is no `Base` entity type existed.

#### Abstract type convention

Rule: The Edm type will be marked as abstract if the class is abstract.

{% highlight csharp %}
public abstract class Base
{
    public int Id { get; set; }
}
{% endhighlight %}

The result is :

{% highlight xml %}
<EntityType Name="Base" Abstract="true">
   <Key>
     <PropertyRef Name="Id" />
   </Key>
   <Property Name="Id" Type="Edm.Int32" Nullable="false" />
</EntityType>
{% endhighlight %}

#### Entity key convention

Rule: If one and only one property’s name is ‘Id’ or ‘`<entity class name>`Id’ (case insensitive), it becomes entity key property.

{% highlight csharp %}
public abstract class Base
{
    public int BaseId { get; set; }
}
{% endhighlight %}

The result is:
{% highlight xml %}
<EntityType Name="Base" Abstract="true">
  <Key>
    <PropertyRef Name="BaseId" />
  </Key>
  <Property Name="BaseId" Type="Edm.Int32" Nullable="false" />
</EntityType>
{% endhighlight %}

#### Key attribute

Rule: The [`KeyAttribute`] specifies key property, it forces a property without 'id' to be Key property. It'll suppress the entity key convention.

{% highlight csharp %}
public class Trip
{
    [Key]
    public int TripNum { get; set; }	
    public int Id { get; set; }
}
{% endhighlight %}

The result is:
{% highlight xml %}
<EntityType Name="Trip">
  <Key>
    <PropertyRef Name="TripNum" />
  </Key>
  <Property Name="TripNum" Type="Edm.Int32" Nullable="false" />
  <Property Name="Id" Type="Edm.Int32" Nullable="false" />
</EntityType>
{% endhighlight %}

#### ComplexType Attribute Convention

Rule-1: Create a class without any ‘id’ or ‘<class name>id’ or [`KeyAttribute`] property, like

{% highlight csharp %}
public class City
{
    public string Name { get; set; }
    public string CountryRegion { get; set; }
    public string Region { get; set; }
}
{% endhighlight %}

Rule-2:	Add [ComplexType] attribute to a model class: it will remove ‘id’ or ‘<class name>id’ or [Key] properties, the model class will have no entity key, thus becomes a complex type.

{% highlight csharp %}
[ComplexType]
public class PairItem
{
    public int Id { get; set; }
    public string Value { get; set; }
}
{% endhighlight %}

Then, the above two types wil be built as `Complex Type`.

#### DataContract & DataMember
Rule: If using DataContract or DataMember, only property with [DataMember] attribute will be added into Edm model.

{% highlight csharp %}
[DataContract]
public class Trip
{
    [DataMember]
    [Key]
    public int TripNum { get; set; }
    public Guid? ShareId { get; set; }  // will be eliminated
    [DataMember]
    public string Name { get; set; }
}
{% endhighlight %}

The resulted EDM document is:

{% highlight xml %}
<EntityType Name="Trip">
  <Key>
	<PropertyRef Name="TripNum" />
  </Key>
  <Property Name="TripNum" Type="Edm.Int32" Nullable="false" />
  <Property Name="Name" Type="Edm.String" />
</EntityType>
{% endhighlight %}

You can also change name-space and property name in EDM document. For example, if the above DataContract attribute is added with NameSpace:

{% highlight csharp %}
[DataContract(Namespace="My.NewNameSpace")]
public class Trip
{ 
   ...
}
{% endhighlight %}

The result will become:

{% highlight csharp %}
<Schema Namespace="My.NewNameSpace">
  <EntityType Name="Trip">
	<Key>
	  <PropertyRef Name="TripNum"/>
	</Key>
	<Property Name="TripNum" Type="Edm.Int32" Nullable="false"/>
	<Property Name="Name" Type="Edm.String"/>
  </EntityType>
</Schema>
{% endhighlight %}

#### NotMapped Attribute Convention

Rule: [NotMapped] deselects the property to be serialized or deserialized, so to some extent, it can be seen as the converse of DataContract & DataMember.

For example, the above if `Trip` class is changed to the below, it generates exactly the same Trip Entity in EDM document, that is, no ‘SharedId’ property.

{% highlight csharp %}
[DataContract]
public class Trip
{
    [DataMember]
    [Key]
    public int TripNum { get; set; }
    [NotMapped]
    public Guid? ShareId { get; set; }
    [DataMember]
    public string Name { get; set; }
}	
{% endhighlight %}

The result is:
{% highlight xml %}

<EntityType Name="Trip">
  <Key>
	<PropertyRef Name="TripNum"/>
  </Key>
  <Property Name="TripNum" Type="Edm.Int32" Nullable="false"/>
  <Property Name="Name" Type="Edm.String"/>
</EntityType>
{% endhighlight %}

#### Required Attribute Convention

Rule: The property with [Required] attribute will be non-nullable. 

{% highlight csharp %}

public class Trip
{
    [Key]
    public int TripNum { get; set; }
    [NotMapped]
    public Guid? ShareId { get; set; }
    [Required]
    public string Name { get; set; }
}
{% endhighlight %}

Then the result has `Nullable=”false”` for `Name` property:

{% highlight xml %}
<EntityType Name="Trip">
  <Key>
	<PropertyRef Name="TripNum"/>
  </Key>
  <Property Name="TripNum" Type="Edm.Int32" Nullable="false"/>
  <Property Name="Name" Type="Edm.String" Nullable="false"/>
</EntityType>
{% endhighlight %}

#### ConcurrencyCheck Attribute Convention
Rule: It can mark one or more properties for doing optimistic concurrency check on entity updates.

{% highlight csharp %}

public class Trip
{
    [Key]
    public int TripNum { get; set; }
    public int Id { get; set; }
    [ConcurrencyCheck]
    public string UpdateVersion { get; set; }
}
{% endhighlight %}

The expected result should be like the below:

{% highlight xml %}
<EntityType Name="Trip">
    <Key>
        <PropertyRef Name="TripNum" />
    </Key>
    <Property Name="TripNum" Type="Edm.Int32" Nullable="false" />
    <Property Name="Id" Type="Edm.Int32" Nullable="false" />
    <Property Name="UpdateVersion" Type="Edm.String" ConcurrencyMode="Fixed" />
</EntityType>
{% endhighlight %}

#### Timestamp Attribute Convention
Rule: It's same as [ConcurrencyCheck].

{% highlight csharp %}

public class Trip
{
    [Key]
    public int TripNum { get; set; }
    public int Id { get; set; }
    [Timestamp]
    public string UpdateVersion { get; set; }
}
{% endhighlight %}

The expected result should be like the below:

{% highlight xml %}
<EntityType Name="Trip">
    <Key>
        <PropertyRef Name="TripNum" />
    </Key>
    <Property Name="TripNum" Type="Edm.Int32" Nullable="false" />
    <Property Name="Id" Type="Edm.Int32" Nullable="false" />
    <Property Name="UpdateVersion" Type="Edm.String" ConcurrencyMode="Fixed" />
</EntityType>
{% endhighlight %}


#### IgnoreDataMember Attribute Convention

Rule: It has the same effect as `[NotMapped]` attribute. It is able to revert the `[DataMember] `attribute on the property when the model class doesn’t have `[DataContract]` attribute.

#### NonFilterable & NotFilterable Attribute Convention
Rule: Property marked with [NonFilterable] or [NotFilterable] will not support `$filter` query option.
{% highlight csharp %}
public class QueryLimitCustomer
{
    public int Id { get; set; }

    [NonFilterable]
    [NotFilterable]
    public string Title { get; set; }
}
{% endhighlight %}

Then, if you issue an query option as:
`~/odata/Customers?$filter=Title eq 'abc'`

You will get the following exception:
{% highlight csharp %}
The query specified in the URI is not valid. The property 'Title' cannot be used in the $filter query option.
{% endhighlight %}

#### NotSortable & Unsortable Attribute Convention
Rule: Property marked with [NotSortable] or [Unsortable] will not support `$orderby` query option.
{% highlight csharp %}
public class QueryLimitCustomer
{
    public int Id { get; set; }

    [NotSortable]
    [Unsortable]	
    public string Title { get; set; }
}
{% endhighlight %}

Then, if you issue an query option as:
`~/odata/Customers?$orderby=Title

You will get the following exception:
{% highlight csharp %}
The query specified in the URI is not valid. The property 'Title' cannot be used in the $orderby query option.
{% endhighlight %}

#### NotNavigable Attribute Convention
Rule: Property marked with [NotNavigable] will not support `$select` query option.
{% highlight csharp %}
public class QueryLimitCustomer
{
    public int Id { get; set; }

    [NotNavigable]	
    public Address address { get; set; }
}
{% endhighlight %}

Then, if you issue an query option as:
`~/odata/Customers?$select=address

You will get the following exception:
{% highlight csharp %}
The query specified in the URI is not valid. The property 'address' cannot be used for navigation."
{% endhighlight %}

#### NotExpandable Attribute Convention
Rule: Property marked with [NotExpandable] will not support `$expand` query option.
{% highlight csharp %}
public class QueryLimitCustomer
{
    public int Id { get; set; }

    [NotExpandable]	
    public ICollection<QueryLimitOrder> Orders { get; set; }
}
{% endhighlight %}

Then, if you issue an query option as:
`~/odata/Customers?$expand=Orders

You will get the following exception:
{% highlight csharp %}
The query specified in the URI is not valid. The property 'Orders' cannot be used in the $expand query option.
{% endhighlight %}

#### NotCountable Attribute Convention
Rule: Property marked with [NotCountable] will not support `$count` query option.
{% highlight csharp %}
public class QueryLimitCustomer
{
    public int Id { get; set; }

    [NotCountable]	
    public ICollection<Address> Addresses { get; set; }
}
{% endhighlight %}

Then, if you issue an query option as:
`~/odata/Customers(1)/Addresses?$count=true

You will get the following exception:
{% highlight csharp %}
The query specified in the URI is not valid. The property 'Addresses' cannot be used for $count.
{% endhighlight %}

#### ForeignKey Attribute Convention

Rule: Property marked with [ForeignKey] will be used to build referential constraint.

{% highlight csharp %}
public class ForeignCustomer
{
    public int ForeignCustomerId { get; set; }
    public int OtherCustomerKey { get; set; }
    public IList<ForeignOrder> Orders { get; set; }
}

public class ForeignOrder
{
    public int ForeignOrderId { get; set; }
    public int CustomerId { get; set; }

    [ForeignKey("CustomerId")]
    public ForeignCustomer Customer { get; set; }
}
{% endhighlight %}

You will get the following result:
{% highlight xml %}
<EntityType Name="ForeignOrder">
<Key>
  <PropertyRef Name="ForeignOrderId" />
</Key>
<Property Name="ForeignOrderId" Type="Edm.Int32" Nullable="false" />
<Property Name="CustomerId" Type="Edm.Int32" />
<NavigationProperty Name="Customer" Type="WebApiDocNS.ForeignCustomer">
  <ReferentialConstraint Property="CustomerId" ReferencedProperty="ForeignCustomerId" />
</NavigationProperty>
</EntityType>
{% endhighlight %}

[ForeignKey] can also put on dependent property. for example:
{% highlight csharp %}
public class ForeignOrder
{
    public int ForeignOrderId { get; set; }
	
    [ForeignKey("Customer")]
    public int CustomerId { get; set; }
    public ForeignCustomer Customer { get; set; }
}
{% endhighlight %}

It'll get the same result.

#### ActionOnDelete Attribute Convention

Rule: Property marked with [ActionOnDelete] will be used to build referential constraint action on delete.

{% highlight csharp %}
public class ForeignCustomer
{
    public int ForeignCustomerId { get; set; }
    public int OtherCustomerKey { get; set; }
    public IList<ForeignOrder> Orders { get; set; }
}

public class ForeignOrder
{
    public int ForeignOrderId { get; set; }
    public int CustomerId { get; set; }

    [ForeignKey("CustomerId")]
    [ActionOnDelete(EdmOnDeleteAction.Cascade)]
    public ForeignCustomer Customer { get; set; }
}
{% endhighlight %}

You will get the following result:
{% highlight xml %}
<EntityType Name="ForeignOrder">
<Key>
  <PropertyRef Name="ForeignOrderId" />
</Key>
<Property Name="ForeignOrderId" Type="Edm.Int32" Nullable="false" />
<Property Name="CustomerId" Type="Edm.Int32" />
<NavigationProperty Name="Customer" Type="WebApiDocNS.ForeignCustomer">
  <OnDelete Action="Cascade" />
  <ReferentialConstraint Property="CustomerId" ReferencedProperty="ForeignCustomerId" />
</NavigationProperty>
</EntityType>
{% endhighlight %}


#### ForeignKeyDiscovery Convention

Rule: A convention used to discover foreign key properties if there is no any foreign key configured on the navigation property.
      The basic rule to discover the foreign key is: with the same property type and follow up the naming convention. 
      The naming convention is:
      1. The **"Principal class name + principal key name"** equals the **dependent property name**
          For example: Customer (Id) <--> Order (CustomerId)
      2. or the **"Principal key name"** equals the **dependent property name**.
          For example: Customer (CustomerId) <--> Order (CustomerId)
		  
{% highlight csharp %}  
public class PrincipalEntity
{
    public string Id { get; set; }
}

public class DependentEntity
{
    public int Id { get; set; }

    public string PrincipalEntityId { get; set; }
    public PrincipalEntity Principal { get; set; }
}		  
{% endhighlight %}

You will get the following result:
{% highlight xml %}
<EntityType Name="PrincipalEntity">
<Key>
  <PropertyRef Name="Id" />
</Key>
<Property Name="Id" Type="Edm.String" Nullable="false" />
</EntityType>
<EntityType Name="DependentEntity">
<Key>
  <PropertyRef Name="Id" />
</Key>
<Property Name="Id" Type="Edm.Int32" Nullable="false" />
<Property Name="PrincipalEntityId" Type="Edm.String" />
<NavigationProperty Name="Principal" Type="WebApiDocNS.PrincipalEntity">

  <ReferentialConstraint Property="PrincipalEntityId" ReferencedProperty="Id" />
</NavigationProperty>
</EntityType>
{% endhighlight %}	

  


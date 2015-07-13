---
title : "4.14 Capabilities vocabulary support"
layout: post
category: "4. OData features"
---

Web API OData supports some query limitations, for example:
• NonFilterable / NotFilterable -- $filter
• NotCountable -- $count
• NotExpandable -- $expand
• NotNavigable -- $select
• NotSortable / Unsortable -- $orderby

However, the corresponding annotations cannot be exposed in metadata document. This sample introduces the capabilities vocabulary support in Web API OData V5.7. 
You can find the sample codes [here](https://github.com/OData/ODataSamples/tree/master/WebApi/v4/ODataCapabilitiesVocabularySample).


### Build Edm Model

Let's define a model with query limitations:

{% highlight csharp %}
public class Customer
{
	public int CustomerId { get; set; }

	public string Name { get; set; }

	[NotFilterable]
	[NotSortable]
	public Guid Token { get; set; }

	[NotNavigable]
	public string Email { get; set; }

	[NotCountable]
	public IList<Address> Addresses { get; set; }

	[NotCountable]
	public IList<Color> FavoriateColors { get; set; }

	[NotExpandable]
	public IEnumerable<Order> Orders { get; set; }
}
{% endhighlight %}

Where, `Address` is a normal complex type, `Color` is an enum type and `Order` is a normal entity type. You can find their definition from the sample codes.

Based on the above CLR classes, we can build the Edm model as:
{% highlight csharp %}
private static IEdmModel GetEdmModel()
{
	var builder = new ODataConventionModelBuilder();
	builder.EntitySet<Customer>("Customers");
	builder.EntitySet<Order>("Orders");
	return builder.GetEdmModel();
}
{% endhighlight %}

### Expose annotations

Now, you query the metadata document for *Customer* entity type as:

{% highlight xml %}
<?xml version="1.0" encoding="utf-8"?>
<edmx:Edmx Version="4.0" xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx">
  <edmx:DataServices>
    <Schema Namespace="CapabilitiesVocabulary" xmlns="http://docs.oasis-open.org/odata/ns/edm">
      <EntityType Name="Customer">
        <Key>
          <PropertyRef Name="CustomerId" />
        </Key>
        <Property Name="CustomerId" Type="Edm.Int32" Nullable="false" />
        <Property Name="Name" Type="Edm.String" />
        <Property Name="Token" Type="Edm.Guid" Nullable="false" />
        <Property Name="Email" Type="Edm.String" />
        <Property Name="Addresses" Type="Collection(CapabilitiesVocabulary.Address)" />
        <Property Name="FavoriateColors" Type="Collection(CapabilitiesVocabulary.Color)" Nullable="false" />
        <NavigationProperty Name="Orders" Type="Collection(CapabilitiesVocabulary.Order)" />
      </EntityType>
      <EntityType Name="Order">
        <Key>
          <PropertyRef Name="OrderId" />
        </Key>
        <Property Name="OrderId" Type="Edm.Int32" Nullable="false" />
        <Property Name="Price" Type="Edm.Double" Nullable="false" />
      </EntityType>
      <ComplexType Name="Address">
        <Property Name="City" Type="Edm.String" />
        <Property Name="Street" Type="Edm.String" />
      </ComplexType>
      <EnumType Name="Color">
        <Member Name="Red" Value="0" />
        <Member Name="Green" Value="1" />
        <Member Name="Blue" Value="2" />
        <Member Name="Yellow" Value="3" />
        <Member Name="Pink" Value="4" />
        <Member Name="Purple" Value="5" />
      </EnumType>
    </Schema>
    <Schema Namespace="Default" xmlns="http://docs.oasis-open.org/odata/ns/edm">
      <EntityContainer Name="Container">
        <EntitySet Name="Customers" EntityType="CapabilitiesVocabulary.Customer">
          <NavigationPropertyBinding Path="Orders" Target="Orders" />
          <Annotation Term="Org.OData.Capabilities.V1.CountRestrictions">
            <Record>
              <PropertyValue Property="Countable" Bool="true" />
              <PropertyValue Property="NonCountableProperties">
                <Collection>
                  <PropertyPath>Addresses</PropertyPath>
                  <PropertyPath>FavoriateColors</PropertyPath>
                </Collection>
              </PropertyValue>
              <PropertyValue Property="NonCountableNavigationProperties">
                <Collection />
              </PropertyValue>
            </Record>
          </Annotation>
          <Annotation Term="Org.OData.Capabilities.V1.FilterRestrictions">
            <Record>
              <PropertyValue Property="Filterable" Bool="true" />
              <PropertyValue Property="RequiresFilter" Bool="true" />
              <PropertyValue Property="RequiredProperties">
                <Collection />
              </PropertyValue>
              <PropertyValue Property="NonFilterableProperties">
                <Collection>
                  <PropertyPath>Token</PropertyPath>
                </Collection>
              </PropertyValue>
            </Record>
          </Annotation>
          <Annotation Term="Org.OData.Capabilities.V1.SortRestrictions">
            <Record>
              <PropertyValue Property="Sortable" Bool="true" />
              <PropertyValue Property="AscendingOnlyProperties">
                <Collection />
              </PropertyValue>
              <PropertyValue Property="DescendingOnlyProperties">
                <Collection />
              </PropertyValue>
              <PropertyValue Property="NonSortableProperties">
                <Collection>
                  <PropertyPath>Token</PropertyPath>
                </Collection>
              </PropertyValue>
            </Record>
          </Annotation>
          <Annotation Term="Org.OData.Capabilities.V1.ExpandRestrictions">
            <Record>
              <PropertyValue Property="Expandable" Bool="true" />
              <PropertyValue Property="NonExpandableProperties">
                <Collection>
                  <NavigationPropertyPath>Orders</NavigationPropertyPath>
                </Collection>
              </PropertyValue>
            </Record>
          </Annotation>
        </EntitySet>
        <EntitySet Name="Orders" EntityType="CapabilitiesVocabulary.Order" />
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>
{% endhighlight %}

Thanks.

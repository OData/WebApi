---
title : "4.26 ModelBound Attribute"
layout: post
category: "4. OData features"
---

Since [Web API OData V6.0.0 beta](https://www.nuget.org/packages/Microsoft.AspNet.OData/6.0.0-beta2) which depends on [OData Lib 7.0.0](https://www.nuget.org/packages/Microsoft.OData.Core/7.0.0), we add a new feature named ModelBoundAttribute, use this feature, we can control the query setting through those attributes to make our service more secure and even control the query result by set page size, automatic select, automatic expand.

Let's see how to use this feature.

### Global Query Setting

Now the default setting for WebAPI OData is : client can't apply `$count`, `$orderby`, `$select`, `$top`, `$expand`, `$filter` in the query, query like `localhost\odata\Customers?$orderby=Name` will failed as BadRequest, because all properties are not sort-able by default, this is a breaking change in 6.0.0, if we want to use the default behavior that all query option are enabled in 5.x version, we can configure the HttpConfigration to enable the query option we want like this:

{% highlight csharp %}
//...
configuration.Count().Filter().OrderBy().Expand().Select().MaxTop(null);
configuration.MapODataServiceRoute("odata", "odata", edmModel);
//...
{% endhighlight %}

### Page Attribute

Pagination settings correlate to OData's @odata.nextLink (server-side pagination) and `?$top=5&$skip=5` (client-side pagination).
We can set the PageSize to control the server-side pagination, and MaxTop to control the maximum value for `$top`, by default client can't use `$top` as we said in the `Global Query Setting` section, every query option is disabled, if you set the `Page` Attribute,  by default it will enable the `$top` with no-limit maximum value, or you can set the MaxTop like:

{% highlight csharp %}
[Page(MaxTop = 5, PageSize = 1)]
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Order Order { get; set; }
    public Address Address { get; set; }
    [Page(MaxTop = 2, PageSize = 1)]
    public List<Order> Orders { get; set; }
    public List<Address> Addresses { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Price { get; set; }
    [Page]
    public List<Customer> Customers { get; set; }
}
{% endhighlight %}

In the model above, we defined the page setting for Customer and Orders navigation property in Customer and Customers navigation property in Order, let's explain the usage of them one by one.

#### Page Attribute on Entity Type

The first page attribute on Customer type, means the query setting when we query the Customer type, like `localhost\odata\Customers`, the max value for `$top` is 5 and page size of returned customers is 1. 

For example:

The query like `localhost\odata\Customers?$top=10` will failed with BadRequest : The limit of '5' for Top query has been exceeded.

The page size is 1 if you request `localhost\odata\Customers`.

#### Page Attribute on Navigation Property

And what about the page attribute in Order type's navigation property Customers? it means the query setting when we query the Customers navigation property in Order type. Now we get a query setting for Customer type and a query setting for Customers navigation property in Order type, how do we merge these two settings? The answer is: currently the property's query setting always override the type's query setting, if there is no query setting on property, it will inherent query setting from it's type.

For example:

The query like `localhost\odata\Orders?$expand=Customers($top=10)` will works because the setting is no limit.

The result of `localhost\odata\Orders?$expand=Customers` won't have paging because the setting didn't set the page size.

So for the attribute on Orders navigation property in Customer type, the page size and maximum value of `$top` setting will have effect when we request like `localhost\odata\Customers?$expand=Orders` or `localhost\odata\Customers(1)\Orders` as long as we are query the Orders property on Customer type.

### Count Attribute

Count settings correlate to OData's `?$count=true` (items + count).
We can set the entity type or property is countable or not like:

{% highlight csharp %}
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Address Address { get; set; }
    [Count]
    public List<Order> Orders { get; set; }
    public List<Address> Addresses { get; set; }
    public List<Address2> Addresses2 { get; set; }
    public List<Order> CountableOrders { get; set; }
}

[Count(Disabled = true)]
public class Order
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Price { get; set; }
}
{% endhighlight %}

In the model above, we can tell that the Order is not countable(maybe the number is very large) but Orders property in Customer is countable.

About the priority of attribute on property and type, please refer to `Page Attribute` section.

So you can have those examples:

Query `localhost\odata\Orders?$count=true` will failed with BadRequest that orders can't used for `$count`

Query `localhost\odata\Customers?$expand=Orders($count=true)` will works

Query `localhost\odata\Customers(1)/Orders?$count=true` works too.

### OrderBy Attribute

Ordering settings correlate to OData's `$orderby` query option.
We can specify which property is sort-able very easy and we can also define very complex rule by use attribute on property and on type. For example:

{% highlight csharp %}
[OrderBy("AutoExpandOrder", "Address")]
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Order AutoExpandOrder { get; set; }
    [OrderBy]
    public Address Address { get; set; }
    [OrderBy("Id")]
    public List<Order> Orders { get; set; }
}
    
[OrderBy("Name", Disabled = true)]
[OrderBy]
public class Order
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Price { get; set; }   
    [OrderBy]
    public List<Customer> Customers { get; set; }
    public List<Customer> UnSortableCustomers { get; set; }
    public List<Car> Cars { get; set; }
}

public class Address
{
    public string Name { get; set; }
    public string Street { get; set; }
}
{% endhighlight %}

#### Multiple Attribute

We can see that the we can have multiple OrderBy attributes, how are they merged? The answer is the Attribute on a class with a constrained set of properties gets high priority, the order of their appear time doesn't matter.

#### OrderBy Attribute on EntityType and ComplexType

Let's go through those attributes to understand the settings, the first attribute means we can specify the single navigation property `AutoExpandOrder` and single complex property `Address` when we query Customer type, like query `localhost\odata\Customers?$orderby=Address/xxx` or `localhost\odata\Customers?$orderby=AutoExpandOrder/xxx`. And how do we control which property under AutoExandOrder is sort-able?

For the AutoExpandOrder property, we add OrderBy Attribute on Order type, the first attribute means `Name` is not sort-able, the second attribute means all the property is sort-able, so for the Order type, properties except `Name` are sort-able.

For example:

Query `localhost\odata\Customers?$orderby=Name` will failed with BadRequest that Name is not sort-able.

Query `localhost\odata\Customers?$orderby=AutoExpandOrder/Price` works.

Query `localhost\odata\Customers?$orderby=AutoExpandOrder/Name` will failed with BadRequest that Name is not sort-able.

#### OrderBy Attribute on Property

About the priority of attribute on property and type, please refer to `Page Attribute` section. 
We have OrderBy attribute on Address property, it means all properties are sort-able when we query Customer, and for Orders property, it means only `Id` is sort-able when we query Orders property under Customer.

For example:

Query `localhost\odata\Customers?$orderby=Address/Name` works.

Query `localhost\odata\Customers?$expand=Orders($orderby=Id)` works.

Query `localhost\odata\Customers?$expand=Orders($orderby=Price)` will failed with BadRequest that Price is not sort-able.

### Filter Attribute

Filtering settings correlate to OData's `$filter` query option.
For now we only support to specify which property can be filter just like what we do in OrderBy Attribute, we can simply replace orderby with filter in the example above, so please refer to `OrderBy Attribute` section.

### Select Attribute

Search settings correlate to OData's `$search` query option.
We can specify which property can be selected, which property is automatic selected when there is no `$select` in the query.

#### Automatic Select

Automatic select mean we will add `$select` in the query depends on the select attribute.

If we have a User class, and we don't want to expose some property to client, like secrete property, so client query `localhost\odata\Users?$select=Secrete` will failed and query `localhost\odata\Users?` won't return Secrete property, how can we achieve that with Select Attribute?

{% highlight csharp %}
[Select(SelectType = SelectExpandType.Automatic)]
[Select("Secrete", SelectType = SelectExpandType.Disabled)]
public class User
{
    public int Id { get; set; }
    public string Secrete { get; set; }
    public string Name { get; set; }
}
{% endhighlight %}

The first attribute means all the property will be automatic select when there is no `$select` in the query, the second attribute means the property Secrete is not select-able. For example, request `localhost\odata\Users` will have the same response with `localhost\odata\Users?$select=Id,Name`

##### Automatic Select on Derived Type

If the target type of our request have some derived types which have automatic select property, then these property will show in the response if there is no `$select` query option, for example, request `localhost\odata\Users` will have the same response with `localhost\odata\Users?$select=Id,Name,SpecialUser/SpecialName` if the SpecinalName property in automatic select.

#### Select Attribute on Navigation Property

About the priority of attribute on property and type, please refer to `Page Attribute` section. 
About the multiple attribute, please refer to `Multiple Attribute` section. 
We also support Select attribute on navigation property, to control the expand scenario and property access scenario, like if we want client can only select `Id` and `Name` from Customer's navigation property Order.

{% highlight csharp %}
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    [Select("Id", "Name")]
    public Order Order { get; set; }
}
{% endhighlight %}

### Expand Attribute

Expansion settings correlate to OData's `$expand` query option.
We can specify which property can be expanded, which property is automatic expanded and we can specify the max depth of the expand property. Currently we support Expand attribute on entity type and navigation property, the using scenario is quite like Select Attribute and other attributes, you can just refer to those sections.

#### Automatic Expand

Automatic expand mean it will always expand that navigation property, it's like automatic select, we will add a $expand in the query, so it will expand even if there is a `$select` which does not contain automatic epxand property.

### Model Bound Fluent APIs

We also provide all fluent APIs to configure above attributes if you can't modify the class by adding attributes, it's very straight forward and simple to use:
 
 {% highlight csharp %}
[Expand("Orders", "Friend", "CountableOrders", MaxDepth = 10)]
[Expand("AutoExpandOrder", ExpandType = SelectExpandType.Automatic, MaxDepth = 8)]
[Page(MaxTop = 5, PageSize = 1)]
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    [Expand(ExpandType = SelectExpandType.Disabled)]
    public Order Order { get; set; }
    public Order AutoExpandOrder { get; set; }
    public Address Address { get; set; }
    [Expand("Customers", MaxDepth = 2)]
    [Count(Disabled = true)]
    [Page(MaxTop = 2, PageSize = 1)]
    public List<Order> Orders { get; set; }
    public List<Order> CountableOrders { get; set; }
    public List<Order> NoExpandOrders { get; set; }
    public List<Address> Addresses { get; set; }
    [Expand(MaxDepth = 2)]
    public Customer Friend { get; set; }
}

var builder = new ODataConventionModelBuilder();
builder.EntitySet<Customer>("Customers")
    .EntityType.Expand(10, "Orders", "Friend", "CountableOrders")
    .Expand(8, SelectExpandType.Automatic, "AutoExpandOrder")
    .Page(5, 2);
builder.EntityType<Customer>()
    .HasMany(p => p.Orders)
    .Expand(2, "Customers")
    .Page(2, 1)
    .Count(QueryOptionSetting.Disabled);
builder.EntityType<Customer>()
    .HasMany(p => p.CountableOrders)
    .Count();
builder.EntityType<Customer>()
    .HasOptional(p => p.Order)
    .Expand(SelectExpandType.Disabled);
{% endhighlight %}

The example shows class with attributes and build model using the model bound fluent APIs if we can't modify the class. These two approaches are getting two same models.
About the multiple attribute, model bound fluent APIs are the same, the model bound fluent API with a constrained set of properties wins. For example: `builder.EntityType<Customer>().Expand().Expand("Friend", SelectExpandType.Disabled)`, Friend can't be expanded, even we put `Expand()` in the end. If there is a setting with same property, the last one wins, for example: `.Expand(8, "Friend").Expand(1, "Friend")`, the max depth will be 1.

### Overall Query Setting Precedence

Query settings can be placed in many places, with the following precedence from lowest to highest: System Default(not query-able by default), Global Configuration, Model Bound Attribute, Fluent API.

### Controller Level Query Setting

If we only want to control the setting in one API call, like the `Get()` method in `CustomerController`, we can simply use the Settings in EnableQueryAttribute, like:

{% highlight csharp %}
[EnableQuery(MaxExpansionDepth = 10)]
public List<Customer> Get()
{
    return _customers;
}
{% endhighlight %}

The model bound attribute and the settings in EnableQueryAttribute are working separately, so the query violent one of them will fail the request.

More test case with more complex scenario can be find at [here](https://github.com/OData/WebApi/tree/OData60/OData/test/E2ETest/WebStack.QA.Test.OData/ModelBoundQuerySettings).

You can participate into discussions and ask questions about this feature at our [GitHub issues](https://github.com/OData/WebApi/issues), your feedback is very important for us.

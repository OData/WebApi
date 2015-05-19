---
title : "4.13 Delta Feed Support"
layout: post
category: "4. OData features"
permalink: "/DeltaFeed_support"
---

<h3>Serialization Support for Delta Feed</h3>
This sample will introduce how to create a <strong>[Delta Feed](http://docs.oasis-open.org/odata/odata-json-format/v4.0/errata02/os/odata-json-format-v4.0-errata02-os-complete.html#_Toc403940644)</strong> which is serialized into a Delta Response in Web API OData V4.

Similar to [`EdmEntityObjectCollection`](https://github.com/OData/WebApi/blob/master/OData/src/System.Web.OData/OData/EdmEntityCollectionObject.cs), [Web API OData V5.6](http://www.nuget.org/packages/Microsoft.AspNet.OData/5.6.0-beta1) now has an [`EdmChangedObjectCollection`](https://github.com/OData/WebApi/blob/master/OData/src/System.Web.OData/OData/EdmChangedObjectCollection.cs) to represent a collection of objects which can be a part of the <strong>Delta Feed</strong>.
A delta response can contain <i>[new/changed entities](http://docs.oasis-open.org/odata/odata-json-format/v4.0/errata02/os/odata-json-format-v4.0-errata02-os-complete.html#_Toc403940645)</i>, <i>[deleted entities](http://docs.oasis-open.org/odata/odata-json-format/v4.0/errata02/os/odata-json-format-v4.0-errata02-os-complete.html#_Toc403940646)</i>, <i>[new links](http://docs.oasis-open.org/odata/odata-json-format/v4.0/errata02/os/odata-json-format-v4.0-errata02-os-complete.html#_Toc403940647)</i> or <i>[deleted links](http://docs.oasis-open.org/odata/odata-json-format/v4.0/errata02/os/odata-json-format-v4.0-errata02-os-complete.html#_Toc403940648)</i>.

WebAPI OData V4 now has [`EdmDeltaEntityObject`](https://github.com/OData/WebApi/blob/master/OData/src/System.Web.OData/OData/EdmDeltaEntityObject.cs), [`EdmDeltaDeletedEntityObject`](https://github.com/OData/WebApi/blob/master/OData/src/System.Web.OData/OData/EdmDeltaDeletedEntityObject.cs), [`EdmDeltaLink`](https://github.com/OData/WebApi/blob/master/OData/src/System.Web.OData/OData/EdmDeltaLink.cs) and [`EdmDeltaDeletedLink`](https://github.com/OData/WebApi/blob/master/OData/src/System.Web.OData/OData/EdmDeltaDeletedLink.cs) respectively for the objects that can be a part of the Delta response.
All the above objects implement the [`IEdmChangedObject`](https://github.com/OData/WebApi/blob/master/OData/src/System.Web.OData/OData/IEdmChangedObject.cs) interface, while the `EdmChangedObjectCollection` is a collection of `IEdmChangedObject`.

For example, if user defines a model as:

{% highlight csharp %}
public class Customer
{
  public int Id { get; set; }
  public string Name { get; set; }
  public virtual IList<Order> Orders { get; set; }
}
public class Order
{
  public int Id { get; set; }
  public string ShippingAddress { get; set; }
}
private static IEdmModel GetEdmModel()
{
  ODataModelBuilder builder = new ODataConventionModelBuilder();
  var customers = builder.EntitySet<Customer>("Customers");
  var orders = builder.EntitySet<Order>("Orders");
  return builder.GetEdmModel();
}
{% endhighlight %}

The `EdmChangedObjectCollection` collection for Customer entity will be created as follows:
{% highlight csharp %}
EdmChangedObjectCollection changedCollection = new EdmChangedObjectCollection(CustomerType); //IEdmEntityType of Customer
{% endhighlight %}

Changed or Modified objects are added as `EdmDeltaEntityObject`s:
{% highlight csharp %}
EdmDeltaEntityObject Customer = new EdmDeltaEntityObject(CustomerType); 
Customer.Id = 123;
Customer.Name = "Added Customer";
changedCollection.Add(Customer);
{% endhighlight %}

Deleted objects are added as `EdmDeltaDeletedObject`s:
{% highlight csharp %}
EdmDeltaDeletedEntityObject Customer = new EdmDeltaDeletedEntityObject(CustomerType);
Customer.Id = 123;
Customer.Reason = DeltaDeletedEntryReason.Deleted;
changedCollection.Add(Customer);
{% endhighlight %}

Delta Link is added corresponding to a $expand in the initial request, these are added as `EdmDeltaLink`s:
{% highlight csharp %}
EdmDeltaLink CustomerOrderLink = new EdmDeltaLink(CustomerType);
CustomerOrderLink.Relationship = "Orders";
CustomerOrderLink.Source = new Uri(ServiceBaseUri, "Customers(123)");	
CustomerOrderLink.Target = new Uri(ServiceBaseUri, "Orders(10)");
changedCollection.Add(CustomerOrderLink);
{% endhighlight %}

Deleted Links is added for each deleted link that corresponds to a $expand path in the initial request, these are added as `EdmDeltaDeletedLink`s:
{% highlight csharp %}
EdmDeltaDeletedLink CustomerOrderDeletedLink = new EdmDeltaDeletedLink(CustomerType);
CustomerOrderDeletedLink.Relationship = "Orders";
CustomerOrderDeletedLink.Source = new Uri(ServiceBaseUri, "Customers(123)");
CustomerOrderDeletedLink.Target = new Uri(ServiceBaseUri, "Orders(10)");
changedCollection.Add(CustomerOrderDeletedLink);
{% endhighlight %}

<h4>Sample for Delta Feed</h4>
Let's create a controller to return a Delta Feed:
{% highlight csharp %}
public class CustomersController : ODataController
{
  public IHttpActionResult Get()
  {
    EdmChangedObjectCollection changedCollection = new EdmChangedObjectCollection(CustomerType);
    EdmDeltaEntityObject Customer = new EdmDeltaEntityObject(CustomerType); 
    Customer.Id = 123;
    Customer.Name = "Added Customer";
    changedCollection.Add(Customer);
    
    EdmDeltaDeletedEntityObject Customer = new EdmDeltaDeletedEntityObject(CustomerType);
    Customer.Id = 124;
    Customer.Reason = DeltaDeletedEntryReason.Deleted;
    changedCollection.Add(Customer);

    EdmDeltaLink CustomerOrderLink = new EdmDeltaLink(CustomerType);
    CustomerOrderLink.Relationship = "Orders";
    CustomerOrderLink.Source = new Uri(ServiceBaseUri, "Customers(123)");
    CustomerOrderLink.Target = new Uri(ServiceBaseUri, "Orders(10)");
    changedCollection.Add(CustomerOrderLink);
    
    EdmDeltaDeletedLink CustomerOrderDeletedLink = new EdmDeltaDeletedLink(CustomerType);
    CustomerOrderDeletedLink.Relationship = "Orders";
    CustomerOrderDeletedLink.Source = new Uri(ServiceBaseUri, "Customers(123)");
    CustomerOrderDeletedLink.Target = new Uri(ServiceBaseUri, "Orders(11)");
    changedCollection.Add(CustomerOrderDeletedLink);
    return Ok(changedCollection);
  }
}
{% endhighlight %}

Now, user can issue a <strong>GET</strong> request as:

{% highlight csharp %}
http://localhost/odata/Customers?$expand=Orders&$deltatoken=abc
{% endhighlight %}

The corresponding payload will has the following contents:
{% highlight json %}
{
  "@odata.context":"http://localhost/odata/$metadata#Customers",
  "value": [
    {
      "Id":123,
      "Name":"Added Customer"
    },
	  {
   	  "@odata.context":"http://localhost/odata/$metadata#Customers/$deletedEntity",
      "Id": 124
      "Reason":"Deleted"
    },
    {
      "@odata.context":"http://localhost/odata/$metadata#Customers/$link",
      "source":"Customers(123)",
      "relationship":"Orders",
      "target":"Orders(10)"
    },
    {
     	"@odata.context":"http://localhost/odata/$metadata#Customers/$deletedLink",
     	"source":"Customers(123)",
     	"relationship":"Orders",
     	"target":"Orders(11)"
    }
  ]
}
{% endhighlight %}

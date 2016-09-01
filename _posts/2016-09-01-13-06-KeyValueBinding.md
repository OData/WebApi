---
layout: post
title : "13.6 Key value binding "
description: "Key value binding"
category: "13. 6.x Features "
---

Since [Web API OData V6.0.0 beta](https://www.nuget.org/packages/Microsoft.AspNet.OData/6.0.0-beta2), Web API OData supports the composite key convention binding.

Let's have an example:
```C#
public class Customer
{
    public string StringProp { get; set; }
	
    public Date DateProp { get; set; }

    public Guid GuidProp { get; set; }
}
```	

Where, **Customer** is an entity type with three properties.
We will make all these trhee properties as the composite keys for **Customer** entity type.

So, we can do:

```C#
private static IEdmModel GetEdmModel()
{
    ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
    builder.EntityType<Customer>().HasKey(c =>new { c.StringProp, c.DateProp, c.GuidProp});
	builder.EntitySet<Customer>("Customers");
	return builder.GetEdmModel();
}	
```	

Before Web API OData V6.x, key segment convention routing only supports the single key convention binding, just use the **key** as the parameter name.

In Web API OData V6.x, we use the following convention for the composite key parameter name, but leave the **key** for single key parameter.

```C#
"key" + {CompositeKeyPropertyName}
```	

Therefore, for **StringProp** key property, the action parameter name should be **keyStringProp**.

Let's see how the contoller action looks like:

```C#
public class CustomersController : ODataController
{
    public IHttpActionResult Get([FromODataUri]string keyStringProp, [FromODataUri]Date keyDateProp, [FromODataUri]Guid keyGuidProp)
    {
    }
}
```	

Now, you can issue a request:

```C#
GET http://~/odata/Customers(StringKey='my',DateKey=2016-05-11,GuidKey=46538EC2-E497-4DFE-A039-1C22F0999D6C)
```	

The result is:

1. keyStringProp == "my";
2. keyDataProp == new Date(2016, 5, 11);
3. keyGuidProp == new Guid("46538EC2-E497-4DFE-A039-1C22F0999D6C")

**Be noted**, this rule also applys to the navigation property key convention binding.

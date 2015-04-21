---
title : "4.2 Referential constraint"
layout: post
category: "4. OData features"
permalink: "/referential-constraint"
---

The following sample codes can be used for Web API OData V3 & V4 with a little bit function name changing.

### Define Referential Constraint Using Attribute

There is an attribute named “ForeignKeyAttribute” which can be place on:

1.the foreign key property and specify the associated navigation property name, for example: 

{% highlight csharp %}

public class Order
{
    public int OrderId { get; set; }

    [ForeignKey("Customer")]
    public int MyCustomerId { get; set; }

    public Customer Customer { get; set; }
}

{% endhighlight %}

2.a navigation property and specify the associated foreign key name, for example:

{% highlight csharp %}

public class Order
{
    public int OrderId { get; set; }

    public int CustId1 { get; set; }
    public string CustId2 { get; set; }

    [ForeignKey("CustId1,CustId2")]
    public Customer Customer { get; set; }
}

{% endhighlight %}
*Where*, *Customer* has two keys.

Now, you can build the Edm Model by convention model builder as:

{% highlight csharp %}

public IEdmModel GetEdmModel()
{            
    ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
    builder.EntitySet<Customer>("Customers");
    builder.EntitySet<Order>("Orders");
    return builder.GetEdmModel();
}

{% endhighlight %}

#### Define Referential Constraint Using Convention

If user doesn’t add *any* referential constraint, Web API will try to help user to discovery the foreign key automatically. There are two conventions as follows:
1.With same property type and same type name plus key name. For example:
   
{% highlight csharp %}

public class Customer
{ 
   [Key]
   public string Id {get;set;}
   public IList<Order> Orders {get;set;}
}
public class Order
{
    public int OrderId { get; set; }
    public string CustomerId {get;set;}
    public Customer Customer { get; set; }
}

{% endhighlight %}
*Where*, *Customer* type name "Customer" plus key name "Id" equals the property "CustomerId" in the *Order*.

2.With same property type and same property name. For example:
   
{% highlight csharp %}

public class Customer
{ 
   [Key]
   public string CustomerId {get;set;}
   public IList<Order> Orders {get;set;}
}

public class Order
{
    public int OrderId { get; set; }
    public string CustomerId {get;set;}
    public Customer Customer { get; set; }
}

{% endhighlight %}
*Where*, Property (key) "CustomerId" in the *Customer* equals the property "CustomerId" in the *Order*.

Now, you can build the Edm Model using convention model builder same as above section.

### Define Referential Constraint Programmatically
You can call the new added Public APIs (HasRequired, HasOptional) to define the referential constraint when defining a navigation property. For example:

{% highlight csharp %}

public class Customer
{
    public int Id { get; set; }
       
    public IList<Order> Orders { get; set; }
}

public class Order
{
    public int OrderId { get; set; }
 
    public int CustomerId { get; set; }         

    public Customer Customer { get; set; }
}

ODataModelBuilder builder = new ODataModelBuilder();
builder.EntityType<Customer>().HasKey(c => c.Id).HasMany(c => c.Orders);
builder.EntityType<Order>().HasKey(c => c.OrderId)
    .HasRequired(o => o.Customer, (o, c) => o.CustomerId == c.Id);
    .CascadeOnDelete();
    
{% endhighlight %}

It also supports to define multiple referential constraints, for example:
{% highlight csharp %}

builder.EntityType<Order>()
    .HasKey(o => o.OrderId)
    .HasRequired(o => o.Customer, (o,c) => o.Key1 == c.Id1 && o.Key2 == c.Id2);
    
{% endhighlight %}

Thanks.

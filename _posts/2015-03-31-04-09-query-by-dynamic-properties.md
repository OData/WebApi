---
title : "4.9 Query by dynamic properties"
layout: post
category: "4. OData features"
---

Since Web API OData V5.5, it supports filter, select and orderby on dynamic properties.

Let's see a sample about this feature.

### CLR Model

First of all, we create the following CLR classes as our model:

{% highlight csharp %}
public class SimpleOpenCustomer
{
    [Key]
    public int CustomerId { get; set; }
    public string Name { get; set; }
    public string Website { get; set; }
    public IDictionary<string, object> CustomerProperties { get; set; }
}

{% endhighlight %}

### Build Edm Model

Now, we can build the Edm Model as:

{% highlight csharp %}
private static IEdmModel GetEdmModel()
{ 
    ODataModelBuilder builder = new ODataConventionModelBuilder();
    builder.EntitySet<SimpleOpenCustomer>("SimpleOpenCustomers");
    return builder.GetEdmModel();
}
{% endhighlight %}

### Use filter, orferby, select on dynamic property

#### Routing
In the `SimpleOpenCustomersController`, add the following method:

{% highlight csharp %}
[EnableQuery]
public IQueryable<SimpleOpenCustomer> Get()
{
    return CreateCustomers().AsQueryable();
}
{% endhighlight %}

#### Request Samples
We can query like:

{% highlight csharp %}
~/odata/SimpleOpenCustomers?$orderby=Token desc&$filter=Token ne null
~/odata/SimpleOpenCustomers?$select=Token
{% endhighlight %}

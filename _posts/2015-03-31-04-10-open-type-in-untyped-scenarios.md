---
title : "4.10 Open type in untyped scenarios"
layout: post
category: "4. OData features"
---

Since Web API OData V5.5, it supports open type and dynamic property on un-type scenario, dynamic properties can be:

1. Primitive Type
2. Enum Type
3. Complex Type
4. Collection of above

Let's see a sample about this feature.

### Build un-type Edm Model

Now, we can build the Edm Model as:

{% highlight csharp %}
private static IEdmModel GetUntypedEdmModel()
{
    var model = new EdmModel();
    // complex type address
    EdmComplexType address = new EdmComplexType("NS", "Address", null, false, true);
    address.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String);
    model.AddElement(address);
    // enum type color
    EdmEnumType color = new EdmEnumType("NS", "Color");
    color.AddMember(new EdmEnumMember(color, "Red", new EdmIntegerConstant(0)));
    model.AddElement(color);
    // entity type customer
    EdmEntityType customer = new EdmEntityType("NS", "UntypedSimpleOpenCustomer", null, false, true);
    customer.AddKeys(customer.AddStructuralProperty("CustomerId", EdmPrimitiveTypeKind.Int32));
    customer.AddStructuralProperty("Color", new EdmEnumTypeReference(color, isNullable: true));
    model.AddElement(customer);  
    EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
    container.AddEntitySet("UntypedSimpleOpenCustomers", customer);
    model.AddElement(container);
    return model;
}
{% endhighlight %}

If the dynamic property is not primitive type, you should declare it in model like the code above.

### GET an untyped open entity with dynamic property

#### Routing
In the `UntypedSimpleOpenCustomersController`, add the following method:

{% highlight csharp %}
[HttpGet]
public IHttpActionResult Get(int key)
{
   //return an EdmEntityObject
   ...
}
{% endhighlight %}

#### Request Samples
We can get the entity as:
{% highlight csharp %}
~/odata/UntypedSimpleOpenCustomers(1)
{% endhighlight %}

### POST an untyped open entity with dynamic property

#### Routing
In the `UntypedSimpleOpenCustomersController`, add the following method :

{% highlight csharp %}
[HttpPost]
public IHttpActionResult PostUntypedSimpleOpenCustomer(EdmEntityObject customer)
{
    object nameValue;
    customer.TryGetPropertyValue("Name", out nameValue);
    Type nameType;
    customer.TryGetPropertyType("Name", out nameType);
    ...
}
{% endhighlight %}

#### Request Samples
You should declare the type of dynamic properties in request body.
Payload:

{% highlight csharp %}
const string Payload = "{" + 
              "\"@odata.context\":\"http://localhost/odata/$metadata#UntypedSimpleOpenCustomer/$entity\"," +
              "\"CustomerId\":6,\"Name\":\"FirstName 6\"," +
              "\"Address\":{" +
                "\"@odata.type\":\"#NS.Address\",\"Street\":\"Street 6\",\"City\":\"City 6\"" +
              "}," + 
              "\"Addresses@odata.type\":\"#Collection(NS.Address)\"," +
              "\"Addresses\":[{" +
                "\"@odata.type\":\"#NS.Address\",\"Street\":\"Street 7\",\"City\":\"City 7\"" +
              "}]," +
              "\"DoubleList@odata.type\":\"#Collection(Double)\"," +
              "\"DoubleList\":[5.5, 4.4, 3.3]," +
              "\"FavoriteColor@odata.type\":\"#NS.Color\"," +
              "\"FavoriteColor\":\"Red\"," +
              "\"Color\":\"Red\"," +
              "\"FavoriteColors@odata.type\":\"#Collection(NS.Color)\"," +
              "\"FavoriteColors\":[\"0\", \"1\"]" +
            "}";
{% endhighlight %}

Url:

{% highlight csharp %}
~/odata/UntypedSimpleOpenCustomers
{% endhighlight %}

### The type of dynamic properties in un-type scenario

#### EdmEntityObject (Collection)
Represent an entity.

#### EdmComplexObject (Collection)
Represent an complex property.

#### EdmEnumObject (Collection)
Represent an enum property.

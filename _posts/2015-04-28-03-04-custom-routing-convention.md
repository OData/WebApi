---
layout: post
title: "3.4 Custom routing convention"
description: "Routing Conventions"
category: "3. Routing"
---

It's easy to custom your own routing convention to override the default Web API OData routing. Let's see how to target it.

### Property access routing convention

From built-in routing convention section, we know that developer should add many actions for every property access. 

For example, if the client issues he following property access,

{% highlight csharp %}
~/odata/Customers(1)/Orders
~/odata/Customers(1)/Address
~/odata/Customers(1)/Name
...
{% endhighlight %}

Service should have the following actions in `CustomersController` to handle:

{% highlight csharp %}
public class CustomersController : ODataController
{
    public string GetOrders([FromODataUri]int key)
    {
        ......
    }
	
	public string GetAddress([FromODataUri]int key)
    {
        ......
    }
	
	public string GetName([FromODataUri]int key)
    {
        ......
    }
}
{% endhighlight %}

If we have hundreds of similar property for `Customer`, we should add hundres of similar function in `CustomersController`. It's boring and we can create our own routing convention to override it.

### Custom routing convention

We can create our own routing convention class by implementing the `IODataRoutingConvention`. However, if you don't want to change behaviour to find the controller, the new routing convention class can derive from `NavigationSourceRoutingConvention'.

Let's build a sample property access routing convention class derived from `NavigationSourceRoutingConvention`.

{% highlight csharp %}
public class CustomPropertyRoutingConvention : NavigationSourceRoutingConvention
{
	private const string ActionName = "GetProperty";

	public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext,
		ILookup<string, HttpActionDescriptor> actionMap)
	{
		if (odataPath == null || controllerContext == null || actionMap == null)
		{
			return null;
		}

		if (odataPath.PathTemplate == "~/entityset/key/property" ||
			odataPath.PathTemplate == "~/entityset/key/cast/property" ||
			odataPath.PathTemplate == "~/singleton/property" ||
			odataPath.PathTemplate == "~/singleton/cast/property")
		{
			var segment = odataPath.Segments[odataPath.Segments.Count - 1] as PropertyAccessPathSegment;

			if (segment != null)
			{
				string actionName = FindMatchingAction(actionMap, ActionName);

				if (actionName != null)
				{
					if (odataPath.PathTemplate.StartsWith("~/entityset/key", StringComparison.Ordinal))
					{
						KeyValuePathSegment keyValueSegment = odataPath.Segments[1] as KeyValuePathSegment;
						controllerContext.RouteData.Values[ODataRouteConstants.Key] = keyValueSegment.Value;
					}

					controllerContext.RouteData.Values["propertyName"] = segment.PropertyName;

					return actionName;
				}
			}
		}

		return null;
	}

	public static string FindMatchingAction(ILookup<string, HttpActionDescriptor> actionMap, params string[] targetActionNames)
	{
		foreach (string targetActionName in targetActionNames)
		{
			if (actionMap.Contains(targetActionName))
			{
				return targetActionName;
			}
		}

		return null;
	}
}
{% endhighlight %}

Where, we routes the following path template to a certain action named `GetProperty`.

{% highlight csharp %}
~/entityset/key/property
~/entityset/key/cast/property
~/singleton/property
~/singleton/cast/property
{% endhighlight %}

### Enable customized routing convention

The following sample codes are used to enable the customized routing convention.

{% highlight csharp %}
HttpConfiguration configuration = ......
IEdmModel model = GetEdmModel();
IList<IODataRoutingConvention> conventions = ODataRoutingConventions.CreateDefaultWithAttributeRouting(configuration, model);
conventions.Insert(0, new CustomPropertyRoutingConvention());
configuration.MapODataServiceRoute("odata", "odata", model, new DefaultODataPathHandler(), conventions);
{% endhighlight %}

Where, we insert our own routing convention at the starting position to override the default Web API OData property access routing convention.

### Add actions

In the `CustomersController`, only one method named `GetProperty` should be added. 

{% highlight csharp %}
public class CustomersController : ODataController
{
	[HttpGet]
	public IHttpActionResult GetProperty(int key, string propertyName)
	{
		Customer customer = _customers.FirstOrDefault(c => c.CustomerId == key);
		if (customer == null)
		{
			return NotFound();
		}

		PropertyInfo info = typeof(Customer).GetProperty(propertyName);

		object value = info.GetValue(customer);

		return Ok(value, value.GetType());
	}
	
	private IHttpActionResult Ok(object content, Type type)
	{
		var resultType = typeof(OkNegotiatedContentResult<>).MakeGenericType(type);
		return Activator.CreateInstance(resultType, content, this) as IHttpActionResult;
	}
}
{% endhighlight %}

### Samples

Let's have some request Uri sample to test:

{% highlight csharp %}
http://localhost/odata/Customers(2)/Name
{% endhighlight %}

The result is:

{% highlight csharp %}
{
  "@odata.context":"http://localhost/odata/$metadata#Customers(2)/Name","value": "Mike"
}
{% endhighlight %}

2. 
{% highlight csharp %}
http://localhost/odata/Customers(2)/Location
{% endhighlight %}

The result is:
{% highlight csharp %}
{
  "@odata.context":"http://localhost/odata/$metadata#Customers(2)/Salary","value ":2000.0
}
{% endhighlight %}

3. 
{% highlight csharp %}
http://localhost/odata/Customers(2)/Location
{% endhighlight %}

The result is:
{% highlight csharp %}
{
  "@odata.context":"http://localhost/odata/$metadata#Customers(2)/Location","Country":"United States","City":"Redmond"
}
{% endhighlight %}

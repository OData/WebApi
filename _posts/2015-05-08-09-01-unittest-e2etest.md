---
layout: post
title: "9.1 Unit Test and E2E Test"
description: ""
category: "9. Test"
---

In OData WebApi, there are unit test, e2e test for V3 and V4, those [test cases](https://github.com/OData/WebApi/tree/master/OData/test) are to ensure the feature and bug fix, also to make sure not break old functionality.

### Unit Test
Every class in OData WebApi has it's own unit test class, for example:
OData/src/System.Web.OData/OData/Builder/ActionLinkBuilder.cs 's test class is 
OData/test/UnitTest/System.Web.OData.Test/OData/Builder/ActionLinkBuilderTests.cs.

You can find that the structural under `System.Web.OData` folder and `System.Web.OData.Test` folder are the same, also for V3 `System.Web.Http.OData.Test`, so if your pull request contains any class add/change, you should add/change(this change here means add test cases) unit test file.

#### How To Add Unit Test
* Try to avoid other dependency use moq.
* Make sure you add/change the right class(V4 or V3 or both).
* Can add functinal test for complicate scenario, but E2E test cases are better.


### E2E Test
E2E test are complete test for user scenarios, always begin with client request and end with server response. If your unit test in pull request can't cover all scenario well or you have a big pull request, please add E2E test for it.

#### How To Add E2E Test
* Add test cases in exist test class that related to your pull request.
* Add new folder and test class for your own scenario.
* If the test has any kind of state that is preserved between request, it should be the only test defined in the test class to avoid conflicts when executed along other tests.
* Try to test with both in memory data and DB data.
* Keep test folder, class style with exist test folder, class.


#### Test Sample
{% highlight csharp %}
[NuwaFramework]
public class MyTest
{

    [NuwaBaseAddress]
    public string BaseAddress { get; set; }

    [NuwaHttpClient]
    public HttpClient Client { get; set; }

    [NuwaConfiguration]
    public static void UpdateConfiguration(HttpConfiguration config)
    {
        config.Routes.MapODataRoute("odata", "odata", GetModel());
    }     

    private static IEdmModel GetModel()
    {
        ODataModelBuilder builder = new ODataConventionModelBuilder();
        var customers = builder.EntitySet<Customer>("Customers");
        var orders = builder.EntitySet<Order>("Orders");
        return builder.GetEdmModel();
    }

    [Fact]
    public void GetCustomersWork()
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,BaseAddress + "/odata/Customers");
        HttpResponseMessage response = Client.SendAsync(request).Result;
        Assert.Equal(HttpStatusCode.OK,response.StatusCode);
    }
}

[NuwaFramework]
public class MyTest2
{
    [NuwaBaseAddress]
    public string BaseAddress { get; set; }

    [NuwaHttpClient]
    public HttpClient Client { get; set; }

    [NuwaConfiguration]
    public static void UpdateConfiguration(HttpConfiguration config)
    {
        config.Routes.MapODataRoute("odata", "odata", GetModel());
    }

    private static IEdmModel GetModel()
    {
        ODataModelBuilder builder = new ODataConventionModelBuilder();
        var customers = builder.EntitySet<Customer>("Customers");
        var orders = builder.EntitySet<Order>("Orders");
        return builder.GetEdmModel();
    }

    [Fact]
    public void GetCustomersWork()
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/Customers");
        HttpResponseMessage response = Client.SendAsync(request).Result;
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public class CustomersController : ODataController
{
    [Queryable(PageSize = 3)]
    public IHttpActionResult Get()
    {
        return Ok(Enumerable.Range(0, 10).Select(i => new Customer
        {
            Id = i,
            Name = "Name " + i
        }));
    }

    public IHttpActionResult Post(Customer customer)
    {
        return Created(customer);
    }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public IList<Order> Orders { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public DateTime PurchaseDate { get; set; }
}
{% endhighlight %}

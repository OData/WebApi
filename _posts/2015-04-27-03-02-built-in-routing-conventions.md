---
layout: post
title: "3.2 Built-in routing conventions"
description: "Routing Conventions"
category: "3. Routing"
---

When Web API gets an OData request, it maps the request to a controller name and an action name. The mapping is based on the HTTP method and the URI. For example, `GET /odata/Products(1)` maps to `ProductsController.GetProduct`.

This article describe the built-in OData routing conventions. These conventions are designed specifically for OData endpoints, and they replace the default Web API routing system. (The replacement happens when you call MapODataRoute.)


### Built-in Routing Conventions
Before describe the OData routing conventions in Web API, it’s helpful to understand OData URIs. An OData URI consists of:

* The service root
* The odata path
* Query options

For example: `http://example.com/odata/Products(1)/Supplier?$top=2 `

* *The service root* : http://example.com/odata
* *The odata path* : Products(1)/Supplier
* *Query options* : ?$top=2  


For OData routing, the important part is the OData path. The OData path is divided into segments, each segments are seperated with '/'.

[For example], `Products(1)/Supplier` has three segments:

* **Products** refers to an entity set named “Products”.
* **1** is an entity key, selecting a single entity from the set.
* **Supplier** is a navigation property that selects a related entity.

So this path picks out the supplier of product 1.

**OData path segments do not always correspond to URI segments. For example, “1” is considered a key path segment.**

**Controller Names.** The controller name is always derived from the entity set at the root of the OData path. For example, if the OData path is `Products(1)/Supplier`, Web API looks for a controller named ProductsController.

So, the controller convention is:  **[entityset name] + "Controller"**, derived from `ODataController`

**Action Names.** Action names are derived from the path segments plus the entity data model (EDM), as listed in the following tables. In some cases, you have two choices for the action name. For example, “Get” or "GetProducts".

**Querying Entities**

![]({{site.baseurl}}/img/03-02-queryEntitiesConvention.png)

**Creating, Updating, and Deleting Entities**

![]({{site.baseurl}}/img/03-02-updateDeleteEntitiesConvention.png)

**Querying a Navigation Property**

Request | Example URI | Action Name | Example Action
------------ | ------------- | ------------- | -------------
GET /entityset(key)/navigation | /Products(1)/Supplier | GetNavigationFromEntityType or GetNavigation | GetSupplierFromProduct
GET /entityset(key)/cast/navigation | /Products(1)/Models.Book/Author | GetNavigationFromEntityType or GetNavigation | GetAuthorFromBook

**Querying, Creating and Deleting Links**

Request | Example URI | Action Name
------------ | ------------- | -------------
GET /entityset(key)/navigation/$ref | /Products(1)/Supplier/$ref | GetRef
GET /entityset(key)/cast/navigation/$ref | /Products(1)/Models.Book/Author/\$ref | GetRef
POST /entityset(key)/navigation/$ref | /Products(1)/Supplier/$ref | CreateRef
POST /entityset(key)/cast/navigation/$ref | /Products(1)/Models.Book/Author/$ref | CreateRef
PUT /entityset(key)/navigation/$ref | /Products(1)/Supplier/$ref | CreateRef
PUT /entityset(key)/cast/navigation/$ref | /Products(1)/Models.Book/Author/$ref | CreateRef
DELETE /entityset(key)/navigation/$ref | /Products(1)/Supplier/$ref | DeleteRef
DELETE /entityset(key)/cast/navigation/$ref | /Products(1)/Models.Book/Author/$ref | DeleteRef
DELETE /entityset(key)/navigation(relatedKey)/$ref | /Products(1)/Supplier(1)/$ref | DeleteRef
DELETE /entityset(key)/cast/navigation(relatedKey)/$ref | /Products(1)/Models.Book/Author(1)/$ref | DeleteRef

**Properties**

Request | Example URI | Action Name | Example Action
------------ | ------------- | ------------- | -------------
GET /entityset(key)/property | /Products(1)/Name | GetPropertyFromEntityType or GetProperty | GetNameFromProduct
GET /entityset(key)/cast/property | /Products(1)/Models.Book/Author | GetPropertyFromEntityType or GetProperty | GetTitleFromBook

**Actions**

Request | Example URI | Action Name | Example Action
------------ | ------------- | ------------- | -------------
POST /entityset(key)/action | /Products(1)/Rate | ActionNameOnEntityType or ActionName | RateOnProduct
POST /entityset(key)/cast/action | /Products(1)/Models.Book/CheckOut | ActionNameOnEntityType or ActionName | CheckOutOnBook
POST /entityset/action | /Products/Rate | ActionNameOnCollectionOfEntityType or ActionName | RateOnCollectionOfProduct
POST /entityset/cast/action | /Products/Models.Book/CheckOut | ActionNameOnCollectionOfEntityType or ActionName | CheckOutOnCollectionOfBook

**Functions**

Request | Example URI | Action Name | Example Action
------------ | ------------- | ------------- | -------------
GET /entityset(key)/function | /Products(1)/Rate | functionNameOnEntityType or functionName | RateOnProduct
GET /entityset(key)/cast/function | /Products(1)/Models.Book/CheckOut | functionNameOnEntityType or functionName | CheckOutOnBook
GET /entityset/function | /Products/Rate | functionNameOnCollectionOfEntityType or functionName | RateOnCollectionOfProduct
GET /entityset/cast/function | /Products/Models.Book/CheckOut | functionNameOnCollectionOfEntityType or functionName | CheckOutOnCollectionOfBook

**Method Signatures**

Here are some rules for the method signatures:

* If the path contains a key, the action should have a parameter named key.
* If the path contains a key into a navigation property, the action should have a parameter named relatedKey.
* POST and PUT requests take a parameter of the entity type.
* PATCH requests take a parameter of type Delta, where T is the entity type.

For reference, here is an example that shows method signatures for most built-in OData routing convention.

{% highlight csharp %}

public class ProductsController : ODataController
{
    // GET /odata/Products
    public IQueryable<Product> Get()

    // GET /odata/Products(1)
    public Product Get(int key)

    // GET /odata/Products(1)/ODataRouting.Models.Book
    public Book GetBook(int key)

    // POST /odata/Products 
    public HttpResponseMessage Post(Product item)

    // PUT /odata/Products(1)
    public HttpResponseMessage Put(int key, Product item)

    // PATCH /odata/Products(1)
    public HttpResponseMessage Patch(int key, Delta<Product> item)

    // DELETE /odata/Products(1)
    public HttpResponseMessage Delete(int key)

    // PUT /odata/Products(1)/ODataRouting.Models.Book
    public HttpResponseMessage PutBook(int key, Book item)

    // PATCH /odata/Products(1)/ODataRouting.Models.Book
    public HttpResponseMessage PatchBook(int key, Delta<Book> item)

    // DELETE /odata/Products(1)/ODataRouting.Models.Book
    public HttpResponseMessage DeleteBook(int key)

    // GET /odata/Products(1)/Supplier
    public Supplier GetSupplierFromProduct(int key)

    // GET /odata/Products(1)/ODataRouting.Models.Book/Author
    public Author GetAuthorFromBook(int key)

    // POST /odata/Products(1)/Supplier/$ref
    public HttpResponseMessage CreateLink(int key, 
        string navigationProperty, [FromBody] Uri link)

    // DELETE /odata/Products(1)/Supplier/$ref
    public HttpResponseMessage DeleteLink(int key, 
        string navigationProperty, [FromBody] Uri link)

    // DELETE /odata/Products(1)/Parts(1)/$ref
    public HttpResponseMessage DeleteLink(int key, string relatedKey, string navigationProperty)

    // GET odata/Products(1)/Name
    // GET odata/Products(1)/Name/$value
    public HttpResponseMessage GetNameFromProduct(int key)

    // GET /odata/Products(1)/ODataRouting.Models.Book/Title
    // GET /odata/Products(1)/ODataRouting.Models.Book/Title/$value
    public HttpResponseMessage GetTitleFromBook(int key)
}
    
{% endhighlight %}

Update form [Routing Conventions in OData V3.0](http://www.asp.net/web-api/overview/odata-support-in-aspnet-web-api/odata-routing-conventions)

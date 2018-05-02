---
layout: post
title : "14.3 WebApi URI parser default setting updates: case-insensitive name token and unqualified functions & actions"
description: "7.x WebApi Uri parser using case-insensitve name token and unqualified functions & actions"
category: "14. 7.x Features "
---


**OData Core Library** v7.x has introduced the following two usability improvement:

* Uri parsing with case-insensitive name token, and 

* Unqualified functions & actions, which are not required to have namespace prefix.

Starting v7.0, these two functionality are supported by default in WebApi.
 
### Examples
**Prior to WebApi v7.0**, for example, the following Uris are functional:

* GET /service/Customers?$filter=Id eq 5

* POST /service/Customers(5)/Default.UpdateAddress()

With WebApi **v7.0** **by default**, in addition to above, the following variances are also supported:

* GET /service/Customers?$filter=id eq 5

* GET /service/CUSTOMERS?$filter=Id eq 5

* POST /service/Customers(5)/UpdateAddress()

and the combination of both case-insensitive and unqualified functions & actions, such as:
 
* POST /service/CUSTOMERS(5)/UpdateAddress()


### Backward Compatibility
Case-insensitive semantics is supported for type name, property name and function/action name. It will first try to resolve the name token with case-sensitive semantics and return the best match if found; otherwise case-insensitive semantics would be attempted, returning the unique match or throwing exception in case of multiple case-insensitive matches.

With support for unqualified function & action, Uri parser will do namespace-qualified function & action resolution when the function name is namespace-qualified; otherwise all namespaces in the customer's model are treated as default namespaces, returning the unique match or throwing exception in case of multiple namespace-unqualified matches. 

Therefore, existing working cases for WebApi prior to v7.0 are supposed to continue to work, with support for variances shown above added to WebApi v7.0.

Please note that, even though case-insensitive and unqualified function & action supports are added as WebApi usability improvement, the best practice of service design of using case-insensitively unique name tokens is still strongly encouraged (see ["Service designers ...should avoid naming bound functions, actions, or derived types with the same name as a structural or navigation property of the type](http://docs.oasis-open.org/odata/odata/v4.01/cs01/part1-protocol/odata-v4.01-cs01-part1-protocol.html#_Toc505771104)). One particular example is that property and unqualified function with same name could result in failing to create operation segment as expected.


### Restoring the original behavior
Even though the updated default values above are backward compatible, customer can still customize WebApi back to original behavior using dependency injection:
~~~csharp
    // HttpConfiguration configuration
    IServiceProvider rootContainer = configuration.CreateODataRootContainer(routeName, 
        builder => builder..AddService<ODataUriResolver>(ServiceLifetime.Singleton, sp => new ODataUriResolver());
~~~
The above code segment utilizes the WebApi's HttpConfigurationExtensions method CreateODataRootContainer to override the built-in WebApi singleton service type ODataUriResolver with an original default instance of ODataUriResolver in the IServiceProvider root container.

---
layout: post
title : "14.3 WebApi URI parser default setting updates: case-insensitive names and unqualified functions & actions"
description: "7.x WebApi URI parser using case-insensitve names and unqualified functions & actions"
category: "14. 7.x Features "
---


**OData Core Library** v7.x has introduced the following two usability improvement:

* Uri parsing with case-insensitive name, and 

* Unqualified functions & actions, which are not required to have namespace prefix.

Starting with WebAPI OData v7.0, these two behaviors are supported by default.
 
### Examples
**Prior to WebApi v7.0**, for example, the following Uris are supported:

* GET /service/Customers?$filter=Id eq 5

* POST /service/Customers(5)/Default.UpdateAddress()

With WebApi **v7.0** **by default**, in addition to above, the following variances are also supported:

* GET /service/Customers?$filter=id eq 5

* GET /service/CUSTOMERS?$filter=Id eq 5

* POST /service/Customers(5)/UpdateAddress()

and the combination of both case-insensitive and unqualified functions & actions, such as:
 
* POST /service/CUSTOMERS(5)/UpdateAddress()


### Backward Compatibility
Case-insensitive semantics is supported for type name, property name and function/action name. WebAPI OData will first try to resolve the name with case-sensitive semantics and return the best match if found; otherwise case-insensitive semantics are applied, returning the unique match or throwing an exception if multiple case-insensitive matches exist.

With support for unqualified function & action, the URI parser will do namespace-qualified function & action resolution when the operation name is namespace-qualified; otherwise all namespaces in the customer's model are treated as default namespaces, returning the unique match or throwing an exception if multiple unqualified matches exist.

Because of the precedence rules applied, scenarios supported in previous versions of WebAPI continue to be supported with the same semantics, while new scenarios that previously returned errors are also are now supported.

Please note that, even though case-insensitive and unqualified function & action support is added as a usability improvement, services are strongly encouraged to use names that are unique regardless of case, and to [avoid naming bound functions, actions, or derived types with the same name as a property of the bound type](http://docs.oasis-open.org/odata/odata/v4.01/cs01/part1-protocol/odata-v4.01-cs01-part1-protocol.html#_Toc505771104). For example, a property and unqualified function with same name would resolve to a property name when the unqualified function may have been expected.


### Restoring the original behavior
Even though the new behavior is backward compatible for most scenarios, customers can configure WebAPI to enforce case sensitivity and namespace qualification, as in 6.x, using dependency injection:
~~~csharp
    // HttpConfiguration configuration
    IServiceProvider rootContainer = configuration.CreateODataRootContainer(routeName, 
        builder => builder.AddService<ODataUriResolver>(ServiceLifetime.Singleton, sp => new ODataUriResolver());
~~~
The above code replaces the ODataUriResolver service that supports case-insensitivity and unqualified names with a default instance of ODataUriResolver that does not.

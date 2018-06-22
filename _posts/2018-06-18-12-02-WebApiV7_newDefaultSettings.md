---
layout: post
title: "12.1 WebApi 7.0 Default Setting Updates"
description: "Design document for updated default settings for WebApi 7.0"
category: "12. Design"
---
        
# WebApi Default Enable Unqualified Operations and Case-insensitive Uri #


Overview
========

OData Layer
-----------

OData libraries 7.4.4+ contains updates to improve
usability & compatibility of the library by virtue of exposing options
that can be set by the caller of the OData core library (ODL). Related
to request Uri parsing, the following two simplifications are now
available when URI parser is configured properly:

-   As usual, namespace is the primary mechanism for resolving name
    token conflicts in multiple schema component of the model, therefore
    namespace is required up to OData v4. To improve flexibility, with
    notion of [Default
    Namespaces](http://docs.oasis-open.org/odata/odata/v4.01/cs01/part1-protocol/odata-v4.01-cs01-part1-protocol.html#sec_DefaultNamespaces)
    introduced in OData v4.01, namespace qualifier is optional for
    function or action identifier in request Uri. When corresponding
    option in ODL Uri parser enabled:

    -   If the function or action identifier contains a namespace
        qualifier, as in all the original cases, Uri parser uses
        original namespace-qualified semantic to ensure backward
        compatibility;

    -   Otherwise, URI parser will search among the main schema and
        referenced sub-schemas treated as default namespaces, trying to
        resolve the unqualified function & action identifier to unique
        function / action element.

        -   Exception will be thrown if no matches are found, or
            multiple functions or actions of same name are found in
            different namespaces of the model.

        -   Property with same name as unqualified function / action
            name could cause the token being bound to a property segment
            unexpectedly. This should be avoided per best design
            practice in OData protocol: \"Service designers should
            ensure uniqueness of schema children across all default
            namespaces, and should avoid naming bound functions,
            actions, or derived types with the same name as a structural
            or navigation property of the type.\"

-   In OData v4.01, case-insensitive name resolution is supported for
    [system query
    options](http://docs.oasis-open.org/odata/odata/v4.01/cs01/part1-protocol/odata-v4.01-cs01-part1-protocol.html#sec_SystemQueryOptions),
    [built-in
    function](http://docs.oasis-open.org/odata/odata/v4.01/cs01/part1-protocol/odata-v4.01-cs01-part1-protocol.html#sec_BuiltinQueryFunctions)
    and
    [operator](http://docs.oasis-open.org/odata/odata/v4.01/cs01/part1-protocol/odata-v4.01-cs01-part1-protocol.html#sec_BuiltinFilterOperations)
    names, as well as type names, property names, enum values in form of
    strings. When corresponding option in ODL Uri parser is enabled, Uri
    parser first uses case-sensitive semantics as before and returns the
    result if exact match is found; otherwise, tries case-insensitive
    semantics, and returns the unique result found or throws exception
    for ambiguous result such as duplicated items.

    -   Most of the case-insensitive support above has been implemented
        in current ODL version, except for minor bug fixes and
        case-insensitive support for built-in function, which are
        addressed as part of this task.

    Note the above options are also combinatorial, with expected behavior for Uri parsing.

    In ODL implementation, the primary support for the two options above is the default ODataUriResolver and its derived classes.

WebApi Layer
------------

WebApi layer utilizes dependency injection to specify various services
as options for URI parser. Dependencies can be specified in WebApi layer
overriding default values provided by the IServicesProvider container.

With the new default values, WebApi will exhibit different behavior
related Uri parsing. The change should be backward compatible (all
existing cases should work as it used to be), and previous error cases
due to required case-sensitive and namespace qualifier for function
should become working cases, hence improving usability of OData stack.

Scenarios
=========

Write Scenario Description
--------------------------

All scenarios are related to OData Uri parsing using default WebAPI
settings. All sample scenarios assume no name collisions in EDM model,
unless noted otherwise.

### Functions: namespace qualified/unqualified

With function defined in the model:
builder.EntityType\<Customer\>().Action(\"UpdateAddress\");

-   Namespace qualified function should work as before:

> POST /service/Customers(1)/Default.UpdateAddress()

-   Namespace unqualified function should become a successful case:

> POST /service/Customers(1)/UpdateAddress()

### Case-Insensitive name resolution:

-   Case-insensitive property name should become resolvable:

    With model: public class InvalidQueryCustomer { public int <span style="color:red">Id</span> { get; set; } }

    > GET /service/InvalidQueryCustomers?\$filter=<span style="color:blue">id</span> eq 5 : HTTP 200

    > GET /service/InvalidQueryCustomers(5)?\$filter=<span style="color:blue">id</span> eq 5 : HTTP 400 "Query
options \$filter, \$orderby, \$count, \$skip, and \$top can be applied
only on collections."

-   Case-insensitive customer uri function name should become
    resolvable:

    With model having entity type People defined and following customized Uri function:
      >
      > FunctionSignatureWithReturnType myFunc
      >
      > = new
      > FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetBoolean(true),
      >
      > EdmCoreModel.Instance.GetString(true),
      > EdmCoreModel.Instance.GetString(true));
      >
      > // Add a custom uri function
      >
      > CustomUriFunctions.AddCustomUriFunction(<span style="color:red">\"myMixedCasestringfunction\"</span>, myFunc);

      This should work:
      > GET /service/People?\$filter=<span style="color:blue">mYMixedCasesTrInGfUnCtIoN</span>(Name,\'BlaBla\') : HTTP 200

-   Combination of case-insensitive type & property name and unqualified
    function should become resolvable:

     With controller:

     > \[HttpGet\]
     > 
     > public ITestActionResult CalculateTotalOrders(int key, int month) {/\*...\*/}

     Following OData v4 Uris should work:
     > GET /service/Customers(1)/Default. CalculateTotalOrders (month=1) : HTTP 200
     > GET /service/CuStOmErS(1)/CaLcUlAtEToTaLoRdErS (MONTH=1) : HTTP 200

Design Strategy
===============

Dependency Injection of ODataUriResolver
----------------------------------------

ODL (Microsoft.OData.Core) library supports dependency injection of a
collection of service types from client via the IServiceProvider
interface. The IServiceProvider can be considered as a container
populated with default objects by ODL, while ODL's client, such as
WebApi, can override default objects by injecting customized
dependencies.

### ODL IContainerBuilder and ContainerBuilderExtensions

The ContainerBuilderExtensions.AddDefaultODataServices(this
IContainerBuilder) implementation populates a collection of default
OData service objects into the container's builder. For example, default
service of type ODataUriResolver is registered as one instance of
ODataUriResolver as follows:

> public static IContainerBuilder AddDefaultODataServices(this IContainerBuilder builder)
>
> {
>
> //.........
>
> builder.AddService(ServiceLifetime.Singleton, 
>
> sp =\> ODataUriResolver.GetUriResolver(null));
>
> //.........
>
> }

###  WebAPI dependency injection of customized ODataUriResolver:

-   WebAPI defines the DefaultContainerBuilder implementing the ODL's
    IContainerBuilder interface.

-   When root container is created from HttpConfiguration (via the
    HttpConfigurationExtensions.CreateODataRootContainer), a
    PerRouteContainer instance will be used to:

    -   Create an instance of DefaultContainerBuilder populated with
        default OData services noted in above;

    -   Override the ODL's default ODataUriResolver service instance in
        the container builder with WebApi's new default for
        *UnqualifiedODataUriResover with EnableCaseInsensitive=true*.

        > protected IContainerBuilder CreateContainerBuilderWithCoreServices()
        >
        > {
        > 
        >     //......
        >
        >     builder.AddDefaultODataServices();
        >
        >     // Set Uri resolver to by default enabling unqualified functions/actions and case insensitive match.
        >
        >     builder.AddService(
        >
        >         ServiceLifetime.Singleton,
        >
        >         typeof(ODataUriResolver),
        >
        >         sp =\> new UnqualifiedODataUriResolver {EnableCaseInsensitive = true});
        >
        >     return builder;
        >
        > }

    -   WebAPI client (per service) can further inject other dependencies
    (for example, typically, adding the EDM model) through
    the'configureAction' argument of the following method from
    HttpConfigurationExtensions:

        > internal static IServiceProvider CreateODataRootContainer(this HttpConfiguration configuration, string routeName, Action\<IContainerBuilder\> configureAction)

###  ODataUriParser configuration with injected ODataUriResolver dependency

When WebApi parses the request Uri, instance of ODataDefaultPathHandler is created with associated service provider container, which is further used to create ODataUriParser with injected dependency of ODataUriResolver.

    public ODataUriParser(IEdmModel model, Uri relativeUri, IServiceProvider container)

Enable Case-Insensitive for Custom Uri function
-----------------------------------------------

One issue is encountered when trying to bind function call token with
case-insensitive enabled. The reason is that at the very beginning of
the function BindAsUriFunction() the name token, when case-insensitive
is enabled, is coerced to lower case (as shown below), which is valid for
build-in function (such as 'startswith' and 'geo.distance', etc), but
might not be valid for custom uri functions.

> private QueryNode BindAsUriFunction(FunctionCallToken functionCallToken, List\<QueryNode\> argumentNodes)
>
> {
>
>     if (functionCallToken.Source != null)
>
>     {
>
>         // the parent must be null for a Uri function.
>
>         throw new ODataException(ODataErrorStrings.FunctionCallBinder\_UriFunctionMustHaveHaveNullParent(functionCallToken.Name));
>
>     }
>
>     string functionCallTokenName = this.state.Configuration.EnableCaseInsensitiveUriFunctionIdentifier ? functionCallToken.Name.ToLowerInvariant() : functionCallToken.Name;

To implement with the correct behavior for enabled case-insensitive:

-   GetUriFunctionSignatures needs to additionally return signatures associated with function names in a dictionary instance.

-   When resolving best match function based on arguments for the invoked function, MatchSignatureToUriFunction will find the best match. Exception is still thrown in case of ambiguity or no matches found.

Work Items
==========

- WebApi PR
  - [Set default to allow Unqualified-function/action and case insensitiveness](https://github.com/OData/WebApi/pull/1409)

- ODL PRs (for dependency fixes)
  - [Fix case insensitive search for custom URI functions](https://github.com/OData/odata.net/pull/1156)
  - [Fix SegmentKeyHandler to include UnqualifiedODataUriResolver proccessing](https://github.com/OData/odata.net/pull/1154)
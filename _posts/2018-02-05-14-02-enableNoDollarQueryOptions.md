---
layout: post
title : "14.2 Simplified optional-$-prefix for OData query option for WebAPI query parsing"
description: "7.x WebAPI query parser use optional-$-prefix for OData query option"
category: "14. 7.x Features "
---

Since ODL-6.x, **OData Core Library** supports query option with optional-$-prefix as described in [this docs](http://odata.github.io/odata.net/v7/#01-05-di-support).

Corresponding support on **WebAPI** layer is available starting WebAPI-7.4.

As result, WebAPI is able to process OData system query with optional $-prefix, as in "GET ~/?filter=id eq 33" with injected dependency setting:
~~~csharp
    ODataUriResolver.EnableNoDollarQueryOptions = true.
~~~

### ODL Enhancement
A public boolean attribute EnableNoDollarQueryOptions is added to ODataUriResolver. Public attribute is needed for dependency injection on the WebAPI layer.
~~~csharp
    public class ODataUriResolver
    {
        ...
        public virtual bool EnableNoDollarQueryOptions { get; set; }
        ...
    }
~~~

### WebAPI optional-$-prefix Setting using Dependency Injection
WebAPI service injects the setting using the ODataUriResolver during service initialization:
Builder of service provider container sets the instantiated ODataUriResover config using dependency injection.
~~~csharp
            ODataUriResolver resolver = new ODataUriResolver
            {
                EnableNoDollarQueryOptions = true,
                EnableCaseInsensitive = enableCaseInsensitive

            };
            
            spContainerBuilder.AddService(ServiceLifetime.Singleton, sp => resolver));
~~~
Note that UriResolver is typically a singleton for the service instance, since each instance should follow the same Uri convention. In case of other injected dependencies that are configurable per request, scoped dependency should be used.

### WebAPI Internal Processing of optional-$-prefix Setting
1. WebAPI EnableQuery attribute processing instantiates WebAPI's ODataQueryOptions object for incoming request.
2. The ODataQueryOptions constructor pins down the optional-$-prefix setting (see _enableNoDollarSignQueryOptions) from the injected ODataUriResolver.
3. Based on the optional-$-prefix setting, ODataQueryOptions parses the request Uri in WebAPI layer accordingly.

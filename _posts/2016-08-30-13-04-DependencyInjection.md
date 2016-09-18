---
layout: post
title : "13.4 Dependency Injection Support"
description: "Dependency injection support"
category: "13. 6.x Features "
---

Since [Web API OData V6.0.0 beta](https://www.nuget.org/packages/Microsoft.AspNet.OData/6.0.0-beta2), we have integrated with the popular dependency injection (DI) framework [Microsoft.Extensions.DependencyInjection](http://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/). By means of DI, we can significantly improve the extensibility of Web API OData as well as simplify the APIs exposed to the developers. Meanwhile, we have incorporated DI support throughout the whole OData stack (including ODataLib, Web API OData and RESTier) thus the three layers can consistently share services and custom implementations via the unified DI container in an OData service. For example, if you register an `ODataPayloadValueConverter` in a RESTier API class, the low-level ODataLib will be aware of that and use it automatically because they share the same DI container.

For the fundamentals of DI support in OData stacks, please refer to [this docs](http://odata.github.io/odata.net/v7/#01-05-di-support) from ODataLib. After understanding that, we can now take a look at how Web API OData implements the container, takes use of it and injects it into ODataLib.

### Implement the Container Builder
By default, if you don't provide a custom container builder, Web API OData will use the `DefaultContainerBuilder` which implements `IContainerBuilder` from ODataLib. The default implementation is based on the Microsoft DI framework introduced above and what it does is just delegating the builder operations to the underlying `ServiceCollection`.

But if you want to use a different DI framework (e.g., Autofac) or make some customizations to the default behavior, you will need to either implement your own container builder from `IContainerBuilder` or inherit from the `DefaultContainerBuilder`. For the former one, please refer to [the docs](http://odata.github.io/odata.net/v7/#01-05-di-support) from ODataLib. For the latter one, here is a simple example to illustrate how to customize the default container builder.

{% highlight csharp %}
public class MyContainerBuilder : DefaultContainerBuilder
{
    public override IContainerBuilder AddService(ServiceLifetime lifetime, Type serviceType, Type implementationType)
    {
        if (serviceType == typeof(ITestService))
        {
            // Force the implementation type of ITestService to be TestServiceImpl.
            base.AddService(lifetime, serviceType, typeof(TestServiceImpl));
        }

        return base.AddService(lifetime, serviceType, implementationType);
    }

    public override IServiceProvider BuildContainer()
    {
        return new MyContainer(base.BuildContainer());
    }
}

public class MyContainer : IServiceProvider
{
    private readonly IServiceProvider inner;

    public MyContainer(IServiceProvider inner)
    {
        this.inner = inner;
    }

    public object GetService(Type serviceType)
    {
        if (serviceType == typeof(ITestService))
        {
            // Force to create a TestServiceImpl2 instance for ITestService.
            return new TestServiceImpl2();
        }

        return base.GetService(serviceType);
    }
}
{% endhighlight %}

After implementing the container builder, you need to register that container builder in `HttpConfiguration` to tell Web API OData that you want to use your custom one. Please note that you MUST call `UseCustomContainerBuilder` BEFORE `MapODataServiceRoute` and `EnableDependencyInjection` because the root container will be actually created in these two methods. Setting the container builder factory after its creation is meaningless. Of course, if you wish to keep the default container builder implementation, `UseCustomContainerBuilder` doesn't need to be called at all.

{% highlight csharp %}
configuration.UseCustomContainerBuilder(() => new MyContainerBuilder());
configuration.MapODataServiceRoute(...);
{% endhighlight %}

### Register the Required Services
Basic APIs to register the services have already been [documented here](http://odata.github.io/odata.net/v7/#01-05-di-support). Here we mainly focus on the APIs from Web API OData that help to register the services into the container builder. The key API to register the required services for an OData service is an overload of `MapODataServiceRoute` which takes a `configureAction` to configure the container builder (i.e., register the services).

{% highlight csharp %}
public static class HttpConfigurationExtensions
{
    public static ODataRoute MapODataServiceRoute(this HttpConfiguration configuration, string routeName,
            string routePrefix, Action<IContainerBuilder> configureAction);
}
{% endhighlight %}

Theoretically you can register any service within the `configureAction` but there are two mandatory services that you are required to register: the `IEdmModel` and a collection of `IRoutingConvention`. Without them, the OData service you build will NOT work correctly. Here is an example of calling the API where a custom batch handler `MyBatchHandler` is registered. You are free to register any other service you like to the `builder`.

{% highlight csharp %}
configuration.MapODataServiceRoute(routeName: "odata", routePrefix: "odata", builder =>
    builder.AddService<IEdmModel>(ServiceLifetime.Singleton, sp => model)
           .AddService<ODataBatchHandler, MyBatchHandler>(ServiceLifetime.Singleton)
           .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
               ODataRoutingConventions.CreateDefaultWithAttributeRouting(routeName, configuration)));
{% endhighlight %}

You might also find that we still preserve the previous overloads of `MapODataServiceRoute` which take batch handlers, path handlers, HTTP message handlers, etc. They are basically wrapping the first overload that takes a `configureAction`. The reason why we keep them is that we want to give the users convenience to create OData services and bearings to the APIs they are familiar with.

Once you have called any of the `MapODataServiceRoute` overloads, the dependency injection for that OData route is enabled and an associated root container is created. As we internally maintain a dictionary to map the route name to its corresponding root container (1-1 mapping), multiple OData routes (i.e., calling `MapODataServiceRoute` multiple times) are still working great and the services registered in different containers (or routes) will not impact each other. That said, if you want a custom batch handler to work in the two OData routes, register them twice.

### Enable Dependency Injection for HTTP Routes
It's also possible that you don't want to create OData routes but just HTTP routes. The dependency injection support will NOT be enabled right after you call `MapHttpRoute`. In this case, you have to call `EnableDependencyInjection` to enable the dependency injection support for ALL HTTP routes. Please note that all the HTTP routes share the SAME root container which is of course different from the one of any OData route. That said calling `EnableDependencyInjection` has nothing to do with `MapODataServiceRoute`.

{% highlight csharp %}
configuration.MapHttpRoute(...);
configuration.EnableDependencyInjection();
{% endhighlight %}

Please also note that the order of `MapHttpRoute` and `EnableDependencyInjection` doesn't matter because they have no dependency on each other.

### Manage and Access the Request Container
Given a root container, we can create scoped containers from it, which is also known as request containers. Mostly you don't need to manage the creation and destruction of request containers yourself but there are some rare cases you have to touch them. Say you want to implement your custom batch handler, you have the full control of the multi-part batch request. You parse and split it into several batch parts (or sub requests) then you will be responsible for creating and destroying the request containers for the parts. They are implemented as extension methods to `HttpRequestMessage` in `HttpRequestMessageExtensions`.

To create the request container, you need to call the following extension method on a request. If you are creating the request container for a request that comes from an HTTP route, just pass `null` for the `routeName`.

{% highlight csharp %}
public static class HttpRequestMessageExtensions
{
    // Example:
    //   IServiceProvider requestContainer = request.CreateRequestContainer("odata");
    //   IServiceProvider requestContainer = request.CreateRequestContainer(null);
    public static IServiceProvider CreateRequestContainer(this HttpRequestMessage request, string routeName);
}
{% endhighlight %}

To delete the request container from a request, you need to call the following extension method on a request. The parameter `dispose` indicates whether to dispose that request container after deleting it from the request. Disposing a request container means that all the scoped and transient services within that container will also be disposed if they implement `IDisposable`.

{% highlight csharp %}
public static class HttpRequestMessageExtensions
{
    // Example:
    //   request.DeleteRequestContainer(true);
    //   request.DeleteRequestContainer(false);
    public static void DeleteRequestContainer(this HttpRequestMessage request, bool dispose)
}
{% endhighlight %}

To get the request container associated with that request, simply call the following extension method on a request. Note that you don't need to provide the route name to get the request container because the container itself has already been stored in the request properties during `CreateRequestContainer`. There is also a little trick in `GetRequestContainer` that if you have never called `CreateRequestContainer` on the request but directly call `GetRequestContainer`, it will try to create the request container for all the HTTP routes and return that container. Thus the return value of `GetRequestContainer` should never be `null`.

{% highlight csharp %}
public static class HttpRequestMessageExtensions
{
    // Example:
    //   IServiceProvider requestContainer = request.GetRequestContainer();
    public static IServiceProvider GetRequestContainer(this HttpRequestMessage request)
}
{% endhighlight %}

Please DO pay attention to the lifetime of the services. DON'T forget to delete and dispose the request container if you create it yourself. And scoped services will be disposed after the request completes.

### Services Available in Web API OData
Currently services Available in Web API OData include:

 - `IODataPathHandler` whose default implementation is `DefaultODataPathHandler` and lifetime is `Singleton`.
 - `XXXQueryValidator` whose lifetime are all `Singleton`.
 - `ODataXXXSerializer` and `ODataXXXDeserializer` whose lifetime are all `Singleton`. But please note that they are ONLY effective when `DefaultODataSerializerProvider` and `DefaultODataDeserializerProvider` are present. Custom serializer and deserializer providers are NOT guaranteed to call those serializers and deserializers from the DI container.
 - `ODataSerializerProvider` and `ODataDeserializerProvider` whose implementation types are `DefaultODataSerializerProvider` and `DefaultODataDeserializerProvider` respectively and lifetime are all `Singleton`. Please note that you might lose all the default serializers and deserializers registered in the DI container if you don't call into the default providers in your own providers.
 - `IAssembliesResolver` whose implementation type is the default one from ASP.NET Web API.
 - `FilterBinder` whose implementation type is `Transient` because each `EnableQueryAttribute` instance will create its own `FilterBinder`. Override it if you want to customize the process of binding a $filter syntax tree.
 
#### Services Avaliable in OData Lib
[Services in OData Lib also can be injected through Web API OData](http://odata.github.io/odata.net/v7/#01-05-di-support).

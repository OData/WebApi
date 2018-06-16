---
layout: post
title: "4.20 Prefer odata.continue-on-error"
description: ""
category: "4. OData features"
---

Since OData Web API V5.7, it supports <strong>[odata.continue-on-error](http://docs.oasis-open.org/odata/odata/v4.0/errata02/os/complete/part1-protocol/odata-v4.0-errata02-os-part1-protocol-complete.html#_Toc406398236)</strong>.

### Enable odata.continue-on-error

Users should call the following API to enable continue on error

- For Microsoft.AspNet.OData (supporting classic ASP.NET Framework):

    {% highlight csharp %}
        var configuration = new HttpConfiguration();
        configuration.EnableContinueOnErrorHeader();
    {% endhighlight %}

- For Microsoft.AspNetCore.OData (supporting ASP.NET Core):

   It can be enabled in the service's HTTP request pipeline configuration method `Configure(IApplicationBuilder app, IHostingEnvironment env)` of the typical `Startup` class:

    {% highlight csharp %}
        app.UseMvc(routeBuilder =>
        {
           routeBuilder.Select().Expand().Filter().OrderBy().MaxTop(100).Count()
                        .EnableContinueOnErrorHeader();	 // Additional configuration to enable continue on error.
           routeBuilder.MapODataServiceRoute("ODataRoute", "odata", builder.GetEdmModel());
       });
    {% endhighlight %}

#### Prefer odata.continue-on-error

We can use the following codes to prefer continue on error

{% highlight csharp %}
HttpRequestMessage request = new HttpRequestMessage(...);
request.Headers.Add("Prefer", "odata.continue-on-error");
request.Content = new StringContent(...);
request.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/mixed; boundary=batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0");
HttpResponseMessage response = client.SendAsync(request).Result;
...
{% endhighlight %}

The response will have all responses, includes the error responses.

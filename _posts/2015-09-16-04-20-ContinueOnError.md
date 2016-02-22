---
layout: post
title: "4.20 Prefer odata.continue-on-error"
description: ""
category: "4. OData features"
---

Since OData Web API V5.7, it supports <strong>[odata.continue-on-error](http://docs.oasis-open.org/odata/odata/v4.0/errata02/os/complete/part1-protocol/odata-v4.0-errata02-os-part1-protocol-complete.html#_Toc406398236)</strong>.

### Enable odata.continue-on-error

Users should call the following API to enable continue on error:
{% highlight csharp %}
var configuration = new HttpConfiguration();
configuration.EnableContinueOnErrorHeader();
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

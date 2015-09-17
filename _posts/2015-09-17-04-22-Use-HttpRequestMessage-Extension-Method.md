---
layout: post
title: "4.22 Use HttpRequestMessage Extension Methods"
description: ""
category: "4. OData Features"
---

HttpRequestMessage provides set of exention methods. For services that don't use LINQ or ODataQueryOptions.ApplyTo(), those extention methods can offer lots of help. 

## ODataProperties
OData methods and properties can be GET/SET through ODataProperties, including:
	 
<strong>Model</strong>
The EDM model associated with the request.
<strong>Path</strong>
The ODataPath of the request.
<strong>PathHandler</strong>  
By default, it would return DefaultODataPathHandler.
<strong>RouteName</strong>
The Route name for generating OData links.
<strong>SelectExpandClause</strong>
The parsed the OData SelectExpandClause of the request.
<strong>NextLink</strong> 
Next page link of the results, can be set through GetNextPageLink.

For example, we may need generate service root when querying ref link of a navigation property. 
{% highlight csharp %}
private string GetServiceRootUri()
{
  var routeName = Request.ODataProperties().RouteName;
  ODataRoute odataRoute = Configuration.Routes[routeName] as ODataRoute;
  var prefixName = odataRoute.RoutePrefix;
  var requestUri = Request.RequestUri.ToString();
  var serviceRootUri = requestUri.Substring(0, requestUri.IndexOf(prefixName) + prefixName.Length);
  return serviceRootUri;
}
{% endhighlight %}
  
## GetNextPageLink
Create a link for the next page of results, can be used as the value of `@odata.nextLink`.
For example, the request Url is `http://localhost/Customers/?$select=Name`.
{% highlight csharp %}
Uri nextlink = this.Request.GetNextPageLink(pageSize:10);
this.Request.ODataProperties().NextLink = nextlink;
{% endhighlight %}
Then the nextlink generated is `http://localhost/Customers/?$select=Name&$skip=10`.

## GetETag
Get the etag for the given request.
{% highlight csharp %}
EntityTagHeaderValue etagHeaderValue = this.Request.Headers.IfMatch.SingleOrDefault();
ETag etag = this.Request.GetETag(etagHeaderValue);
{% endhighlight %}

## CreateErrorResponse
Create a HttpResponseMessage to represent an error.
{% highlight csharp %}
public HttpResponseMessage Post()
{
  ODataError error = new ODataError()
  {
    ErrorCode = "36",
    Message = "Bad stuff",
    InnerError = new ODataInnerError()
    {
      Message = "Exception message"
    }
  };

  return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
}
{% endhighlight %}

Then payload would be like:

    {
      "error":{
      "code":"36",
      "message":"Bad stuff",
      "innererror":{
        "message":"Exception message",
        "type":"",
        "stacktrace":""
      }
    }

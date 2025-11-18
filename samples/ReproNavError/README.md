# Repro for issue #2890

This is a sample project that demonstrates the issue https://github.com/OData/WebApi/issues/2890

To reproduce the issue, run this sample project and make a request to

`GET http://localhost:5287/odata/GetAppUsage()/App1?$expand=KeyCredentials`

The response serialization should fail and in the output debug logs, you should see the following exception

```
Microsoft.OData.ODataException: The Path property in ODataMessageWriterSettings.ODataUri must be set when writing contained elements.
   at Microsoft.OData.ODataWriterCore.EnterScope(WriterState newState, ODataItem item)
   at Microsoft.OData.ODataWriterCore.WriteStartNestedResourceInfoImplementation(ODataNestedResourceInfo nestedResourceInfo)
   at Microsoft.OData.ODataWriterCore.<>c.<WriteStartAsync>b__64_0(ODataWriterCore thisParam, ODataNestedResourceInfo nestedResourceInfoParam)
   at Microsoft.OData.TaskUtils.GetTaskForSynchronousOperation[TArg1,TArg2](Action`2 synchronousOperation, TArg1 arg1, TArg2 arg2)
--- End of stack trace from previous location ---
   at Microsoft.AspNet.OData.Formatter.Serialization.ODataResourceSerializer.WriteExpandedNavigationPropertiesAsync(SelectExpandNode selectExpandNode, ResourceContext resourceContext, ODataWriter writer) in C:\Users\clhabins\source\repos\WebApi\src\Microsoft.AspNet.OData.Shared\Formatter\Serialization\ODataResourceSerializer.cs:line 1889
   at Microsoft.AspNet.OData.Formatter.Serialization.ODataResourceSerializer.WriteResourceAsync(Object graph, ODataWriter writer, ODataSerializerContext writeContext, IEdmTypeReference expectedType) in C:\Users\clhabins\source\repos\WebApi\src\Microsoft.AspNet.OData.Shared\Formatter\Serialization\ODataResourceSerializer.cs:line 1497
   at Microsoft.AspNet.OData.Formatter.Serialization.ODataResourceSerializer.WriteObjectAsync(Object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext) in C:\Users\clhabins\source\repos\WebApi\src\Microsoft.AspNet.OData.Shared\Formatter\Serialization\ODataResourceSerializer.cs:line 81
   at Microsoft.AspNet.OData.Formatter.ODataOutputFormatterHelper.WriteToStreamAsync(Type type, Object value, IEdmModel model, ODataVersion version, Uri baseAddress, MediaTypeHeaderValue contentType, IWebApiUrlHelper internaUrlHelper, IWebApiRequestMessage internalRequest, IWebApiHeaders internalRequestHeaders, Func`2 getODataMessageWrapper, Func`2 getEdmTypeSerializer, Func`2 getODataPayloadSerializer, Func`1 getODataSerializerContext) in C:\Users\clhabins\source\repos\WebApi\src\Microsoft.AspNet.OData.Shared\Formatter\ODataOutputFormatterHelper.cs:line 229
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResultFilterAsync>g__Awaited|30_0[TFilter,TFilterAsync](ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResultExecutedContextSealed context)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.ResultNext[TFilter,TFilterAsync](State& next, Scope& scope, Object& state, Boolean& isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.InvokeResultFilters()
--- End of stack trace from previous location ---
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.InvokeFilterPipelineAsync()
--- End of stack trace from previous location ---
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
   at Microsoft.AspNetCore.Builder.RouterMiddleware.Invoke(HttpContext httpContext)
   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
   at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
   at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware: Warning: The response has already started, the error page middleware will not be executed.
Exception thrown: 'Microsoft.OData.ODataException' in System.Private.CoreLib.dll
Exception thrown: 'Microsoft.OData.ODataException' in System.Private.CoreLib.dll
Exception thrown: 'Microsoft.OData.ODataException' in System.Private.CoreLib.dll
Exception thrown: 'Microsoft.OData.ODataException' in System.Private.CoreLib.dll
```
// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames", Justification = "The assembly is delay signed")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Http.Controllers")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Http.Dispatcher")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Http.Hosting")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Http.Metadata")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Http.Validation.ClientRules")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Http.ValueProviders")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Http.ValueProviders.Providers")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Http.Services")]
[assembly: SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member", Target = "System.Web.Http.Filters.AuthorizationFilterAttribute.#System.Web.Http.Filters.IAuthorizationFilter.ExecuteAuthorizationFilterAsync(System.Web.Http.Controllers.HttpActionContext,System.Threading.CancellationToken,System.Func`1<System.Threading.Tasks.Task`1<System.Net.Http.HttpResponseMessage>>)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member", Target = "System.Web.Http.Filters.ExceptionFilterAttribute.#System.Web.Http.Filters.IExceptionFilter.ExecuteExceptionFilterAsync(System.Web.Http.Filters.HttpActionExecutedContext,System.Threading.CancellationToken)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member", Target = "System.Web.Http.Filters.ActionFilterAttribute.#System.Web.Http.Filters.IActionFilter.ExecuteActionFilterAsync(System.Web.Http.Controllers.HttpActionContext,System.Threading.CancellationToken,System.Func`1<System.Threading.Tasks.Task`1<System.Net.Http.HttpResponseMessage>>)")]
[assembly: SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Scope = "member", Target = "System.Web.Http.Tracing.Tracers.ActionInvokerTracer.#System.Web.Http.Controllers.IHttpActionInvoker.InvokeActionAsync(System.Web.Http.Controllers.HttpActionContext,System.Threading.CancellationToken)", Justification = "Tracing layer needs to observe all Task completion paths")]
[assembly: SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Scope = "member", Target = "System.Web.Http.Tracing.Tracers.ApiControllerTracer.#System.Web.Http.Controllers.IHttpController.ExecuteAsync(System.Web.Http.Controllers.HttpControllerContext,System.Threading.CancellationToken)", Justification = "Tracing layer needs to observe all Task completion paths")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Net.Http.Formatting", Justification = "Namespace follows folder structure")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Http.Dependencies", Justification = "Namespace follows folder structure")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Http.Tracing.Tracers", Justification = "Namespace follows folder structure")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Http.Validation.Validators", Justification = "Namespace follows folder structure")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Net.Http", Justification = "Functionality logically belongs in a namespace defined in a different binary")]

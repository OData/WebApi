// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Dependencies;
using System.Web.Http.Dispatcher;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// Provides a mechanism for a <see cref="IHttpController"/> implementation to indicate 
    /// what kind of <see cref="IHttpControllerActivator"/>, <see cref="IHttpActionSelector"/>, <see cref="IActionValueBinder"/>
    /// and <see cref="IHttpActionInvoker"/> to use for that controller. The types are 
    /// first looked up in the <see cref="IDependencyResolver"/> and if not found there
    /// then created directly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class HttpControllerConfigurationAttribute : Attribute
    {
        public Type HttpControllerActivator { get; set; }

        public Type HttpActionSelector { get; set; }

        public Type HttpActionInvoker { get; set; }

        public Type ActionValueBinder { get; set; }
    }
}

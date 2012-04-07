// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Microsoft.Web.Http.Data
{
    /// <summary>
    /// This descriptor translates between the wire format for Submit (an enumerable
    /// of ChangeSetEntry) and the actual Submit(ChangeSet) signature.
    /// </summary>
    internal sealed class SubmitActionDescriptor : ReflectedHttpActionDescriptor
    {
        private const string ChangeSetParameterName = "changeSet";
        private Collection<HttpParameterDescriptor> _parameters;

        public SubmitActionDescriptor(HttpControllerDescriptor controllerDescriptor, Type controllerType)
            : base(controllerDescriptor, controllerType.GetMethod("Submit", BindingFlags.Instance | BindingFlags.Public))
        {
            _parameters = new Collection<HttpParameterDescriptor>(new List<HttpParameterDescriptor>() { new ChangeSetParameterDescriptor(this) });
        }

        public override Type ReturnType
        {
            get { return typeof(HttpResponseMessage); }
        }

        public override Collection<HttpParameterDescriptor> GetParameters()
        {
            return _parameters;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller owns HttpResponseMessage.")]
        public override Task<object> ExecuteAsync(HttpControllerContext controllerContext, IDictionary<string, object> arguments)
        {
            return TaskHelpers.RunSynchronously<object>(() =>
            {
                // create a changeset from the entries
                ChangeSet changeSet = new ChangeSet((IEnumerable<ChangeSetEntry>)arguments[ChangeSetParameterName]);
                changeSet.SetEntityAssociations();

                DataController controller = (DataController)controllerContext.Controller;
                if (!controller.Submit(changeSet) &&
                    controller.ActionContext.Response != null)
                {
                    // If the submit failed due to an authorization failure,
                    // return the authorization response directly
                    return controller.ActionContext.Response;
                }

                // return the entries
                return controllerContext.Request.CreateResponse<ChangeSetEntry[]>(HttpStatusCode.OK, changeSet.ChangeSetEntries.ToArray());
            });
        }

        /// <summary>
        /// ParameterDescriptor representing the single Submit(ChangeSet) parameter,
        /// but with the enumerable of ChangeSetEntry ParameterType so the formatters
        /// deserialize the argument properly.
        /// </summary>
        private class ChangeSetParameterDescriptor : ReflectedHttpParameterDescriptor
        {
            public ChangeSetParameterDescriptor(HttpActionDescriptor actionDescriptor)
                : base(actionDescriptor, actionDescriptor.ControllerDescriptor.ControllerType.GetMethod("Submit", BindingFlags.Instance | BindingFlags.Public).GetParameters()[0])
            {
            }

            public override Type ParameterType
            {
                get { return typeof(ChangeSetEntry[]); }
            }
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace Microsoft.Web.Http.Data
{
    /// <summary>
    /// This descriptor translates a direct CUD action invocation into a call to
    /// Submit. This descriptor wraps the actual action, intercepting the execution
    /// to do the transformation.
    /// </summary>
    internal sealed class SubmitProxyActionDescriptor : ReflectedHttpActionDescriptor
    {
        private UpdateActionDescriptor _updateAction;

        public SubmitProxyActionDescriptor(UpdateActionDescriptor updateAction)
            : base(updateAction.ControllerDescriptor, updateAction.ControllerDescriptor.ControllerType.GetMethod("Submit", BindingFlags.Instance | BindingFlags.Public))
        {
            _updateAction = updateAction;
        }

        public override string ActionName
        {
            get { return _updateAction.ActionName; }
        }

        public override Type ReturnType
        {
            get { return typeof(HttpResponseMessage); }
        }

        public override Collection<HttpParameterDescriptor> GetParameters()
        {
            return _updateAction.GetParameters();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for the lifetime of the object")]
        public override Task<object> ExecuteAsync(HttpControllerContext controllerContext, IDictionary<string, object> arguments)
        {
            return TaskHelpers.RunSynchronously<object>(() =>
            {
                // create the changeset
                object entity = arguments.Single().Value; // there is only a single parameter - the entity being submitted
                ChangeSetEntry[] changeSetEntries = new ChangeSetEntry[]
                {
                    new ChangeSetEntry
                    {
                        Id = 1,
                        ActionDescriptor = _updateAction,
                        Entity = entity,
                        Operation = _updateAction.ChangeOperation
                    }
                };
                ChangeSet changeSet = new ChangeSet(changeSetEntries);
                changeSet.SetEntityAssociations();

                DataController controller = (DataController)controllerContext.Controller;
                if (!controller.Submit(changeSet) &&
                    controller.ActionContext.Response != null)
                {
                    // If the submit failed due to an authorization failure,
                    // return the authorization response directly
                    return controller.ActionContext.Response;
                }

                // return the entity
                entity = changeSet.ChangeSetEntries[0].Entity;
                // REVIEW does JSON make sense here?
                return new HttpResponseMessage()
                {
                    Content = new ObjectContent(_updateAction.EntityType, entity, new JsonMediaTypeFormatter())
                };
            });
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Microsoft.Web.Http.Data
{
    [DebuggerDisplay("Action = {ActionName}, Type = {EntityType.Name}, Operation = {ChangeOperation}")]
    public class UpdateActionDescriptor : ReflectedHttpActionDescriptor
    {
        private readonly ChangeOperation _changeOperation;
        private readonly Type _entityType;
        private readonly MethodInfo _method;

        public UpdateActionDescriptor(HttpControllerDescriptor controllerDescriptor, MethodInfo method, Type entityType, ChangeOperation operationType)
            : base(controllerDescriptor, method)
        {
            _entityType = entityType;
            _changeOperation = operationType;
            _method = method;
        }

        public Type EntityType
        {
            get { return _entityType; }
        }

        public ChangeOperation ChangeOperation
        {
            get { return _changeOperation; }
        }

        public bool Authorize(HttpActionContext context)
        {
            // We only select Action scope Authorization filters, since Global and Class level filters will already
            // be executed when Submit is invoked. We only look at AuthorizationFilterAttributes because we are only
            // interested in running synchronous (i.e., quick to run) attributes.
            IEnumerable<AuthorizationFilterAttribute> authFilters =
                GetFilterPipeline()
                    .Where(p => p.Scope == FilterScope.Action)
                    .Select(p => p.Instance)
                    .OfType<AuthorizationFilterAttribute>();

            foreach (AuthorizationFilterAttribute authFilter in authFilters)
            {
                authFilter.OnAuthorization(context);

                if (context.Response != null && !context.Response.IsSuccessStatusCode)
                {
                    return false;
                }
            }

            return true;
        }

        public override Task<object> ExecuteAsync(HttpControllerContext controllerContext, IDictionary<string, object> arguments)
        {
            return TaskHelpers.RunSynchronously(() =>
            {
                DataController controller = (DataController)controllerContext.Controller;
                object[] paramValues = arguments.Select(p => p.Value).ToArray();

                return _method.Invoke(controller, paramValues);
            });
        }
    }
}

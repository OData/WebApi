// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace System.Web.Http.Routing
{
    internal class DirectRouteBuilder
    {
        private readonly ReflectedHttpActionDescriptor[] _actions;

        private string _template;

        public DirectRouteBuilder(IEnumerable<ReflectedHttpActionDescriptor> actions)
        {
            if (actions == null)
            {
                throw new ArgumentNullException("actions");
            }

            _actions = actions.ToArray();
        }

        public string Name { get; set; }

        public string Template
        {
            get
            {
                return _template;
            }
            set
            {
                ParsedRoute = null;
                _template = value;
            }
        }

        public HttpRouteValueDictionary Defaults { get; set; }

        public HttpRouteValueDictionary Constraints { get; set; }

        public HttpRouteValueDictionary DataTokens { get; set; }

        public HttpMessageHandler Handler { get; set; }

        internal HttpParsedRoute ParsedRoute { get; set; }

        public int Order { get; set; }

        public decimal Precedence { get; set; }

        public IEnumerable<ReflectedHttpActionDescriptor> Actions
        {
            get { return _actions; }
        }

        public virtual HttpRouteEntry Build()
        {
            HttpRouteValueDictionary dataTokens = DataTokens;

            if (dataTokens == null)
            {
                dataTokens = new HttpRouteValueDictionary();
            }

            dataTokens[RouteKeys.ActionsDataTokenKey] = _actions;

            int order = Order;

            if (order != default(int))
            {
                dataTokens[RouteKeys.OrderDataTokenKey] = order;
            }

            decimal precedence = Precedence;

            if (precedence != default(decimal))
            {
                dataTokens[RouteKeys.PrecedenceDataTokenKey] = precedence;
            }

            IHttpRoute route = new HttpRoute(Template, Defaults, Constraints, dataTokens, Handler, ParsedRoute);
            return new HttpRouteEntry(Name, route);
        }
    }
}

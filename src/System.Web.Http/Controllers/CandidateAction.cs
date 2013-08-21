// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Web.Http.Controllers;

namespace System.Web.Http.Routing
{
    [DebuggerDisplay("{ActionDescriptor.ActionName}, Order={Order}, Prec={Precedence}")]
    internal struct CandidateAction
    {
        public ReflectedHttpActionDescriptor ActionDescriptor { get; set; }
        public int Order { get; set; }
        public decimal Precedence { get; set; }

        public bool MatchName(string actionName)
        {
            return String.Equals(ActionDescriptor.ActionName, actionName, StringComparison.OrdinalIgnoreCase);
        }
    }
}

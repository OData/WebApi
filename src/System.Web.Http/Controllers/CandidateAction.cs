// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http.Controllers;
using System.Web.Http.Properties;

namespace System.Web.Http.Routing
{
    internal struct CandidateAction
    {
        public ReflectedHttpActionDescriptor ActionDescriptor { get; set; }
        public int Order { get; set; }
        public decimal Precedence { get; set; }
    }
}

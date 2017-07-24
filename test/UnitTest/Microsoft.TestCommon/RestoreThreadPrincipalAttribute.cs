﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Security.Principal;
using System.Threading;

namespace Microsoft.TestCommon
{
    public class RestoreThreadPrincipalAttribute : Xunit.BeforeAfterTestAttribute
    {
        private IPrincipal _originalPrincipal;

        public override void Before(MethodInfo methodUnderTest)
        {
            _originalPrincipal = Thread.CurrentPrincipal;

            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.NoPrincipal);

            Thread.CurrentPrincipal = null;
        }

        public override void After(MethodInfo methodUnderTest)
        {
            Thread.CurrentPrincipal = _originalPrincipal;

            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.UnauthenticatedPrincipal);
        }
    }
}

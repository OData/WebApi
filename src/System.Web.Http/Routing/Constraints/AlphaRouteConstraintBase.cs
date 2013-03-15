// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.Routing.Constraints
{
    /// <summary>
    /// Constrains a url parameter to contain only letters from the alphabet.
    /// </summary>
    public class AlphaHttpRouteConstraintBase : RegexHttpRouteConstraint
    {
        public AlphaHttpRouteConstraintBase() : base(@"^[A-Za-z]*$") {}
    }
}
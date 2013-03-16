// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http.Routing.Constraints;

namespace System.Web.Http.Routing
{
    public class DefaultInlineRouteConstraintResolver : IInlineRouteConstraintResolver
    {
        private readonly IDictionary<string, Type> _inlineRouteConstraintMap = GetDefaultInlineRouteConstraints();

        public IHttpRouteConstraint ResolveConstraint(string inlineConstraint)
        {
            string[] arguments;
            int indexOfFirstOpenParens = inlineConstraint.IndexOf('(');
            if (indexOfFirstOpenParens >= 0 && inlineConstraint.EndsWith(")", StringComparison.Ordinal))
            {
                string argumentString = inlineConstraint.Substring(indexOfFirstOpenParens + 1, inlineConstraint.Length - indexOfFirstOpenParens - 2);
                inlineConstraint = inlineConstraint.Substring(0, indexOfFirstOpenParens);
                arguments = argumentString.Split(',');
            }
            else
            {
                arguments = new string[0];
            }

            // TODO: parse parameters
            return ResolveConstraint(inlineConstraint, arguments);
        }

        internal IHttpRouteConstraint ResolveConstraint(string constraintKey, string[] arguments)
        {
            // Make sure the key exists in the key/Type map.
            if (!_inlineRouteConstraintMap.ContainsKey(constraintKey))
            {
                throw Error.KeyNotFound(
                    "Could not resolve a route constraint for the key '{0}'.", constraintKey);
            }

            Type httpRouteConstraintType = typeof(IHttpRouteConstraint);
            Type type = _inlineRouteConstraintMap[constraintKey];

            return (IHttpRouteConstraint)Activator.CreateInstance(type, arguments.ToArray<object>());
        }

        private static IDictionary<string, Type> GetDefaultInlineRouteConstraints()
        {
            return new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { "alpha", typeof(AlphaHttpRouteConstraint) },
                { "bool", typeof(BoolHttpRouteConstraint) },
                { "datetime", typeof(DateTimeHttpRouteConstraint) },
                { "decimal", typeof(DecimalHttpRouteConstraint) },
                { "double", typeof(DoubleHttpRouteConstraint) },
                { "float", typeof(FloatHttpRouteConstraint) },
                { "guid", typeof(GuidHttpRouteConstraint) },
                { "int", typeof(IntHttpRouteConstraint) },
                { "length", typeof(LengthHttpRouteConstraint) },
                { "long", typeof(LongHttpRouteConstraint) },
                { "max", typeof(MaxHttpRouteConstraint) },
                { "maxlength", typeof(MaxLengthHttpRouteConstraint) },
                { "min", typeof(MinHttpRouteConstraint) },
                { "minlength", typeof(MinLengthHttpRouteConstraint) },
                { "range", typeof(RangeHttpRouteConstraint) },
                { "regex", typeof(RegexHttpRouteConstraint) }
            };
        }
    }
}

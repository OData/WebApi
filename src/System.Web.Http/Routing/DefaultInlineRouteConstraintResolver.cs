// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Routing.Constraints;

namespace System.Web.Http.Routing
{
    public class DefaultInlineRouteConstraintResolver : IInlineRouteConstraintResolver
    {
        private IDictionary<string, Type> _inlineRouteConstraintMap = GetDefaultInlineRouteConstraints();

        public IHttpRouteConstraint ResolveConstraint(string inlineConstraint)
        {
            // TODO: parse parameters
            return ResolveConstraint(inlineConstraint, Enumerable.Empty<string>());
        }

        public IHttpRouteConstraint ResolveConstraint(string constraintKey, IEnumerable<string> args)
        {
            // Make sure the key exists in the key/Type map.
            if (!_inlineRouteConstraintMap.ContainsKey(constraintKey))
            {
                // TODO: Throw via the Error helper.
                throw new KeyNotFoundException(
                    Error.Format("Could not resolve a route constraint for the key '{0}'.", constraintKey));
            }

            Type httpRouteConstraintType = typeof(IHttpRouteConstraint);
            Type type = _inlineRouteConstraintMap[constraintKey];

            // Make sure the type is an IHttpRouteConstraint.
            if (!httpRouteConstraintType.IsAssignableFrom(type))
            {
                throw Error.InvalidOperation(
                    "The type '{0}' must implement '{1}'", type.FullName, httpRouteConstraintType.FullName);
            }

            return (IHttpRouteConstraint)Activator.CreateInstance(type, args.ToArray<object>());
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

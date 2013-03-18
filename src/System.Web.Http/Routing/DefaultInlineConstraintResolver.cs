// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Http.Properties;
using System.Web.Http.Routing.Constraints;

namespace System.Web.Http.Routing
{
    /// <summary>
    /// The default implementation of <see cref="IInlineConstraintResolver"/>. Resolves constraints by parsing
    /// a constraint key and constraint arguments, using a map to resolve the constraint type, and calling an
    /// appropriate constructor for the constraint type.
    /// </summary>
    public class DefaultInlineConstraintResolver : IInlineConstraintResolver
    {
        private readonly IDictionary<string, Type> _inlineConstraintMap = GetDefaultConstraintMap();

        /// <summary>
        /// Gets the mutable dictionary that maps constraint keys to a particular constraint type.
        /// </summary>
        public IDictionary<string, Type> ConstraintMap
        {
            get
            {
                return _inlineConstraintMap;
            }
        }

        private static IDictionary<string, Type> GetDefaultConstraintMap()
        {
            return new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                // Type-specific constraints
                { "bool", typeof(BoolHttpRouteConstraint) },
                { "datetime", typeof(DateTimeHttpRouteConstraint) },
                { "decimal", typeof(DecimalHttpRouteConstraint) },
                { "double", typeof(DoubleHttpRouteConstraint) },
                { "float", typeof(FloatHttpRouteConstraint) },
                { "guid", typeof(GuidHttpRouteConstraint) },
                { "int", typeof(IntHttpRouteConstraint) },
                { "long", typeof(LongHttpRouteConstraint) },

                // Length constraints
                { "minlength", typeof(MinLengthHttpRouteConstraint) },
                { "maxlength", typeof(MaxLengthHttpRouteConstraint) },
                { "length", typeof(LengthHttpRouteConstraint) },
                
                // Min/Max value constraints
                { "min", typeof(MinHttpRouteConstraint) },
                { "max", typeof(MaxHttpRouteConstraint) },
                { "range", typeof(RangeHttpRouteConstraint) },

                // Regex-based constraints
                { "alpha", typeof(AlphaHttpRouteConstraint) },
                { "regex", typeof(RegexHttpRouteConstraint) }
            };
        }

        /// <inheritdoc />
        public virtual IHttpRouteConstraint ResolveConstraint(string inlineConstraint)
        {
            if (inlineConstraint == null)
            {
                throw Error.ArgumentNull("inlineConstraint");
            }

            string constraintKey;
            string argumentString;
            int indexOfFirstOpenParens = inlineConstraint.IndexOf('(');
            if (indexOfFirstOpenParens >= 0 && inlineConstraint.EndsWith(")", StringComparison.Ordinal))
            {
                constraintKey = inlineConstraint.Substring(0, indexOfFirstOpenParens);
                argumentString = inlineConstraint.Substring(indexOfFirstOpenParens + 1, inlineConstraint.Length - indexOfFirstOpenParens - 2);
            }
            else
            {
                constraintKey = inlineConstraint;
                argumentString = null;
            }

            Type constraintType;
            if (!_inlineConstraintMap.TryGetValue(constraintKey, out constraintType))
            {
                // Cannot resolve the constraint key
                return null;
            }

            if (!typeof(IHttpRouteConstraint).IsAssignableFrom(constraintType))
            {
                throw Error.InvalidOperation(SRResources.DefaultInlineConstraintResolver_TypeNotConstraint, constraintType.Name, constraintKey);
            }

            return CreateConstraint(constraintType, argumentString);
        }

        private static IHttpRouteConstraint CreateConstraint(Type constraintType, string argumentString)
        {
            Contract.Assert(typeof(IHttpRouteConstraint).IsAssignableFrom(constraintType));

            // No arguments - call the default constructor
            if (argumentString == null)
            {
                return (IHttpRouteConstraint)Activator.CreateInstance(constraintType);
            }

            ConstructorInfo activationConstructor = null;
            object[] parameters = null;
            ConstructorInfo[] constructors = constraintType.GetConstructors();

            // If there is only one constructor and it has a single parameter, pass the argument string directly
            // This is necessary for the RegexHttpRouteConstraint to ensure that patterns are not split on commas.
            if (constructors.Length == 1 && constructors[0].GetParameters().Length == 1)
            {
                activationConstructor = constructors[0];
                parameters = ConvertArguments(activationConstructor.GetParameters(), new string[] { argumentString });
            }
            else
            {
                string[] splitArguments = argumentString.Split(',');
                int argumentCount = splitArguments.Length;

                ConstructorInfo[] matchingConstructors = constructors.Where(ci => ci.GetParameters().Length == argumentCount).ToArray();
                int constructorMatches = matchingConstructors.Length;

                if (constructorMatches == 0)
                {
                    throw Error.InvalidOperation(SRResources.DefaultInlineConstraintResolver_CouldNotFindCtor, constraintType.Name, argumentString.Length);
                }
                else if (constructorMatches == 1)
                {
                    activationConstructor = matchingConstructors[0];
                    parameters = ConvertArguments(activationConstructor.GetParameters(), splitArguments);
                }
                else
                {
                    throw Error.InvalidOperation(SRResources.DefaultInlineConstraintResolver_AmbiguousCtors, constraintType.Name, argumentString.Length);
                }
            }

            return (IHttpRouteConstraint)activationConstructor.Invoke(parameters);
        }

        private static object[] ConvertArguments(ParameterInfo[] parameterInfos, string[] arguments)
        {
            object[] parameters = new object[parameterInfos.Length];
            for (int i = 0; i < parameterInfos.Length; i++)
            {
                ParameterInfo parameter = parameterInfos[i];
                Type parameterType = parameter.ParameterType;
                parameters[i] = Convert.ChangeType(arguments[i], parameterType, CultureInfo.InvariantCulture);
            }
            return parameters;
        }
    }
}

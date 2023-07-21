//-----------------------------------------------------------------------------
// <copyright file="ExpandQueryBuilder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// Exposes the ability to generate a $expand query parameter from a payload object.
    /// </summary>
    public class ExpandQueryBuilder : IExpandQueryBuilder
    {
        /// <inheritdoc />
        public virtual string GenerateExpandQueryParameter(object value, IEdmModel model)
        {
            if (value == null)
            {
                throw Error.ArgumentNull(nameof(value));
            }

            return GenerateExpandQueryStringInternal(value, model, false);
        }

        private string GenerateExpandQueryStringInternal(object value, IEdmModel model, bool isNestedExpand)
        {
            string expandString = "";

            if (value == null || model == null)
            {
                return expandString;
            }

            Type type = value.GetType();
            bool isCollection = TypeHelper.IsCollection(type, out Type elementType);
            IEnumerable collection = null;

            if (isCollection)
            {
                type = elementType;
                collection = value as IEnumerable;
            }

            string edmFullName = type.EdmFullName();

            IEdmStructuredType edmStructuredType = model.FindType(edmFullName) as IEdmStructuredType;

            if (edmStructuredType == null)
            {
                return expandString;
            }

            IEnumerable<IEdmNavigationProperty> navigationProperties = edmStructuredType.NavigationProperties();

            int count = 0;

            HashSet<string> navPropNames = new HashSet<string>();

            foreach (IEdmNavigationProperty navProp in navigationProperties)
            {
                count++;
                PropertyInfo prop = type.GetProperty(navProp.Name);

                if (isCollection)
                {
                    foreach (object obj in collection)
                    {
                        object navPropValue = prop.GetValue(obj);

                        if (navPropValue == null)
                        {
                            continue;
                        }

                        if (!navPropNames.Contains(navProp.Name))
                        {
                            expandString += !isNestedExpand ? "" : "(";
                            expandString += count > 1 ? "," + prop.Name : string.Concat("$expand=", prop.Name);
                            navPropNames.Add(navProp.Name);
                            expandString += GenerateExpandQueryStringInternal(navPropValue, model, true);
                        }
                        else
                        {
                            expandString = expandString.TrimEnd(')');
                            expandString += GenerateExpandQueryStringInternal(navPropValue, model, true);
                        }

                        expandString += !isNestedExpand ? "" : ")";
                    }
                }
                else
                {
                    object navPropValue = prop.GetValue(value);

                    if (navPropValue != null)
                    {
                        if (!navPropNames.Contains(navProp.Name))
                        {
                            expandString += !isNestedExpand ? "" : "(";
                            expandString += count > 1 ? "," + prop.Name : string.Concat("$expand=", prop.Name);
                            navPropNames.Add(navProp.Name);
                            expandString += GenerateExpandQueryStringInternal(navPropValue, model, true);
                        }
                        else
                        {
                            expandString = expandString.TrimEnd(')');
                            expandString += GenerateExpandQueryStringInternal(navPropValue, model, true);
                        }

                        expandString += !isNestedExpand ? "" : ")";
                    }
                }
            }

            return expandString;
        }
    }
}

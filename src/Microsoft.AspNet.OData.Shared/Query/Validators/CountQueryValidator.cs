//-----------------------------------------------------------------------------
// <copyright file="CountQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Query.Validators
{
    /// <summary>
    /// Represents a validator used to validate a <see cref="CountQueryOption"/> 
    /// based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class CountQueryValidator
    {
        private readonly DefaultQuerySettings _defaultQuerySettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="CountQueryValidator" /> class based on
        /// the <see cref="DefaultQuerySettings" />.
        /// </summary>
        /// <param name="defaultQuerySettings">The <see cref="DefaultQuerySettings" />.</param>
        public CountQueryValidator(DefaultQuerySettings defaultQuerySettings)
        {
            _defaultQuerySettings = defaultQuerySettings;
        }

        /// <summary>
        /// Validates a <see cref="CountQueryOption" />.
        /// </summary>
        /// <param name="countQueryOption">The $count query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        public virtual void Validate(CountQueryOption countQueryOption, ODataValidationSettings validationSettings)
        {
            if (countQueryOption == null)
            {
                throw Error.ArgumentNull("countQueryOption");
            }

            if (validationSettings == null)
            {
                throw Error.ArgumentNull("validationSettings");
            }

            ODataPath path = countQueryOption.Context.Path;

            if (path != null && path.Segments.Count > 0)
            {
                IEdmProperty property = countQueryOption.Context.TargetProperty;
                IEdmStructuredType structuredType = countQueryOption.Context.TargetStructuredType;
                string name = countQueryOption.Context.TargetName;
                if (EdmLibHelpers.IsNotCountable(property, structuredType,
                    countQueryOption.Context.Model,
                    _defaultQuerySettings.EnableCount))
                {
                    if (property == null)
                    {
                        throw new InvalidOperationException(Error.Format(
                            SRResources.NotCountableEntitySetUsedForCount,
                            name));
                    }
                    else
                    {
                        throw new InvalidOperationException(Error.Format(
                            SRResources.NotCountablePropertyUsedForCount,
                            name));
                    }
                }
            }
        }

        internal static CountQueryValidator GetCountQueryValidator(ODataQueryContext context)
        {
            if (context == null)
            {
                return new CountQueryValidator(new DefaultQuerySettings());
            }

            return context.RequestContainer == null
                ? new CountQueryValidator(context.DefaultQuerySettings)
                : context.RequestContainer.GetRequiredService<CountQueryValidator>();
        }
    }
}

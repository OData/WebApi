﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.OData.Edm;
using System.Linq;
using Microsoft.AspNetCore.OData.Formatter;

namespace Microsoft.AspNetCore.OData.Query.Validators
{
    /// <summary>
    /// Represents a validator used to validate a <see cref="CountQueryOption"/> 
    /// based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class CountQueryValidator
    {
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
                ODataPathSegment lastSegment = path.Segments.Last();

                if (lastSegment is CountPathSegment && path.Segments.Count > 1)
                {
                    ValidateCount(path.Segments[path.Segments.Count - 2], countQueryOption.Context.Model);
                }
                else
                {
                    ValidateCount(lastSegment, countQueryOption.Context.Model);
                }
            }
        }

        private static void ValidateCount(ODataPathSegment segment, IEdmModel model)
        {
            Contract.Assert(segment != null);
            Contract.Assert(model != null);

            NavigationPathSegment navigationPathSegment = segment as NavigationPathSegment;
            if (navigationPathSegment != null)
            {
                if (EdmLibHelpers.IsNotCountable(navigationPathSegment.NavigationProperty, model))
                {
                    throw new InvalidOperationException(Error.Format(
                        SRResources.NotCountablePropertyUsedForCount,
                        navigationPathSegment.NavigationPropertyName));
                }
                return;
            }

            PropertyAccessPathSegment propertyAccessPathSegment = segment as PropertyAccessPathSegment;
            if (propertyAccessPathSegment != null)
            {
                if (EdmLibHelpers.IsNotCountable(propertyAccessPathSegment.Property, model))
                {
                    throw new InvalidOperationException(Error.Format(
                        SRResources.NotCountablePropertyUsedForCount,
                        propertyAccessPathSegment.PropertyName));
                }
            }
        }
    }
}
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Formatter;
using System.Web.OData.Properties;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = System.Web.OData.Routing.ODataPath;

namespace System.Web.OData.Query.Validators
{
    /// <summary>
    /// Represents a validator used to validate a <see cref="CountQueryOption"/> 
    /// based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class CountQueryValidator
    {
        private readonly DefaultQuerySettings _defaultQuerySettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="CountQueryValidator" /> class.
        /// </summary>>
        public CountQueryValidator()
        {
            _defaultQuerySettings = new DefaultQuerySettings();
        }

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
                ODataPathSegment lastSegment = path.Segments.Last();

                if (lastSegment is CountSegment && path.Segments.Count > 1)
                {
                    ValidateCount(path.Segments[path.Segments.Count - 2], countQueryOption.Context.Model);
                }
                else
                {
                    ValidateCount(lastSegment, countQueryOption.Context.Model);
                }
            }
        }

        private void ValidateCount(ODataPathSegment segment, IEdmModel model)
        {
            Contract.Assert(segment != null);
            Contract.Assert(model != null);

            NavigationPropertySegment navigationPathSegment = segment as NavigationPropertySegment;
            if (navigationPathSegment != null)
            {
                if (EdmLibHelpers.IsNotCountable(navigationPathSegment.NavigationProperty,
                    navigationPathSegment.NavigationProperty.ToEntityType(), model,
                    _defaultQuerySettings.EnableCount))
                {
                    throw new InvalidOperationException(Error.Format(
                        SRResources.NotCountablePropertyUsedForCount,
                        navigationPathSegment.NavigationProperty.Name));
                }
                return;
            }

            PropertySegment propertyAccessPathSegment = segment as PropertySegment;
            if (propertyAccessPathSegment != null)
            {
                if (EdmLibHelpers.IsNotCountable(propertyAccessPathSegment.Property,
                    propertyAccessPathSegment.Property.Type.Definition as IEdmStructuredType, model,
                    _defaultQuerySettings.EnableCount))
                {
                    throw new InvalidOperationException(Error.Format(
                        SRResources.NotCountablePropertyUsedForCount,
                        propertyAccessPathSegment.Property.Name));
                }
            }

            EntitySetSegment entitySetSegment = segment as EntitySetSegment;
            if (entitySetSegment != null)
            {
                if (EdmLibHelpers.IsNotCountable(null, entitySetSegment.EntitySet.EntityType() as IEdmStructuredType,
                    model,
                    _defaultQuerySettings.EnableCount))
                {
                    throw new InvalidOperationException(Error.Format(
                        SRResources.NotCountableEntitySetUsedForCount,
                        entitySetSegment.EntitySet.Name));
                }
            }
        }
    }
}
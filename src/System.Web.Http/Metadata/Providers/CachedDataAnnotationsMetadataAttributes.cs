// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;

namespace System.Web.Http.Metadata.Providers
{
    // REVIEW: No access to HiddenInputAttribute
    public class CachedDataAnnotationsMetadataAttributes
    {
        public CachedDataAnnotationsMetadataAttributes(IEnumerable<Attribute> attributes)
        {
            CacheAttributes(attributes);
        }

        public DisplayAttribute Display { get; protected set; }

        public DisplayFormatAttribute DisplayFormat { get; protected set; }

        public EditableAttribute Editable { get; protected set; }

        public ReadOnlyAttribute ReadOnly { get; protected set; }

        private void CacheAttributes(IEnumerable<Attribute> attributes)
        {
            Display = attributes.OfType<DisplayAttribute>().FirstOrDefault();
            DisplayFormat = attributes.OfType<DisplayFormatAttribute>().FirstOrDefault();
            Editable = attributes.OfType<EditableAttribute>().FirstOrDefault();
            ReadOnly = attributes.OfType<ReadOnlyAttribute>().FirstOrDefault();
        }
    }
}

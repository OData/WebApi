// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

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

        // [SecuritySafeCritical] because it uses DataAnnotations type DisplayAttribute
        public DisplayAttribute Display { [SecuritySafeCritical] get; [SecuritySafeCritical] protected set; }

        // [SecuritySafeCritical] because it uses DataAnnotations type DisplayFormatAttribute
        public DisplayFormatAttribute DisplayFormat { [SecuritySafeCritical] get; [SecuritySafeCritical] protected set; }

        // [SecuritySafeCritical] because it uses DataAnnotations type EditableAttribute
        public EditableAttribute Editable { [SecuritySafeCritical] get; [SecuritySafeCritical] protected set; }

        public ReadOnlyAttribute ReadOnly { get; protected set; }

        // [SecuritySafeCritical] because it uses several DataAnnotations attribute types
        [SecuritySafeCritical]
        private void CacheAttributes(IEnumerable<Attribute> attributes)
        {
            Display = attributes.OfType<DisplayAttribute>().FirstOrDefault();
            DisplayFormat = attributes.OfType<DisplayFormatAttribute>().FirstOrDefault();
            Editable = attributes.OfType<EditableAttribute>().FirstOrDefault();
            ReadOnly = attributes.OfType<ReadOnlyAttribute>().FirstOrDefault();
        }
    }
}

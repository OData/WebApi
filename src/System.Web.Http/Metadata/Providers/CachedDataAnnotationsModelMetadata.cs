// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Web.Http.Internal;
using System.Web.Http.Properties;

namespace System.Web.Http.Metadata.Providers
{
    // REVIEW: No access to HiddenInputAttribute
    public class CachedDataAnnotationsModelMetadata : CachedModelMetadata<CachedDataAnnotationsMetadataAttributes>
    {
        public CachedDataAnnotationsModelMetadata(CachedDataAnnotationsModelMetadata prototype, Func<object> modelAccessor)
            : base(prototype, modelAccessor)
        {
        }

        public CachedDataAnnotationsModelMetadata(DataAnnotationsModelMetadataProvider provider, Type containerType, Type modelType, string propertyName, IEnumerable<Attribute> attributes)
            : base(provider, containerType, modelType, propertyName, new CachedDataAnnotationsMetadataAttributes(attributes))
        {
        }

        protected override bool ComputeConvertEmptyStringToNull()
        {
            return PrototypeCache.DisplayFormat != null
                       ? PrototypeCache.DisplayFormat.ConvertEmptyStringToNull
                       : base.ComputeConvertEmptyStringToNull();
        }

        protected override string ComputeDescription()
        {
            return PrototypeCache.Display != null
                       ? PrototypeCache.Display.GetDescription()
                       : base.ComputeDescription();
        }

        protected override bool ComputeIsReadOnly()
        {
            if (PrototypeCache.Editable != null)
            {
                return !PrototypeCache.Editable.AllowEdit;
            }

            if (PrototypeCache.ReadOnly != null)
            {
                return PrototypeCache.ReadOnly.IsReadOnly;
            }

            return base.ComputeIsReadOnly();
        }

        public override string GetDisplayName()
        {
            if (PrototypeCache.Display != null)
            {
                string name = PrototypeCache.Display.GetName();

                // if the user specified a display property but not the name we still fallback to the property name.
                if (name != null)
                {
                    return name;
                }
            }

            return base.GetDisplayName();
        }
    }
}

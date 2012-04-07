// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

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

        public CachedDataAnnotationsModelMetadata(CachedDataAnnotationsModelMetadataProvider provider, Type containerType, Type modelType, string propertyName, IEnumerable<Attribute> attributes)
            : base(provider, containerType, modelType, propertyName, new CachedDataAnnotationsMetadataAttributes(attributes.ToArray()))
        {
        }

        // [SecuritySafeCritical] because it uses DataAnnotations type
        [SecuritySafeCritical]
        protected override bool ComputeConvertEmptyStringToNull()
        {
            return PrototypeCache.DisplayFormat != null
                       ? PrototypeCache.DisplayFormat.ConvertEmptyStringToNull
                       : base.ComputeConvertEmptyStringToNull();
        }

        // [SecuritySafeCritical] because it uses DataAnnotations type
        [SecuritySafeCritical]
        protected override string ComputeDataTypeName()
        {
            if (PrototypeCache.DataType != null)
            {
                return PrototypeCache.DataType.ToDataTypeName();
            }

            if (PrototypeCache.DisplayFormat != null && !PrototypeCache.DisplayFormat.HtmlEncode)
            {
                return DataTypeUtil.HtmlTypeName;
            }

            return base.ComputeDataTypeName();
        }

        // [SecuritySafeCritical] because it uses DataAnnotations type
        [SecuritySafeCritical]
        protected override string ComputeDescription()
        {
            return PrototypeCache.Display != null
                       ? PrototypeCache.Display.GetDescription()
                       : base.ComputeDescription();
        }

        // [SecuritySafeCritical] because it uses DataAnnotations type
        [SecuritySafeCritical]
        protected override string ComputeDisplayFormatString()
        {
            return PrototypeCache.DisplayFormat != null
                       ? PrototypeCache.DisplayFormat.DataFormatString
                       : base.ComputeDisplayFormatString();
        }

        // [SecuritySafeCritical] because it uses DataAnnotations type
        [SecuritySafeCritical]
        protected override string ComputeDisplayName()
        {
            string result = null;

            if (PrototypeCache.Display != null)
            {
                result = PrototypeCache.Display.GetName();
            }

            if (result == null && PrototypeCache.DisplayName != null)
            {
                result = PrototypeCache.DisplayName.DisplayName;
            }

            return result ?? base.ComputeDisplayName();
        }

        // [SecuritySafeCritical] because it uses DataAnnotations type
        [SecuritySafeCritical]
        protected override string ComputeEditFormatString()
        {
            if (PrototypeCache.DisplayFormat != null && PrototypeCache.DisplayFormat.ApplyFormatInEditMode)
            {
                return PrototypeCache.DisplayFormat.DataFormatString;
            }

            return base.ComputeEditFormatString();
        }

        protected override bool ComputeHideSurroundingHtml()
        {
            return /*PrototypeCache.HiddenInput != null
                       ? !PrototypeCache.HiddenInput.DisplayValue
                       :*/ base.ComputeHideSurroundingHtml();
        }

        // [SecuritySafeCritical] because it uses DataAnnotations type EditableAttribute
        [SecuritySafeCritical]
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

        // [SecuritySafeCritical] because it uses DataAnnotations type RequiredAttribute
        [SecuritySafeCritical]
        protected override bool ComputeIsRequired()
        {
            return PrototypeCache.Required != null
                       ? true
                       : base.ComputeIsRequired();
        }

        // [SecuritySafeCritical] because it uses DataAnnotations type DisplayFormatAttribute
        [SecuritySafeCritical]
        protected override string ComputeNullDisplayText()
        {
            return PrototypeCache.DisplayFormat != null
                       ? PrototypeCache.DisplayFormat.NullDisplayText
                       : base.ComputeNullDisplayText();
        }

        // [SecuritySafeCritical] because it uses DataAnnotations type OrderAttribute
        [SecuritySafeCritical]
        protected override int ComputeOrder()
        {
            int? result = null;

            if (PrototypeCache.Display != null)
            {
                result = PrototypeCache.Display.GetOrder();
            }

            return result ?? base.ComputeOrder();
        }

        // [SecuritySafeCritical] because it uses DataAnnotations type DisplayAttribute
        [SecuritySafeCritical]
        protected override string ComputeShortDisplayName()
        {
            return PrototypeCache.Display != null
                       ? PrototypeCache.Display.GetShortName()
                       : base.ComputeShortDisplayName();
        }

        // [SecuritySafeCritical] because it uses DataAnnotations type ScaffoldColumnAttribute
        [SecuritySafeCritical]
        protected override bool ComputeShowForDisplay()
        {
            return PrototypeCache.ScaffoldColumn != null
                       ? PrototypeCache.ScaffoldColumn.Scaffold
                       : base.ComputeShowForDisplay();
        }

        // [SecuritySafeCritical] because it uses DataAnnotations type ScaffoldColumnAttribute
        [SecuritySafeCritical]
        protected override bool ComputeShowForEdit()
        {
            return PrototypeCache.ScaffoldColumn != null
                       ? PrototypeCache.ScaffoldColumn.Scaffold
                       : base.ComputeShowForEdit();
        }

        // [SecuritySafeCritical] because it uses DataAnnotations type DisplayColumnAttribute
        [SecuritySafeCritical]
        protected override string ComputeSimpleDisplayText()
        {
            if (Model != null)
            {
                if (PrototypeCache.DisplayColumn != null && !String.IsNullOrEmpty(PrototypeCache.DisplayColumn.DisplayColumn))
                {
                    PropertyInfo displayColumnProperty = GetPropertyInfoViaReflection(PrototypeCache.DisplayColumn.DisplayColumn);
                    ValidateDisplayColumnAttribute(PrototypeCache.DisplayColumn, displayColumnProperty, ModelType);

                    string simpleDisplayTextValue;
                    if (TryGetPropertyValueAsStringViaReflection(displayColumnProperty, out simpleDisplayTextValue))
                    {
                        return simpleDisplayTextValue;
                    }
                }
            }

            return base.ComputeSimpleDisplayText();
        }

        // [SecuritySafeCritical] because it uses DataAnnotations type UIHintAttribute
        [SecuritySafeCritical]
        protected override string ComputeTemplateHint()
        {
            if (PrototypeCache.UIHint != null)
            {
                return PrototypeCache.UIHint.UIHint;
            }
#if false
            if (PrototypeCache.HiddenInput != null)
            {
                return "HiddenInput";
            }
#endif
            return base.ComputeTemplateHint();
        }

        // [SecuritySafeCritical] because it uses DataAnnotations type DisplayAttribute
        [SecuritySafeCritical]
        protected override string ComputeWatermark()
        {
            return PrototypeCache.Display != null
                       ? PrototypeCache.Display.GetPrompt()
                       : base.ComputeWatermark();
        }

        // [SecuritySafeCritical] because it uses DataAnnotations type DisplayColumnAttribute
        [SecuritySafeCritical]
        private static void ValidateDisplayColumnAttribute(DisplayColumnAttribute displayColumnAttribute, PropertyInfo displayColumnProperty, Type modelType)
        {
            if (displayColumnProperty == null)
            {
                throw Error.InvalidOperation(SRResources.DataAnnotationsModelMetadataProvider_UnknownProperty, modelType, displayColumnAttribute.DisplayColumn);
            }
            if (displayColumnProperty.GetGetMethod() == null)
            {
                throw Error.InvalidOperation(SRResources.DataAnnotationsModelMetadataProvider_UnreadableProperty, modelType, displayColumnAttribute.DisplayColumn);
            }
        }

        // This method is [SecurityTransparent] and is used only so that we don't call Reflection from
        // a [SecuritySafeCritical] method.  Reflection works differently when called from [SecurityTransparent].
        private PropertyInfo GetPropertyInfoViaReflection(string propertyName)
        {
            return ModelType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
        }

        // This method is [SecurityTransparent] and is used only so that we don't call Reflection from
        // a [SecuritySafeCritical] method.  Reflection works differently when called from [SecurityTransparent].
        private bool TryGetPropertyValueAsStringViaReflection(PropertyInfo propertyInfo, out string valueAsString)
        {
            // PropertyInfo.GetValue() done here to avoid danger of the reflected method call under [SecuritySafeCritical]
            object value = propertyInfo.GetValue(Model, new object[0]);
            if (value == null)
            {
                valueAsString = null;
                return false;
            }

            valueAsString = value.ToString();
            return true;
        }
    }
}

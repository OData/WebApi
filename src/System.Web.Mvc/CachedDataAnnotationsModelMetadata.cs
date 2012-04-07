// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
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

        protected override bool ComputeConvertEmptyStringToNull()
        {
            return PrototypeCache.DisplayFormat != null
                       ? PrototypeCache.DisplayFormat.ConvertEmptyStringToNull
                       : base.ComputeConvertEmptyStringToNull();
        }

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

        protected override string ComputeDescription()
        {
            return PrototypeCache.Display != null
                       ? PrototypeCache.Display.GetDescription()
                       : base.ComputeDescription();
        }

        protected override string ComputeDisplayFormatString()
        {
            return PrototypeCache.DisplayFormat != null
                       ? PrototypeCache.DisplayFormat.DataFormatString
                       : base.ComputeDisplayFormatString();
        }

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
            return PrototypeCache.HiddenInput != null
                       ? !PrototypeCache.HiddenInput.DisplayValue
                       : base.ComputeHideSurroundingHtml();
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

        protected override bool ComputeIsRequired()
        {
            return PrototypeCache.Required != null
                       ? true
                       : base.ComputeIsRequired();
        }

        protected override string ComputeNullDisplayText()
        {
            return PrototypeCache.DisplayFormat != null
                       ? PrototypeCache.DisplayFormat.NullDisplayText
                       : base.ComputeNullDisplayText();
        }

        protected override int ComputeOrder()
        {
            int? result = null;

            if (PrototypeCache.Display != null)
            {
                result = PrototypeCache.Display.GetOrder();
            }

            return result ?? base.ComputeOrder();
        }

        protected override string ComputeShortDisplayName()
        {
            return PrototypeCache.Display != null
                       ? PrototypeCache.Display.GetShortName()
                       : base.ComputeShortDisplayName();
        }

        protected override bool ComputeShowForDisplay()
        {
            return PrototypeCache.ScaffoldColumn != null
                       ? PrototypeCache.ScaffoldColumn.Scaffold
                       : base.ComputeShowForDisplay();
        }

        protected override bool ComputeShowForEdit()
        {
            return PrototypeCache.ScaffoldColumn != null
                       ? PrototypeCache.ScaffoldColumn.Scaffold
                       : base.ComputeShowForEdit();
        }

        protected override string ComputeSimpleDisplayText()
        {
            if (Model != null)
            {
                if (PrototypeCache.DisplayColumn != null && !String.IsNullOrEmpty(PrototypeCache.DisplayColumn.DisplayColumn))
                {
                    PropertyInfo displayColumnProperty = ModelType.GetProperty(PrototypeCache.DisplayColumn.DisplayColumn, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
                    ValidateDisplayColumnAttribute(PrototypeCache.DisplayColumn, displayColumnProperty, ModelType);

                    object simpleDisplayTextValue = displayColumnProperty.GetValue(Model, new object[0]);
                    if (simpleDisplayTextValue != null)
                    {
                        return simpleDisplayTextValue.ToString();
                    }
                }
            }

            return base.ComputeSimpleDisplayText();
        }

        protected override string ComputeTemplateHint()
        {
            if (PrototypeCache.UIHint != null)
            {
                return PrototypeCache.UIHint.UIHint;
            }

            if (PrototypeCache.HiddenInput != null)
            {
                return "HiddenInput";
            }

            return base.ComputeTemplateHint();
        }

        protected override string ComputeWatermark()
        {
            return PrototypeCache.Display != null
                       ? PrototypeCache.Display.GetPrompt()
                       : base.ComputeWatermark();
        }

        private static void ValidateDisplayColumnAttribute(DisplayColumnAttribute displayColumnAttribute, PropertyInfo displayColumnProperty, Type modelType)
        {
            if (displayColumnProperty == null)
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.DataAnnotationsModelMetadataProvider_UnknownProperty,
                        modelType.FullName, displayColumnAttribute.DisplayColumn));
            }
            if (displayColumnProperty.GetGetMethod() == null)
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.DataAnnotationsModelMetadataProvider_UnreadableProperty,
                        modelType.FullName, displayColumnAttribute.DisplayColumn));
            }
        }
    }
}

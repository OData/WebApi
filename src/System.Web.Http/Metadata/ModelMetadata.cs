// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Web.Http.Internal;
using System.Web.Http.Validation;

namespace System.Web.Http.Metadata
{
    public class ModelMetadata
    {
        public const int DefaultOrder = 10000;

        private readonly Type _containerType;
        private readonly Type _modelType;
        private readonly string _propertyName;

        /// <summary>
        /// Explicit backing store for the things we want initialized by default, so don't have to call
        /// the protected virtual setters of an auto-generated property.
        /// </summary>
        private Dictionary<string, object> _additionalValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private bool _convertEmptyStringToNull = true;
        private bool _isRequired;
        private object _model;
        private Func<object> _modelAccessor;
        private int _order = DefaultOrder;
        private IEnumerable<ModelMetadata> _properties;
        private Type _realModelType;
        private bool _requestValidationEnabled = true;
        private bool _showForDisplay = true;
        private bool _showForEdit = true;
        private string _simpleDisplayText;

        public ModelMetadata(ModelMetadataProvider provider, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName)
        {
            if (provider == null)
            {
                throw Error.ArgumentNull("provider");
            }
            if (modelType == null)
            {
                throw Error.ArgumentNull("modelType");
            }

            Provider = provider;

            _containerType = containerType;
            _isRequired = !TypeHelper.TypeAllowsNullValue(modelType);
            _modelAccessor = modelAccessor;
            _modelType = modelType;
            _propertyName = propertyName;
        }

        public virtual Dictionary<string, object> AdditionalValues
        {
            get { return _additionalValues; }
        }

        public Type ContainerType
        {
            get { return _containerType; }
        }

        public virtual bool ConvertEmptyStringToNull
        {
            get { return _convertEmptyStringToNull; }
            set { _convertEmptyStringToNull = value; }
        }

        public virtual string DataTypeName { get; set; }

        public virtual string Description { get; set; }

        public virtual string DisplayFormatString { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "The method is a delegating helper to choose among multiple property values")]
        public virtual string DisplayName { get; set; }

        public virtual string EditFormatString { get; set; }

        public virtual bool HideSurroundingHtml { get; set; }

        public virtual bool IsComplexType
        {
            get { return !TypeDescriptor.GetConverter(ModelType).CanConvertFrom(typeof(string)); }
        }

        public bool IsNullableValueType
        {
            get { return TypeHelper.IsNullableValueType(ModelType); }
        }

        public virtual bool IsReadOnly { get; set; }

        public virtual bool IsRequired
        {
            get { return _isRequired; }
            set { _isRequired = value; }
        }

        public object Model
        {
            get
            {
                if (_modelAccessor != null)
                {
                    _model = _modelAccessor();
                    _modelAccessor = null;
                }
                return _model;
            }
            set
            {
                _model = value;
                _modelAccessor = null;
                _properties = null;
                _realModelType = null;
            }
        }

        public Type ModelType
        {
            get { return _modelType; }
        }

        public virtual string NullDisplayText { get; set; }

        public virtual int Order
        {
            get { return _order; }
            set { _order = value; }
        }

        public virtual IEnumerable<ModelMetadata> Properties
        {
            get
            {
                if (_properties == null)
                {
                    _properties = Provider.GetMetadataForProperties(Model, RealModelType).OrderBy(m => m.Order);
                }
                return _properties;
            }
        }

        public string PropertyName
        {
            get { return _propertyName; }
        }

        protected ModelMetadataProvider Provider { get; set; }

        internal Type RealModelType
        {
            get
            {
                if (_realModelType == null)
                {
                    _realModelType = ModelType;

                    // Don't call GetType() if the model is Nullable<T>, because it will
                    // turn Nullable<T> into T for non-null values
                    if (Model != null && !TypeHelper.IsNullableValueType(ModelType))
                    {
                        _realModelType = Model.GetType();
                    }
                }

                return _realModelType;
            }
        }

        public virtual bool RequestValidationEnabled
        {
            get { return _requestValidationEnabled; }
            set { _requestValidationEnabled = value; }
        }

        public virtual string ShortDisplayName { get; set; }

        public virtual bool ShowForDisplay
        {
            get { return _showForDisplay; }
            set { _showForDisplay = value; }
        }

        public virtual bool ShowForEdit
        {
            get { return _showForEdit; }
            set { _showForEdit = value; }
        }

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "This property delegates to the method when the user has not yet set a simple display text value.")]
        public virtual string SimpleDisplayText
        {
            get
            {
                if (_simpleDisplayText == null)
                {
                    _simpleDisplayText = GetSimpleDisplayText();
                }
                return _simpleDisplayText;
            }
            set { _simpleDisplayText = value; }
        }

        public virtual string TemplateHint { get; set; }

        public virtual string Watermark { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "The method is a delegating helper to choose among multiple property values")]
        public string GetDisplayName()
        {
            return DisplayName ?? PropertyName ?? ModelType.Name;
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method is used to resolve the simple display text when it was not explicitly set through other means.")]
        protected virtual string GetSimpleDisplayText()
        {
            if (Model == null)
            {
                return NullDisplayText;
            }

            string toStringResult = Convert.ToString(Model, CultureInfo.CurrentCulture);
            if (toStringResult == null)
            {
                return String.Empty;
            }

            if (!toStringResult.Equals(Model.GetType().FullName, StringComparison.Ordinal))
            {
                return toStringResult;
            }

            ModelMetadata firstProperty = Properties.FirstOrDefault();
            if (firstProperty == null)
            {
                return String.Empty;
            }

            if (firstProperty.Model == null)
            {
                return firstProperty.NullDisplayText;
            }

            return Convert.ToString(firstProperty.Model, CultureInfo.CurrentCulture);
        }

        public virtual IEnumerable<ModelValidator> GetValidators(IEnumerable<ModelValidatorProvider> validatorProviders)
        {
            if (validatorProviders == null)
            {
                throw Error.ArgumentNull("validatorProviders");
            }

            return validatorProviders.SelectMany(provider => provider.GetValidators(this, validatorProviders));
        }
    }
}

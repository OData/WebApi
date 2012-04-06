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
        private readonly Type _containerType;
        private readonly Type _modelType;
        private readonly string _propertyName;

        /// <summary>
        /// Explicit backing store for the things we want initialized by default, so don't have to call
        /// the protected virtual setters of an auto-generated property.
        /// </summary>
        private Dictionary<string, object> _additionalValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private bool _convertEmptyStringToNull = true;
        private object _model;
        private Func<object> _modelAccessor;
        private IEnumerable<ModelMetadata> _properties;
        private Type _realModelType;

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

        public virtual string Description { get; set; }

        public virtual bool IsComplexType
        {
            get { return !TypeDescriptor.GetConverter(ModelType).CanConvertFrom(typeof(string)); }
        }

        public bool IsNullableValueType
        {
            get { return TypeHelper.IsNullableValueType(ModelType); }
        }

        public virtual bool IsReadOnly { get; set; }

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

        public virtual IEnumerable<ModelMetadata> Properties
        {
            get
            {
                if (_properties == null)
                {
                    _properties = Provider.GetMetadataForProperties(Model, RealModelType);
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

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "The method is a delegating helper to choose among multiple property values")]
        public string GetDisplayName()
        {
            return PropertyName ?? ModelType.Name;
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

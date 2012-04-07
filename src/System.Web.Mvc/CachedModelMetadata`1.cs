// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    // This class assumes that model metadata is expensive to create, and allows the user to
    // stash a cache object that can be copied around as a prototype to make creation and
    // computation quicker. It delegates the retrieval of values to getter methods, the results
    // of which are cached on a per-metadata-instance basis.
    //
    // This allows flexible caching strategies: either caching the source of information across
    // instances or caching of the actual information itself, depending on what the developer
    // decides to put into the prototype cache.
    public abstract class CachedModelMetadata<TPrototypeCache> : ModelMetadata
    {
        private bool _convertEmptyStringToNull;
        private string _dataTypeName;
        private string _description;
        private string _displayFormatString;
        private string _displayName;
        private string _editFormatString;
        private bool _hideSurroundingHtml;
        private bool _isReadOnly;
        private bool _isRequired;
        private string _nullDisplayText;
        private int _order;
        private string _shortDisplayName;
        private bool _showForDisplay;
        private bool _showForEdit;
        private string _templateHint;
        private string _watermark;

        private bool _convertEmptyStringToNullComputed;
        private bool _dataTypeNameComputed;
        private bool _descriptionComputed;
        private bool _displayFormatStringComputed;
        private bool _displayNameComputed;
        private bool _editFormatStringComputed;
        private bool _hideSurroundingHtmlComputed;
        private bool _isReadOnlyComputed;
        private bool _isRequiredComputed;
        private bool _nullDisplayTextComputed;
        private bool _orderComputed;
        private bool _shortDisplayNameComputed;
        private bool _showForDisplayComputed;
        private bool _showForEditComputed;
        private bool _templateHintComputed;
        private bool _watermarkComputed;

        // Constructor for creating real instances of the metadata class based on a prototype
        protected CachedModelMetadata(CachedModelMetadata<TPrototypeCache> prototype, Func<object> modelAccessor)
            : base(prototype.Provider, prototype.ContainerType, modelAccessor, prototype.ModelType, prototype.PropertyName)
        {
            PrototypeCache = prototype.PrototypeCache;
        }

        // Constructor for creating the prototype instances of the metadata class
        protected CachedModelMetadata(CachedDataAnnotationsModelMetadataProvider provider, Type containerType, Type modelType, string propertyName, TPrototypeCache prototypeCache)
            : base(provider, containerType, null /* modelAccessor */, modelType, propertyName)
        {
            PrototypeCache = prototypeCache;
        }

        public sealed override bool ConvertEmptyStringToNull
        {
            get
            {
                return CacheOrCompute(ComputeConvertEmptyStringToNull,
                                      ref _convertEmptyStringToNull,
                                      ref _convertEmptyStringToNullComputed);
            }
            set
            {
                _convertEmptyStringToNull = value;
                _convertEmptyStringToNullComputed = true;
            }
        }

        public sealed override string DataTypeName
        {
            get
            {
                return CacheOrCompute(ComputeDataTypeName,
                                      ref _dataTypeName,
                                      ref _dataTypeNameComputed);
            }
            set
            {
                _dataTypeName = value;
                _dataTypeNameComputed = true;
            }
        }

        public sealed override string Description
        {
            get
            {
                return CacheOrCompute(ComputeDescription,
                                      ref _description,
                                      ref _descriptionComputed);
            }
            set
            {
                _description = value;
                _descriptionComputed = true;
            }
        }

        public sealed override string DisplayFormatString
        {
            get
            {
                return CacheOrCompute(ComputeDisplayFormatString,
                                      ref _displayFormatString,
                                      ref _displayFormatStringComputed);
            }
            set
            {
                _displayFormatString = value;
                _displayFormatStringComputed = true;
            }
        }

        public sealed override string DisplayName
        {
            get
            {
                return CacheOrCompute(ComputeDisplayName,
                                      ref _displayName,
                                      ref _displayNameComputed);
            }
            set
            {
                _displayName = value;
                _displayNameComputed = true;
            }
        }

        public sealed override string EditFormatString
        {
            get
            {
                return CacheOrCompute(ComputeEditFormatString,
                                      ref _editFormatString,
                                      ref _editFormatStringComputed);
            }
            set
            {
                _editFormatString = value;
                _editFormatStringComputed = true;
            }
        }

        public sealed override bool HideSurroundingHtml
        {
            get
            {
                return CacheOrCompute(ComputeHideSurroundingHtml,
                                      ref _hideSurroundingHtml,
                                      ref _hideSurroundingHtmlComputed);
            }
            set
            {
                _hideSurroundingHtml = value;
                _hideSurroundingHtmlComputed = true;
            }
        }

        public sealed override bool IsReadOnly
        {
            get
            {
                return CacheOrCompute(ComputeIsReadOnly,
                                      ref _isReadOnly,
                                      ref _isReadOnlyComputed);
            }
            set
            {
                _isReadOnly = value;
                _isReadOnlyComputed = true;
            }
        }

        public sealed override bool IsRequired
        {
            get
            {
                return CacheOrCompute(ComputeIsRequired,
                                      ref _isRequired,
                                      ref _isRequiredComputed);
            }
            set
            {
                _isRequired = value;
                _isRequiredComputed = true;
            }
        }

        public sealed override string NullDisplayText
        {
            get
            {
                return CacheOrCompute(ComputeNullDisplayText,
                                      ref _nullDisplayText,
                                      ref _nullDisplayTextComputed);
            }
            set
            {
                _nullDisplayText = value;
                _nullDisplayTextComputed = true;
            }
        }

        public sealed override int Order
        {
            get
            {
                return CacheOrCompute(ComputeOrder,
                                      ref _order,
                                      ref _orderComputed);
            }
            set
            {
                _order = value;
                _orderComputed = true;
            }
        }

        protected TPrototypeCache PrototypeCache { get; set; }

        public sealed override string ShortDisplayName
        {
            get
            {
                return CacheOrCompute(ComputeShortDisplayName,
                                      ref _shortDisplayName,
                                      ref _shortDisplayNameComputed);
            }
            set
            {
                _shortDisplayName = value;
                _shortDisplayNameComputed = true;
            }
        }

        public sealed override bool ShowForDisplay
        {
            get
            {
                return CacheOrCompute(ComputeShowForDisplay,
                                      ref _showForDisplay,
                                      ref _showForDisplayComputed);
            }
            set
            {
                _showForDisplay = value;
                _showForDisplayComputed = true;
            }
        }

        public sealed override bool ShowForEdit
        {
            get
            {
                return CacheOrCompute(ComputeShowForEdit,
                                      ref _showForEdit,
                                      ref _showForEditComputed);
            }
            set
            {
                _showForEdit = value;
                _showForEditComputed = true;
            }
        }

        public sealed override string SimpleDisplayText
        {
            get
            {
                // This is already cached in the base class with an appropriate override available
                return base.SimpleDisplayText;
            }
            set { base.SimpleDisplayText = value; }
        }

        public sealed override string TemplateHint
        {
            get
            {
                return CacheOrCompute(ComputeTemplateHint,
                                      ref _templateHint,
                                      ref _templateHintComputed);
            }
            set
            {
                _templateHint = value;
                _templateHintComputed = true;
            }
        }

        public sealed override string Watermark
        {
            get
            {
                return CacheOrCompute(ComputeWatermark,
                                      ref _watermark,
                                      ref _watermarkComputed);
            }
            set
            {
                _watermark = value;
                _watermarkComputed = true;
            }
        }

        private static TResult CacheOrCompute<TResult>(Func<TResult> computeThunk, ref TResult value, ref bool computed)
        {
            if (!computed)
            {
                value = computeThunk();
                computed = true;
            }

            return value;
        }

        protected virtual bool ComputeConvertEmptyStringToNull()
        {
            return base.ConvertEmptyStringToNull;
        }

        protected virtual string ComputeDataTypeName()
        {
            return base.DataTypeName;
        }

        protected virtual string ComputeDescription()
        {
            return base.Description;
        }

        protected virtual string ComputeDisplayFormatString()
        {
            return base.DisplayFormatString;
        }

        protected virtual string ComputeDisplayName()
        {
            return base.DisplayName;
        }

        protected virtual string ComputeEditFormatString()
        {
            return base.EditFormatString;
        }

        protected virtual bool ComputeHideSurroundingHtml()
        {
            return base.HideSurroundingHtml;
        }

        protected virtual bool ComputeIsReadOnly()
        {
            return base.IsReadOnly;
        }

        protected virtual bool ComputeIsRequired()
        {
            return base.IsRequired;
        }

        protected virtual string ComputeNullDisplayText()
        {
            return base.NullDisplayText;
        }

        protected virtual int ComputeOrder()
        {
            return base.Order;
        }

        protected virtual string ComputeShortDisplayName()
        {
            return base.ShortDisplayName;
        }

        protected virtual bool ComputeShowForDisplay()
        {
            return base.ShowForDisplay;
        }

        protected virtual bool ComputeShowForEdit()
        {
            return base.ShowForEdit;
        }

        protected virtual string ComputeSimpleDisplayText()
        {
            return base.GetSimpleDisplayText();
        }

        protected virtual string ComputeTemplateHint()
        {
            return base.TemplateHint;
        }

        protected virtual string ComputeWatermark()
        {
            return base.Watermark;
        }

        protected sealed override string GetSimpleDisplayText()
        {
            // Rename for consistency
            return ComputeSimpleDisplayText();
        }
    }
}

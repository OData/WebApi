// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.Metadata.Providers
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
        private string _description;
        private bool _isReadOnly;
        private bool _isComplexType;

        private bool _convertEmptyStringToNullComputed;
        private bool _descriptionComputed;
        private bool _isReadOnlyComputed;
        private bool _isComplexTypeComputed;

        // Constructor for creating real instances of the metadata class based on a prototype
        protected CachedModelMetadata(CachedModelMetadata<TPrototypeCache> prototype, Func<object> modelAccessor)
            : base(prototype.Provider, prototype.ContainerType, modelAccessor, prototype.ModelType, prototype.PropertyName)
        {
            PrototypeCache = prototype.PrototypeCache;

            _isComplexType = prototype.IsComplexType;
            _isComplexTypeComputed = true;
        }

        // Constructor for creating the prototype instances of the metadata class
        protected CachedModelMetadata(DataAnnotationsModelMetadataProvider provider, Type containerType, Type modelType, string propertyName, TPrototypeCache prototypeCache)
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

        public sealed override bool IsComplexType
        {
            get
            {
                return CacheOrCompute(ComputeIsComplexType,
                                      ref _isComplexType,
                                      ref _isComplexTypeComputed);
            }
        }

        protected TPrototypeCache PrototypeCache { get; set; }

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

        protected virtual string ComputeDescription()
        {
            return base.Description;
        }

        protected virtual bool ComputeIsReadOnly()
        {
            return base.IsReadOnly;
        }

        protected virtual bool ComputeIsComplexType()
        {
            return base.IsComplexType;
        }
    }
}

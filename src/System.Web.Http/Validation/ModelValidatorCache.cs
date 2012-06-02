// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Http.Metadata;

namespace System.Web.Http.Validation
{
    /// <summary>
    /// Defines a cache for <see cref="ModelValidator"/>s. This cache is keyed on the type or property that the metadata is associated with.
    /// </summary>
    internal class ModelValidatorCache : IModelValidatorCache, IDisposable
    {
        private ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim();
        private Dictionary<Tuple<Type, string>, ModelValidator[]> _validatorCache = new Dictionary<Tuple<Type, string>, ModelValidator[]>();
        private Lazy<IEnumerable<ModelValidatorProvider>> _validatorProviders;

        public ModelValidatorCache(Lazy<IEnumerable<ModelValidatorProvider>> validatorProviders)
        {
            _validatorProviders = validatorProviders;
        }

        public IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata)
        {
            // If metadata is for a property then containerType != null && propertyName != null
            // If metadata is for a type then containerType == null && propertyName == null, so we have to use modelType for the cache key.
            Type typeForCache = metadata.ContainerType ?? metadata.ModelType;
            Tuple<Type, string> cacheKey = Tuple.Create(typeForCache, metadata.PropertyName);

            ModelValidator[] validators;
            if (!TryGetValue(cacheKey, out validators))
            {
                _cacheLock.EnterWriteLock();
                try
                {
                    // Check the cache again in case the value was computed and added to the cache while we were waiting on the write lock
                    if (!_validatorCache.TryGetValue(cacheKey, out validators))
                    {
                        // Compute validators
                        validators = metadata.GetValidators(_validatorProviders.Value).ToArray();
                        _validatorCache.Add(cacheKey, validators);
                    }
                }
                finally
                {
                    _cacheLock.ExitWriteLock();
                }
            }
            return validators;
        }

        internal bool TryGetValue(Tuple<Type, string> cacheKey, out ModelValidator[] validators)
        {
            if (_cacheLock.TryEnterReadLock(0))
            {
                try
                {
                    return _validatorCache.TryGetValue(cacheKey, out validators);
                }
                finally
                {
                    _cacheLock.ExitReadLock();
                }
            }
            validators = null;
            return false;
        }

        void IDisposable.Dispose()
        {
            _cacheLock.Dispose();
        }
    }
}

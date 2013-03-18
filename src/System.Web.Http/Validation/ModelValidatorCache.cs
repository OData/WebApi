// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Http.Metadata;

namespace System.Web.Http.Validation
{
    /// <summary>
    /// Defines a cache for <see cref="ModelValidator"/>s. This cache is keyed on the type or property that the metadata is associated with.
    /// </summary>
    internal class ModelValidatorCache : IModelValidatorCache
    {
        private ConcurrentDictionary<Tuple<Type, string>, ModelValidator[]> _validatorCache = new ConcurrentDictionary<Tuple<Type, string>, ModelValidator[]>();
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
            if (!_validatorCache.TryGetValue(cacheKey, out validators))
            {
                // Compute validators
                // There are no side-effects if the same validators are created more than once
                validators = metadata.GetValidators(_validatorProviders.Value).ToArray();
                _validatorCache.TryAdd(cacheKey, validators);
            }
            return validators;
        }
    }
}

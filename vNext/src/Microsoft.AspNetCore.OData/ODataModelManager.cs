// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// 
    /// </summary>
    public class ODataModelManager : IODataModelManger
    {
        private IDictionary<string, IEdmModel> _edmModels;

        private const string NullPrifix = "NULL_PREFIX";
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataModelManager"/> class.
        /// </summary>
        public ODataModelManager()
        {
            _edmModels = new ConcurrentDictionary<string, IEdmModel>();
        }

        /// <summary>
        /// Gets the EDM model from the key.
        /// </summary>
        /// <param name="key"></param>
        public IEdmModel GetModel(string key)
        {
            string newKey = key;
            if (key == null)
            {
                newKey = NullPrifix;
            }

            IEdmModel value;
            if (_edmModels.TryGetValue(newKey, out value))
            {
                return value;
            }

            return null;
        }

        public void AddModel(string key, IEdmModel model)
        {
            string newKey = key;
            if (key == null)
            {
                newKey = NullPrifix;
            }

            _edmModels[newKey] = model;
        }
    }
}

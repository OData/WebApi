// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using Microsoft.Data.Edm;

namespace System.Web.Http
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpConfigurationExtensions
    {
        private const string EdmModelKey = "MS_EdmModel";

        /// <summary>
        /// Retrieve the EdmModel from the configuration Properties collection. Null if user has not set it.
        /// </summary>
        /// <param name="configuration">Configuration to be updated.</param>
        /// <returns>Returns an EdmModel for this configuration</returns>
        public static IEdmModel GetEdmModel(this HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            // returns one if user sets one, null otherwise
            object result;
            if (configuration.Properties.TryGetValue(EdmModelKey, out result))
            {
                return result as IEdmModel;
            }

            return null;
        }

        /// <summary>
        /// Sets the given EdmModel with the configuration.
        /// </summary>
        /// <param name="configuration">Configuration to be updated.</param>
        /// <param name="model">The EdmModel to update.</param>
        public static void SetEdmModel(this HttpConfiguration configuration, IEdmModel model)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            configuration.Properties.AddOrUpdate(EdmModelKey, model, (a, b) =>
                {
                    return model;
                });
        }
    }
}

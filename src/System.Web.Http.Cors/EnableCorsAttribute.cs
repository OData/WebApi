// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Cors;

namespace System.Web.Http.Cors
{
    /// <summary>
    /// This class defines an attribute that can be applied to an action or a controller to enable CORS.
    /// By default, it allows all origins, methods and headers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class EnableCorsAttribute : Attribute, ICorsPolicyProvider
    {
        private CorsPolicy _corsPolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnableCorsAttribute"/> class.
        /// </summary>
        public EnableCorsAttribute()
        {
            _corsPolicy = new CorsPolicy();
            _corsPolicy.AllowAnyHeader = true;
            _corsPolicy.AllowAnyMethod = true;
            _corsPolicy.AllowAnyOrigin = true;
        }

        /// <summary>
        /// Gets or sets the headers that the resource might use and can be exposed.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">value</exception>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Attributes can contain properties that return arrays.")]
        public string[] ExposedHeaders
        {
            get
            {
                return _corsPolicy.ExposedHeaders.ToArray();
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _corsPolicy.ExposedHeaders.Clear();
                for (int i = 0; i < value.Length; i++)
                {
                    _corsPolicy.ExposedHeaders.Add(value[i]);
                }
            }
        }

        /// <summary>
        /// Gets or sets the headers that are supported by the resource.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">value</exception>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Attributes can contain properties that return arrays.")]
        public string[] Headers
        {
            get
            {
                return _corsPolicy.Headers.ToArray();
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _corsPolicy.Headers.Clear();
                for (int i = 0; i < value.Length; i++)
                {
                    _corsPolicy.Headers.Add(value[i]);
                }
                _corsPolicy.AllowAnyHeader = false;
            }
        }

        /// <summary>
        /// Gets or sets the methods that are supported by the resource.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">value</exception>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Attributes can contain properties that return arrays.")]
        public string[] Methods
        {
            get
            {
                return _corsPolicy.Methods.ToArray();
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _corsPolicy.Methods.Clear();
                for (int i = 0; i < value.Length; i++)
                {
                    _corsPolicy.Methods.Add(value[i]);
                }
                _corsPolicy.AllowAnyMethod = false;
            }
        }

        /// <summary>
        /// Gets or sets the origins that are allowed to access the resource.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">value</exception>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Attributes can contain properties that return arrays.")]
        public string[] Origins
        {
            get
            {
                return _corsPolicy.Origins.ToArray();
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _corsPolicy.Origins.Clear();
                for (int i = 0; i < value.Length; i++)
                {
                    _corsPolicy.Origins.Add(value[i]);
                }
                _corsPolicy.AllowAnyOrigin = false;
            }
        }

        /// <summary>
        /// Gets or sets the number of seconds the results of a preflight request can be cached.
        /// </summary>
        public long PreflightMaxAge
        {
            get
            {
                return _corsPolicy.PreflightMaxAge ?? -1;
            }
            set
            {
                _corsPolicy.PreflightMaxAge = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the resource supports user credentials in the request.
        /// </summary>
        public bool SupportsCredentials
        {
            get
            {
                return _corsPolicy.SupportsCredentials;
            }
            set
            {
                _corsPolicy.SupportsCredentials = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="CorsPolicy" />.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        /// The <see cref="CorsPolicy" />.
        /// </returns>
        public Task<CorsPolicy> GetCorsPolicyAsync(HttpRequestMessage request)
        {
            return Task.FromResult(_corsPolicy);
        }
    }
}
// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Http.Internal;
using System.Web.Http.ModelBinding;

namespace System.Web.Http.Controllers
{
    public abstract class HttpParameterDescriptor
    {
        private readonly ConcurrentDictionary<object, object> _properties = new ConcurrentDictionary<object, object>();

        private ModelBinderAttribute _modelBinderAttribute;
        private bool _searchedModelBinderAttribute;
        private HttpConfiguration _configuration;
        private HttpActionDescriptor _actionDescriptor;

        protected HttpParameterDescriptor()
        {
        }

        protected HttpParameterDescriptor(HttpActionDescriptor actionDescriptor)
        {
            if (actionDescriptor == null)
            {
                throw Error.ArgumentNull("actionDescriptor");
            }

            _actionDescriptor = actionDescriptor;
            _configuration = _actionDescriptor.Configuration;
        }

        public HttpConfiguration Configuration
        {
            get { return _configuration; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _configuration = value;
            }
        }

        public HttpActionDescriptor ActionDescriptor
        {
            get { return _actionDescriptor; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _actionDescriptor = value;
            }
        }

        /// <summary>
        /// Gets the properties associated with this instance.
        /// </summary>
        public ConcurrentDictionary<object, object> Properties
        {
            get { return _properties; }
        }

        public virtual object DefaultValue
        {
            get { return null; }
        }

        public abstract string ParameterName { get; }

        public abstract Type ParameterType { get; }

        public virtual string Prefix
        {
            get
            {
                ModelBinderAttribute attribute = ModelBinderAttribute;
                return attribute != null && !String.IsNullOrEmpty(attribute.Name)
                           ? attribute.Name
                           : null;
            }
        }

        public virtual ModelBinderAttribute ModelBinderAttribute
        {
            get
            {
                if (_modelBinderAttribute == null)
                {
                    if (!_searchedModelBinderAttribute)
                    {
                        _searchedModelBinderAttribute = true;
                        _modelBinderAttribute = FindModelBinderAttribute();
                    }
                }

                return _modelBinderAttribute;
            }

            set { _modelBinderAttribute = value; }
        }

        public virtual Collection<T> GetCustomAttributes<T>() where T : class
        {
            return new Collection<T>();
        }

        private ModelBinderAttribute FindModelBinderAttribute()
        {
            // Can be on parameter itself or on the parameter's type.  Nearest wins.
            return GetCustomAttributes<ModelBinderAttribute>().SingleOrDefault()
                ?? ParameterType.GetCustomAttributes<ModelBinderAttribute>(false).SingleOrDefault();
        }
    }
}

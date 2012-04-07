// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Reflection;
using System.Web.Http.Internal;

namespace System.Web.Http.Controllers
{
    public class ReflectedHttpParameterDescriptor : HttpParameterDescriptor
    {
        private ParameterInfo _parameterInfo;

        public ReflectedHttpParameterDescriptor(HttpActionDescriptor actionDescriptor, ParameterInfo parameterInfo)
            : base(actionDescriptor)
        {
            if (parameterInfo == null)
            {
                throw Error.ArgumentNull("parameterInfo");
            }

            ParameterInfo = parameterInfo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectedHttpParameterDescriptor"/> class.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        public ReflectedHttpParameterDescriptor()
        {
        }

        public override object DefaultValue
        {
            get
            {
                object value;
                if (ParameterInfo.TryGetDefaultValue(out value))
                {
                    return value;
                }
                else
                {
                    return base.DefaultValue;
                }
            }
        }

        public ParameterInfo ParameterInfo
        {
            get { return _parameterInfo; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _parameterInfo = value;
            }
        }

        public override string ParameterName
        {
            get { return ParameterInfo.Name; }
        }

        public override Type ParameterType
        {
            get { return ParameterInfo.ParameterType; }
        }

        public override Collection<T> GetCustomAttributes<T>()
        {
            return new Collection<T>(ParameterInfo.GetCustomAttributes<T>(inherit: false));
        }
    }
}

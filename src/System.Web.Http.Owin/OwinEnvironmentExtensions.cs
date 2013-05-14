// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Web.Http.Owin.Properties;

namespace System.Web.Http.Owin
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class OwinEnvironmentExtensions
    {
        public static T GetOwinValue<T>(this IDictionary<string, object> environment, string key)
        {
            Contract.Assert(environment != null);
            Contract.Assert(key != null);

            object value;
            if (environment.TryGetValue(key, out value))
            {
                if (value is T)
                {
                    return (T)value;
                }
                throw Error.InvalidOperation(OwinResources.GetOwinValue_IncorrectType, key, typeof(T).Name);
            }
            throw Error.InvalidOperation(OwinResources.GetOwinValue_MissingRequiredValue, key);
        }
    }
}
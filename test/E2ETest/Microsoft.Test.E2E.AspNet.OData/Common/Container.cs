//-----------------------------------------------------------------------------
// <copyright file="Container.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Client;

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    public class Container : DataServiceContext
    {
        public Container(Uri serviceRoot) :
            this(serviceRoot, ODataProtocolVersion.V4)
        {
        }

        public Container(Uri serviceRoot, ODataProtocolVersion protocolVersion)
            : base(serviceRoot, protocolVersion)
        {
            this.ResolveName = new Func<Type, string>(this.ResolveNameFromType);
            this.ResolveType = new Func<string, Type>(this.ResolveTypeFromName);
        }

        protected Type ResolveTypeFromName(string typeName)
        {
            var assemblyName = typeName.Substring(0, typeName.IndexOf("."));
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName);
            if (assembly != null)
            {
                return assembly.GetType(typeName.Insert(typeName.LastIndexOf('.') + 1, "Client."));
            }

            return this.GetType().Assembly.GetType(typeName.Insert(typeName.LastIndexOf('.') + 1, "Client."));
        }

        protected string ResolveNameFromType(Type clientType)
        {
            return clientType.FullName.Replace(".Client", string.Empty);
        }
    }
}

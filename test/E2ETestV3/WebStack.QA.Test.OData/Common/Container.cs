using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Linq;
using Microsoft.Data.Edm;

namespace WebStack.QA.Test.OData.Common
{
    public class Container : DataServiceContext
    {
        public Container(Uri serviceRoot) :
            this(serviceRoot, DataServiceProtocolVersion.V3)
        {
        }

        public Container(Uri serviceRoot, DataServiceProtocolVersion protocolVersion) : base(serviceRoot, protocolVersion)
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

        protected string ResolveNameFromType(global::System.Type clientType)
        {
            return clientType.FullName.Replace(".Client", string.Empty);
        }
    }
}

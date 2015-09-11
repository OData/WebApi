using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.OData.Query.Expressions
{
    /// <summary>
    /// Definition for generate types
    /// </summary>
    internal class TypeDefinition
    {
        public TypeDefinition()
        {
            this.Name = "Dynamic_" + Guid.NewGuid().ToString();
            this.Properties = new Dictionary<string, Type>();
        }

        public string Name { get; private set; }

        public IDictionary<string, Type> Properties
        {
            get; set;
        }

        public TypeDefinition Clone()
        {
            var result = new TypeDefinition();
            result.Properties = this.Properties.ToList().ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return result;
        }
    }
}

using System;
using System.Reflection;

namespace WebStack.QA.Test.OData.Common
{
    internal sealed class TypesInjectionAssembly : Assembly
    {
        private Type[] _types;

        public TypesInjectionAssembly(params Type[] types)
        {
            _types = types;
        }

        public override Type[] GetTypes()
        {
            return _types;
        }
    }
}

using System.Collections.Generic;
using System.Reflection;


namespace System.Web.OData
{
    /// <summary>
    /// Allows client to tell OData which are the custom aggregation methods defined.
    /// In order to do it it must receive a methodToken - that is the full identifier
    /// of the method in the OData URL - and an IDictionary that maps the input type
    /// of the aggregation method to it's MethodInfo.
    /// </summary>
    public class CustomAggregateMethodAnnotation 
    {

        private Dictionary<string, IDictionary<Type, MethodInfo>> _tokenToMethodMap 
            = new Dictionary<string, IDictionary<Type, MethodInfo>>();

        /// <summary>
        /// CustomAggregateMethodAnnotation simple constructor.
        /// </summary>
        public CustomAggregateMethodAnnotation() { }

        /// <summary>
        /// Adds all implementations of a method that share the same methodToken.
        /// </summary>
        public CustomAggregateMethodAnnotation AddMethod(string methodToken, IDictionary<Type, MethodInfo> methods)
        {
            _tokenToMethodMap.Add(methodToken, methods);
            return this;
        }

        /// <summary>
        /// Get an implementation of a method with the specifies returnType and methodToken.
        /// If there's no method that matches the requirements, returns null.
        /// </summary>
        public bool GetMethodInfo(string methodToken, Type returnType, out MethodInfo method)
        {
            IDictionary<Type, MethodInfo> methodWrapper;
            method = null;

            if (_tokenToMethodMap.TryGetValue(methodToken, out methodWrapper))
            {
                return methodWrapper.TryGetValue(returnType, out method);
            }

            return false;
        }
    }
}

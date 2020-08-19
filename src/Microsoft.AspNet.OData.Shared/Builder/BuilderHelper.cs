using Microsoft.AspNet.OData.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Helper Method for Builder
    /// </summary>
    public static class BuilderHelper
    {
        /// <summary>
        /// Common method for IsAssignableFrom
        /// </summary>
        /// /// <param name="expectedType">Expected CLR Type</param>
        /// <param name="type"> CLR Type</param>
        /// <param name="customTypeDescription"> Custom Type description</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static void ValidateAssignableFrom(Type expectedType, Type type, string customTypeDescription = null)
        {
            if(!expectedType.IsAssignableFrom(type))
            {
                throw Error.Argument("propertyInfo", SRResources.ArgumentMustBeOfType,
                   customTypeDescription ?? expectedType.FullName);
            }
        }
    }
}

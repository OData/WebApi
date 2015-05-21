using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;
using System;

namespace Microsoft.AspNet.OData.Extensions
{
    public static class EdmModelExtensions
    {
        public static IEdmType GetEdmType(this IEdmModel model, Type clrType)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            return model.FindDeclaredType(clrType.EdmFullName());
        }
    }
}
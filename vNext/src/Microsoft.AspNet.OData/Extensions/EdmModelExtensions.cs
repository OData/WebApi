using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData;
using Microsoft.OData.Edm.Library;

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
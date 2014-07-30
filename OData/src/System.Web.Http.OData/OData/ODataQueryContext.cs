// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData
{
    /// <summary>
    /// This defines some context information used to perform query composition. 
    /// </summary>
    public class ODataQueryContext
    {
        /// <summary>
        /// Constructs an instance of <see cref="ODataQueryContext"/> with <see cref="IEdmModel" /> and element CLR type. 
        /// </summary>
        /// <param name="model">The EdmModel that includes the <see cref="IEdmType"/> corresponding to the given <paramref name="elementClrType"/>.</param>
        /// <param name="elementClrType">The CLR type of the element of the collection being queried.</param>
        public ODataQueryContext(IEdmModel model, Type elementClrType)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (elementClrType == null)
            {
                throw Error.ArgumentNull("elementClrType");
            }

            ElementType = model.GetEdmType(elementClrType);

            if (ElementType == null)
            {
                throw Error.Argument("elementClrType", SRResources.ClrTypeNotInModel, elementClrType.FullName);
            }

            ElementClrType = elementClrType;
            Model = model;
        }

        /// <summary>
        /// Constructs an instance of <see cref="ODataQueryContext"/> with <see cref="IEdmModel" /> and element CLR type.
        /// </summary>
        /// <param name="model">The EDM model the given EDM type belongs to.</param>
        /// <param name="elementType">The EDM type of the element of the collection being queried.</param>
        public ODataQueryContext(IEdmModel model, IEdmType elementType)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }
            if (elementType == null)
            {
                throw Error.ArgumentNull("elementType");
            }

            Model = model;
            ElementType = elementType;
        }

        /// <summary>
        /// Gets the given <see cref="IEdmModel"/> that contains the EntitySet.
        /// </summary>
        public IEdmModel Model { get; private set; }

        /// <summary>
        /// Gets the <see cref="IEdmType"/> of the element.
        /// </summary>
        public IEdmType ElementType { get; private set; }

        /// <summary>
        /// Gets the CLR type of the element.
        /// </summary>
        public Type ElementClrType { get; private set; }
    }
}

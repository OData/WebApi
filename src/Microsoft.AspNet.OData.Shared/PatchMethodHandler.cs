﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Handler Class to handle users methods for create, delete and update.
    /// This is the handler for data modification where there is a CLR type.
    /// </summary>
    public abstract class PatchMethodHandler<TStructuralType>: IPatchMethodHandler where TStructuralType : class
    {
        /// <summary>
        /// TryCreate method to create a new object.
        /// </summary>        
        /// <param name="patchObject">The Delta<typeparamref name="TStructuralType"/> object to be patched over original object. Optional to patch</param>
        /// <param name="createdObject">The created object (CLR or Typeless)</param>
        /// <param name="errorMessage">Any error message in case of an exception</param>
        /// <returns>The status of the TryCreate method <see cref="PatchStatus"/> </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        public abstract PatchStatus TryCreate(Delta<TStructuralType> patchObject, out TStructuralType createdObject, out string errorMessage);

        /// <summary>
        /// TryGet method which tries to get the Origignal object based on a keyvalues.
        /// </summary>
        /// <param name="keyValues">Key value pair for the entity keys</param>        
        /// <param name="originalObject">Object to return</param>
        /// <param name="errorMessage">Any error message in case of an exception</param>
        /// <returns>The status of the TryGet method <see cref="PatchStatus"/> </returns>
        public abstract PatchStatus TryGet(IDictionary<string, object> keyValues, out TStructuralType originalObject, out string errorMessage);

        /// <summary>
        /// TryDelete Method which will delete the object based on keyvalue pairs.
        /// </summary>
        /// <param name="keyValues"></param>
        /// <param name="errorMessage"></param>
        /// <returns>The status of the TryGet method <see cref="PatchStatus"/> </returns>
        public abstract PatchStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage);

        /// <summary>
        /// Get the PatchHandler for the nested type
        /// </summary>
        /// <param name="parent">Parent instance.</param>
        /// <param name="navigationPropertyName">The name of the navigation property for the handler</param>
        /// <returns>The type of Nested PatchMethodHandler</returns>
        public abstract IPatchMethodHandler GetNestedPatchHandler(TStructuralType parent, string navigationPropertyName);
    }
}
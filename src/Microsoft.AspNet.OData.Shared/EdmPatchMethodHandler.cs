﻿using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNet.OData
{    
   
    /// <summary>
    /// Handler Class to handle users methods for create, delete and update
    /// </summary>
    public abstract class EdmPatchMethodHandler
    {
        /// <summary>
        /// TryCreate method to create a new object.
        /// </summary>
        /// <param name="changedObject">Changed object which can be appied on creted object, optional</param>
        /// <param name="createdObject">The created object (Typeless)</param>
        /// <param name="errorMessage">Any error message in case of an exception</param>
        /// <returns>The status of the TryCreate Method, statuses are <see cref="PatchStatus"/></returns>
        public abstract PatchStatus TryCreate(IEdmChangedObject changedObject, out IEdmStructuredObject createdObject, out string errorMessage);

        /// <summary>
        ///  TryGet method which tries to get the Origignal object based on a keyvalues.
        /// </summary>
        /// <param name="keyValues">Key value pair for the entity keys</param>        
        /// <param name="originalObject">Object to return</param>
        /// <param name="errorMessage">Any error message in case of an exception</param>
        /// <returns>The status of the TryGet Method, statuses are <see cref="PatchStatus"/></returns>
        public abstract PatchStatus TryGet(IDictionary<string, object> keyValues, out IEdmStructuredObject originalObject, out string errorMessage);

        /// <summary>
        ///  TryDelete Method which will delete the object based on keyvalue pairs.
        /// </summary>
        /// <param name="keyValues"></param>
        /// <param name="errorMessage"></param>
        /// <returns>The status of the TryDelete Method, statuses are <see cref="PatchStatus"/></returns>
        public abstract PatchStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage);

        /// <summary>
        /// Get the PatchHandler for the nested type
        /// </summary>
        /// <param name="parent">Parent instance.</param>
        /// <param name="navigationPropertyName">The name of the navigation property for the handler</param>
        /// <returns>Nested Patch Method handler for the navigation property</returns>
        public abstract EdmPatchMethodHandler GetNestedPatchHandler(IEdmStructuredObject parent, string navigationPropertyName);
    }
}

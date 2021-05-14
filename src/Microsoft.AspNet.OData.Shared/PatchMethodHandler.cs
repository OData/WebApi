using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNet.OData
{    
    /// <summary>
    /// Base Interface for PatchMethodHandler
    /// </summary>
    public interface IPatchMethodHandler
    {

    }

    /// <summary>
    /// Handler Class to handle users methods for create, delete and update
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


    /// <summary>
    /// Enum for Patch Status
    /// </summary>
    public enum PatchStatus
    {
        /// <summary>
        /// Success Status
        /// </summary>
        Success,
        /// <summary>
        /// Failure Status
        /// </summary>
        Failure,
        /// <summary>
        /// Resource Not Found
        /// </summary>
        NotFound
    }

}

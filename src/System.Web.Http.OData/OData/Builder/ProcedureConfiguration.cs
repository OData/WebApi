// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// Represents a Procedure that is exposed in the model
    /// </summary>
    public abstract class ProcedureConfiguration
    {
        /// <summary>
        /// The Name of the procedure
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// The parameters the procedure takes
        /// </summary>
        public abstract IEnumerable<ParameterConfiguration> Parameters { get; }

        /// <summary>
        /// The type returned when the procedure is invoked.
        /// </summary>
        public IEdmTypeConfiguration ReturnType { get; set; }

        /// <summary>
        /// The EntitySet that entities are returned from.
        /// </summary>
        public EntitySetConfiguration EntitySet { get; set; }

        /// <summary>
        /// The Kind of procedure, which can be either Action, Function or ServiceOperation
        /// </summary>
        public abstract ProcedureKind Kind { get; }

        /// <summary>
        /// The qualified name of the procedure when used in OData urls.
        /// Qualification is required to distinguish the procedure from other possible single part identifiers.
        /// </summary>
        public string ContainerQualifiedName
        {
            get { return ModelBuilder.ContainerName + "." + Name; }
        }

        /// <summary>
        /// The FullyQualifiedName is the ContainerQualifiedName further qualified using the Namespace.
        /// Typically this is not required, because most services have at most one container with the same name.
        /// </summary>
        public string FullyQualifiedName
        {
            get { return ModelBuilder.Namespace + "." + ContainerQualifiedName; }
        }

        /// <summary>
        /// The FullName is the ContainerQualifiedName.
        /// </summary>
        public string FullName
        {
            get { return ContainerQualifiedName; }
        }

        /// <summary>
        /// Can the procedure be composed upon.
        /// 
        /// For example can a URL that invokes the procedure be used as the base url for 
        /// a request that invokes the procedure and does something else with the results
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Copies existing spelling used in EdmLib.")]
        public virtual bool IsComposable
        {
            get { return false; }
        }

        /// <summary>
        /// Can the procedure be bound to a url representing the BindingParameter.
        /// </summary>
        public virtual bool IsBindable
        {
            get { return false; }
        }

        /// <summary>
        /// If the procedure IsBindable is it Always bindable.
        /// </summary>
        public virtual bool IsAlwaysBindable
        {
            get { return IsBindable; }
        }

        /// <summary>
        /// Does the procedure have side-effects.
        /// </summary>
        public virtual bool IsSideEffecting
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets the <see cref="ODataModelBuilder"/> used to create this configuration.
        /// </summary>
        protected ODataModelBuilder ModelBuilder { get; set; }
    }
}

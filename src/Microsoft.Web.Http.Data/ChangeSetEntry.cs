// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.Web.Http.Data
{
    /// <summary>
    /// Represents a change operation to be performed on an entity.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("Operation = {Operation}, Type = {Entity.GetType().Name}")]
    public sealed class ChangeSetEntry
    {
        /// <summary>
        /// Gets or sets the client ID for the entity
        /// </summary>
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ChangeOperation"/> to be performed on the entity.
        /// </summary>
        [DataMember]
        public ChangeOperation Operation { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Entity"/> being operated on
        /// </summary>
        [DataMember]
        public object Entity { get; set; }

        /// <summary>
        /// Gets or sets the original state of the entity being operated on
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public object OriginalEntity { get; set; }

        /// <summary>
        /// Gets or sets the state of the entity in the data store
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public object StoreEntity { get; set; }

        /// <summary>
        /// Gets or sets the custom methods invoked on the entity, as a set
        /// of method name / parameter set pairs.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IDictionary<string, object[]> EntityActions { get; set; }

        /// <summary>
        /// Gets or sets the validation errors encountered during the processing of the operation. 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IEnumerable<ValidationResultInfo> ValidationErrors { get; set; }

        /// <summary>
        /// Gets or sets the collection of members in conflict. The <see cref="StoreEntity"/> property
        /// contains the current store value for each member in conflict.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IEnumerable<string> ConflictMembers { get; set; }

        /// <summary>
        /// Gets or sets whether the conflict is a delete conflict, meaning the
        /// entity no longer exists in the store.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsDeleteConflict { get; set; }

        /// <summary>
        /// Gets or sets the collection of IDs of the associated entities for
        /// each association of the Entity
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IDictionary<string, int[]> Associations { get; set; }

        /// <summary>
        /// Gets or sets the collection of IDs for each association of the <see cref="OriginalEntity"/>
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IDictionary<string, int[]> OriginalAssociations { get; set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ChangeSetEntry"/> contains conflicts.
        /// </summary>
        public bool HasConflict
        {
            get { return (IsDeleteConflict || (ConflictMembers != null && ConflictMembers.Any())); }
        }

        public bool HasError
        {
            get { return HasConflict || (ValidationErrors != null && ValidationErrors.Any()); }
        }

        internal UpdateActionDescriptor ActionDescriptor { get; set; }
    }
}

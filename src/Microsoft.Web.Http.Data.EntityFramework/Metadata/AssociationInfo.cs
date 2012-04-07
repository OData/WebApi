// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Web.Http.Data.EntityFramework.Metadata
{
    /// <summary>
    /// Information about an Association
    /// </summary>
    internal sealed class AssociationInfo
    {
        /// <summary>
        /// The name of the association
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The key members on the FK side of the association
        /// </summary>
        public string[] ThisKey { get; set; }

        /// <summary>
        /// The key members on the non-FK side of the association
        /// </summary>
        public string[] OtherKey { get; set; }

        /// <summary>
        /// The foreign key role name for this association
        /// </summary>
        public string FKRole { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this association can have a
        /// multiplicity of zero
        /// </summary>
        public bool IsRequired { get; set; }
    }
}

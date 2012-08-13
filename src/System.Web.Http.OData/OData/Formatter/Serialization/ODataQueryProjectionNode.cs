// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// An ODataQueryProjectionNode represents the $expand and $select part of the OData query. The $expand and $select part of an OData query can be
    /// represented by a root ODataQueryProjectionNode (with name ""). Each ODataQueryProjectionNode in the Selects collection are the primitive/complex nodes/properties 
    /// that need to be written to the message and each ODataQueryProjectionNode in Expands collection are the navigations that need to be expanded in the message.
    /// </summary>
    public class ODataQueryProjectionNode
    {
        private string _name;
        private IEdmType _nodeType;

        public ODataQueryProjectionNode()
        {
            Selects = new Collection<ODataQueryProjectionNode>();
            Expands = new Collection<ODataQueryProjectionNode>();
        }

        /// <summary>
        /// Gets or Sets the Name of the Property. Root Node has an empty name by convention.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _name = value;
            }
        }

        /// <summary>
        /// The <see cref="IEdmType" /> of this property.
        /// </summary>
        public IEdmType NodeType
        {
            get
            {
                return _nodeType;
            }

            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _nodeType = value;
            }
        }

        /// <summary>
        /// The list of properties that need to be selected for this entity. An empty collection signifies that all properties 
        /// need to be selected.
        /// </summary>
        public Collection<ODataQueryProjectionNode> Selects { get; private set; }

        /// <summary>
        /// The list of navigations that need to be expanded for this entity.
        /// </summary>
        public Collection<ODataQueryProjectionNode> Expands { get; private set; }
    }
}

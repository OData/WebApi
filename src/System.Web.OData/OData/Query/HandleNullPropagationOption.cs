// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.OData.Query
{
    /// <summary>
    /// This enum defines how to handle null propagation in queryable support.
    /// </summary>
    public enum HandleNullPropagationOption
    {
        /// <summary>
        /// Determine how to handle null propagation based on the
        /// query provider during query composition.  This is the
        /// default value used in <see cref="ODataQuerySettings"/>
        /// </summary>
        Default = 0,

        /// <summary>
        /// Handle null propagation during query composition.
        /// </summary>
        True = 1,

        /// <summary>
        /// Do not handle null propagation during query composition.
        /// </summary>
        False = 2
    }
}
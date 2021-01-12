// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{

    /// <summary>
    /// Represents a Message Type
    /// </summary>
    public class MessageType
    {

    }

    /// <summary>
    /// Represents an Exception Type
    /// </summary>
    public abstract class ExceptionType
    {
        /// <summary>
        /// Represents a MessageType
        /// </summary>
        protected MessageType MessageType { get; set; }
    }

    /// <summary>
    /// Represents an Exception for Data modification Operation.
    /// </summary>    
    public class DataModificationExceptionType : ExceptionType
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="DataModificationExceptionType"/> class.
        /// </summary>
        public DataModificationExceptionType(DataModificationOperationKind failedOperation)
        {
            this.FailedOperation = failedOperation;
        }

        /// <summary>
        /// Represents king of <see cref="DataModificationOperationKind"/> type of operation 
        /// </summary>
        public DataModificationOperationKind FailedOperation { get; set; }

        /// <summary>
        /// Represents response code
        /// </summary>
        public Int16 ResponseCode { get; set; }
    }

    /// <summary>
    /// Enumerates the DataModificationOperation for the operation kind
    /// </summary>
    public enum DataModificationOperationKind
    {
        /// <summary>
        /// Insert new Instance
        /// </summary>
        Insert,

        /// <summary>
        /// Update existing Instance
        /// </summary>
        Update,

        /// <summary>
        /// Insert new instance or update it if it already exists
        /// </summary>
        Upsert,

        /// <summary>
        /// Delete existing instance
        /// </summary>
        Delete,

        /// <summary>
        /// Invoke action or function
        /// </summary>
        Invoke,

        /// <summary>
        /// Add link between entities
        /// </summary>
        Link,

        /// <summary>
        /// Remove link between entities
        /// </summary>
        Unlink
    }
}
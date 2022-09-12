//-----------------------------------------------------------------------------
// <copyright file="MessageType.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Org.OData.Core.V1
{
    /// <summary>
    /// Represents a message type.
    /// </summary>
    public class MessageType
    {
        /// <summary>
        /// Code of message.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Actual message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Severity of message.
        /// </summary>
        public string Severity { get; set; }

        /// <summary>
        /// Target of message.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Details of message.
        /// </summary>
        public string Details { get; set; }
    }

    /// <summary>
    /// Represents an exception type.
    /// </summary>
    public abstract class ExceptionType
    {
        /// <summary>
        /// Represents a message type.
        /// </summary>
        public MessageType MessageType { get; set; }
    }

    /// <summary>
    /// Represents the type of exception thrown during a data modification operation.
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
        /// Represents the kind of data modification operation.
        /// </summary>
        public DataModificationOperationKind FailedOperation { get; }

        /// <summary>
        /// Represents the response code.
        /// </summary>
        public Int16 ResponseCode { get; set; }
    }

    /// <summary>
    /// Enumerates the kind of data modification operations.
    /// </summary>
    public enum DataModificationOperationKind
    {
        /// <summary>
        /// Represents an insert operation.
        /// </summary>
        Insert,

        /// <summary>
        /// Represents an update operation.
        /// </summary>
        Update,

        /// <summary>
        /// Represents an insert or update operation if item already exists.
        /// </summary>
        Upsert,

        /// <summary>
        /// Represents a delete data modification operation.
        /// </summary>
        Delete,

        /// <summary>
        /// Represents an action or function invocation.
        /// </summary>
        Invoke,

        /// <summary>
        /// Represents adding a link between entities.
        /// </summary>
        Link,

        /// <summary>
        /// Represents removing a link between entities.
        /// </summary>
        Unlink
    }
}

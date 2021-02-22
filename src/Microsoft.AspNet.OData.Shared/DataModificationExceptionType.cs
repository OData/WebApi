// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Builder;

namespace Org.OData.Core.V1
{
    /// <summary>
    /// Represents a Message Type
    /// </summary>
    public class MessageType
    {
        /// <summary>
        /// Code of message
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Actual message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Severity of message
        /// </summary>
        public string Severity { get; set; }

        /// <summary>
        /// Target of message
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Details of message
        /// </summary>
        public string Details { get; set; }
    }

    /// <summary>
    /// Represents an Exception Type
    /// </summary>
    public abstract class ExceptionType
    {
        /// <summary>
        /// Instanciates a type of <see cref="ExceptionType"/>
        /// </summary>
        public ExceptionType()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntityType<DataModificationExceptionType>();
      
            builder.Namespace = typeof(DataModificationExceptionType).Namespace;
        }

        /// <summary>
        /// Represents a MessageType
        /// </summary>
        public MessageType MessageType { get; set; }
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
            this.failedOperation = failedOperation;
        }

        /// <summary>
        /// Represents king of <see cref="DataModificationOperationKind"/> type of operation 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "failed")]
        public DataModificationOperationKind failedOperation { get; set; }

        /// <summary>
        /// Represents response code
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "response")]
        public Int16 responseCode { get; set; }
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
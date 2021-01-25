// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Org.OData.Core.V1
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
        /// Instanciates a type of <see cref="ExceptionType"/>
        /// </summary>
        public ExceptionType()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntityType<DataModificationExceptionType>();
      
            builder.Namespace = typeof(DataModificationExceptionType).Namespace;

            Model = builder.GetEdmModel();

            //((EdmEnumObject)graph).Value;
            //new ODataEnumValue(value, enumType.FullName());


            //ODataModelBuilder builder = new ODataModelBuilder();
            //ComplexTypeConfiguration<DataModificationExceptionType> dataException = builder.ComplexType<DataModificationExceptionType>();

            //dataException.EnumProperty<DataModificationOperationKind>(c => c.FailedOperation);
            //EnumTypeConfiguration<DataModificationOperationKind> operationKind = builder.EnumType<DataModificationOperationKind>();
            //operationKind.Member(DataModificationOperationKind.Delete);
            //operationKind.Member(DataModificationOperationKind.Insert);
            //operationKind.Member(DataModificationOperationKind.Invoke);
            //operationKind.Member(DataModificationOperationKind.Link);
            //operationKind.Member(DataModificationOperationKind.Unlink);
            //operationKind.Member(DataModificationOperationKind.Update);
            //operationKind.Member(DataModificationOperationKind.Upsert);

            //dataException.ComplexProperty<MessageType>(c => c.MessageType);
            //dataException.Property(c => c.ResponseCode);

            //builder.Namespace = typeof(DataModificationExceptionType).Namespace;
            //Model = builder.GetEdmModel();
        }

        /// <summary>
        /// Represents a MessageType
        /// </summary>
        protected MessageType MessageType { get; set; }

        /// <summary>
        /// Stores the Model
        /// </summary>
        public static IEdmModel Model { get; set; }

        ///// <summary>
        ///// Get Edm Type
        ///// </summary>
        ///// <returns></returns>
        //public IEdmTypeReference GetEdmType()
        //{
        //    return Model.GetEdmTypeReference(typeof(DataModificationExceptionType));
        //}

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        //internal ODataResourceValue AddAnno(DataModificationExceptionType value)
        //{
        //    ODataResourceValue resourceValue = new ODataResourceValue { TypeName = "Org.OData.Core.V1.DataModificationExceptionType" };

        //    List<ODataProperty> properties = new List<ODataProperty>();
        //    properties.Add(new ODataProperty {Name= "FailedOperation", Value = new ODataEnumValue(value.failedOperation.ToString(), "FailedOperation") });

        //    resourceValue.Properties = properties;

        //    return resourceValue;
        //}
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
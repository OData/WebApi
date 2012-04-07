// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Http;

namespace Microsoft.Web.Http.Data
{
    /// <summary>
    /// The data contract of an error that has occurred 
    /// during the execution of an operation on the server.
    /// This is sent back along with the action 
    /// result(s) to the client.
    /// </summary>
    [DataContract]
    public sealed class ValidationResultInfo : IEquatable<ValidationResultInfo>
    {
        /// <summary>
        /// Constructor accepting a localized error message and and collection 
        /// of the names of the members the error originated from.
        /// </summary>
        /// <param name="message">The localized message</param>
        /// <param name="sourceMemberNames">A collection of the names of the members the error originated from.</param>
        public ValidationResultInfo(string message, IEnumerable<string> sourceMemberNames)
        {
            if (message == null)
            {
                throw Error.ArgumentNull("message");
            }

            if (sourceMemberNames == null)
            {
                throw Error.ArgumentNull("sourceMemberNames");
            }

            Message = message;
            SourceMemberNames = sourceMemberNames;
        }

        /// <summary>
        /// Constructor accepting a localized error message, error code, optional stack trace,
        /// and collection of the names of the members the error originated from.
        /// </summary>
        /// <param name="message">The localized error message</param>
        /// <param name="errorCode">The custom error code</param>
        /// <param name="stackTrace">The error stack trace</param>
        /// <param name="sourceMemberNames">A collection of the names of the members the error originated from.</param>
        public ValidationResultInfo(string message, int errorCode, string stackTrace, IEnumerable<string> sourceMemberNames)
        {
            if (message == null)
            {
                throw Error.ArgumentNull("message");
            }

            if (sourceMemberNames == null)
            {
                throw Error.ArgumentNull("sourceMemberNames");
            }

            Message = message;
            ErrorCode = errorCode;
            StackTrace = stackTrace;
            SourceMemberNames = sourceMemberNames;
        }

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets custom error code
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the stack trace of the error
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string StackTrace { get; set; }

        /// <summary>
        /// Gets or sets the names of the members the error originated from.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IEnumerable<string> SourceMemberNames { get; set; }

        /// <summary>
        /// Returns the hash code for this object.
        /// </summary>
        /// <returns>The hash code for this object.</returns>
        public override int GetHashCode()
        {
            return Message.GetHashCode();
        }

        #region IEquatable<ValidationResultInfo> Members

        /// <summary>
        /// Test the current instance against the specified instance for equality
        /// </summary>
        /// <param name="other">The ValidationResultInfo to compare to</param>
        /// <returns>True if the instances are equal, false otherwise</returns>
        bool IEquatable<ValidationResultInfo>.Equals(ValidationResultInfo other)
        {
            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }
            if (Object.ReferenceEquals(null, other))
            {
                return false;
            }
            return ((Message == other.Message) &&
                    (ErrorCode == other.ErrorCode) &&
                    (StackTrace == other.StackTrace) &&
                    (SourceMemberNames.SequenceEqual(other.SourceMemberNames)));
        }

        #endregion
    }
}

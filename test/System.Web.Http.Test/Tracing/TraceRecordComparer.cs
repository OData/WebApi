// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Http.Tracing
{
    /// <summary>
    /// Comparer class to allow xUnit asserts for <see cref="TraceRecord"/>.
    /// </summary>
    class TraceRecordComparer : IEqualityComparer<TraceRecord>
    {
        public bool Equals(TraceRecord x, TraceRecord y)
        {
            if (!String.Equals(x.Category, y.Category) ||
                   x.Level != y.Level ||
                   x.Kind != y.Kind ||
                   !Object.ReferenceEquals(x.Request, y.Request))
                return false;

            // The following must match only if they are present on 'x' -- the expected value
            if (x.Exception != null && !Object.ReferenceEquals(x.Exception, y.Exception))
                return false;

            if (!String.IsNullOrEmpty(x.Message) && !String.Equals(x.Message, y.Message))
                return false;

            if (!String.IsNullOrEmpty(x.Operation) && !String.Equals(x.Operation, y.Operation))
                return false;

            if (!String.IsNullOrEmpty(x.Operator) && !String.Equals(x.Operator, y.Operator))
                return false;

            return true;
        }

        public int GetHashCode(TraceRecord obj)
        {
            return obj.GetHashCode();
        }
    }
}

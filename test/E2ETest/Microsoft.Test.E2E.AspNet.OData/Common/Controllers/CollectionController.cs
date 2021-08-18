//-----------------------------------------------------------------------------
// <copyright file="CollectionController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers.TypeLibrary;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Controllers
{
    public class CollectionController : ApiController
    {
        #region IEnumerable, IEnumerable<T>
        [AcceptVerbs("PUT", "POST", "DELETE")]
        public IEnumerable EchoIEnumerable(IEnumerable input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public IEnumerable<string> EchoIEnumerableOfString(IEnumerable<string> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public IEnumerable<Address> EchoIEnumerableOfAddress(IEnumerable<Address> input)
        {
            return input;
        }
        #endregion

        #region IList, IList<T>, List<T>
        [AcceptVerbs("PUT", "POST", "DELETE")]
        public IList EchoIList(IList input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public IList<int?> EchoIListNullableInt(IList<int?> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<object> EchoListOfObject(List<object> input)
        {
            return input;
        }
        #endregion

        #region Array
        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Guid[] ReverseGuidArray(Guid[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public double?[] ReverseNullableDoubleArray(double?[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Nullable<DateTime>[] ReverseNullableDateTimeArray(Nullable<DateTime>[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Person[] ReversePersonArray(Person[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }
        #endregion

        #region IDictionary, Dictionary<K,V>
        [AcceptVerbs("PUT", "POST", "DELETE")]
        public IDictionary EchoIDictionary(IDictionary input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public IDictionary<string, Person> EchoIDictionaryOfStringAndPerson(IDictionary<string, Person> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Dictionary<int, string> EchoListOfObject(Dictionary<int, string> input)
        {
            return input;
        }
        #endregion
    }
}

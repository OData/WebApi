//-----------------------------------------------------------------------------
// <copyright file="ODataAcceptHeaderTestSet.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    public class ODataAcceptHeaderTestSet
    {
        private static ODataAcceptHeaderTestSet _singleton = new ODataAcceptHeaderTestSet();
        private List<string> _allAcceptHeaders;

        public static ODataAcceptHeaderTestSet GetInstance()
        {
            return _singleton;
        }

        private ODataAcceptHeaderTestSet()
        {
            _allAcceptHeaders = new List<string>();
            _allAcceptHeaders.Add("application/json;odata.metadata=full");
            _allAcceptHeaders.Add("application/json;odata.metadata=full;odata.streaming=true");
            _allAcceptHeaders.Add("application/json;odata.metadata=full;odata.streaming=false");
            _allAcceptHeaders.Add("application/json;odata.metadata=minimal");
            _allAcceptHeaders.Add("application/json;odata.metadata=minimal;odata.streaming=true");
            _allAcceptHeaders.Add("application/json;odata.metadata=minimal;odata.streaming=false");
            _allAcceptHeaders.Add("application/json;odata.metadata=none");
            _allAcceptHeaders.Add("application/json;odata.metadata=none;odata.streaming=true");
            _allAcceptHeaders.Add("application/json;odata.metadata=none;odata.streaming=false");
            _allAcceptHeaders.Add("application/json");
            _allAcceptHeaders.Add("application/json;odata.streaming=true");
            _allAcceptHeaders.Add("application/json;odata.streaming=false");
        }

        public TheoryDataSet<string> GetAllAcceptHeaders()
        {
            var retval = new TheoryDataSet<string>();

            _allAcceptHeaders.ForEach(a => retval.Add(a));

            return retval;
        }

        public string[] GetAllAcceptHeadersInArray()
        {
            return _allAcceptHeaders.ToArray();
        }
    }
}
